using BitacoraAlfipac.Data;
using BitacoraAlfipac.Documents;
using BitacoraAlfipac.Models.Entidades;
using BitacoraAlfipac.Models.ViewModels;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System.Text;

namespace BitacoraAlfipac.Controllers
{
    [Authorize]
    public class BitacoraDespachosController : Controller
    {

        private readonly ApplicationDbContext _context;

        public BitacoraDespachosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
        DateTime? fecha,
        string? contenedor,
        string? marchamos,
        string? cliente,
        string? chasis)
            {
            var fechaSeleccionada = fecha ?? DateTime.Today;

            var inicio = fechaSeleccionada.Date;
            var fin = inicio.AddDays(1);

            var lista = await _context.BitacoraDespachos
                .Where(x => x.FechaHoraDespacho >= inicio && x.FechaHoraDespacho < fin)
                .OrderBy(i => i.FechaHoraDespacho)
                .ToListAsync();

            var vm = new BitacoraDespachosViewModel
            {
                Despachos = lista,
                FechaSeleccionada = fechaSeleccionada,
                FechaHoraDespacho = DateTime.Now,

                // 👇 AUTO-RELLENO
                Contenedor = contenedor ?? "",
                Marchamos = marchamos ?? "",
                Informacion = cliente ?? "" // opcional aquí
            };

            return View(vm);
        }

        //Crear despacho
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(BitacoraDespachosViewModel vm)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", new { fecha = vm.FechaHoraDespacho.Date });

            var contenedorBuscado = vm.Contenedor.ToUpper().Trim();

            object? entidad = null;
            string tipo = "";
            string? ubicacion = null;

            // 🔎 Buscar unidad SOLO si no es furgón
            if (!vm.EsSalidaEnFurgon)
            {
                var resultado = await BuscarUnidadGlobal(contenedorBuscado);
                entidad = resultado.entidad;
                tipo = resultado.tipo;
                ubicacion = resultado.ubicacion;

                if (entidad == null)
                {
                    TempData["Error"] = "La unidad no existe. Si es salida en furgón, debe marcar la opción.";
                    return RedirectToAction("Index", new { fecha = vm.FechaHoraDespacho.Date });
                }

                // 🛟 BACKUP SOLO PARA CONTENEDORES
                if (tipo == "CONTENEDOR")
                {
                    var inventario = (IContenedorInventario)entidad;

                    inventario.Marchamos = vm.Marchamos;

                    var backup = new ContenedorBackupDespacho
                    {
                        Contenedor = inventario.Contenedor,
                        PatioOrigen = ubicacion ?? "",
                        Marchamos = inventario.Marchamos ?? "",
                        Estado = inventario.EstadoCarga ?? "",
                        Tamaño = inventario.Tamano ?? "",
                        Transportista = inventario.Transportista ?? "",
                        Cliente = inventario.Cliente ?? "",
                        Chasis = inventario.Chasis ?? "",
                        FechaRespaldo = DateTime.Now
                    };

                    _context.ContenedoresBackupDespacho.Add(backup);
                }
            }

            // 📝 CREAR DESPACHO
            var despacho = new BitacoraDespacho
            {
                Contenedor = contenedorBuscado,
                Marchamos = vm.Marchamos,
                FechaHoraDespacho = vm.FechaHoraDespacho,
                Transportista = vm.Transportista,
                Informacion = vm.Informacion,
                Chofer = vm.Chofer,
                PlacaCabezal = vm.PlacaCabezal,
                Chasis = vm.Chasis,
                ViajeDua = vm.ViajeDua,
                EsSalidaEnFurgon = vm.EsSalidaEnFurgon,
                ContenedorReferencia = vm.ContenedorReferencia,
                GuardarContenedorSalida = vm.GuardarContenedorSalida
            };

            _context.BitacoraDespachos.Add(despacho);

            // =========================
            // 🔥 MANEJO INTELIGENTE
            // =========================

            if (entidad != null)
            {
                if (tipo == "CONTENEDOR")
                {
                    switch (entidad)
                    {
                        case ContenedorSinAsignarPatio c: _context.ContenedoresSinAsignarPatio.Remove(c); break;
                        case PatioQuimicos c: _context.PatioQuimicos.Remove(c); break;
                        case Patio1 c: _context.Patio1.Remove(c); break;
                        case Patio2 c: _context.Patio2.Remove(c); break;
                        case Anden2000 c: _context.Anden2000.Remove(c); break;
                    }
                }
                else if (tipo == "VEHICULO")
                {
                    var vehiculo = entidad as Vehiculo;

                    if (vehiculo == null)
                    {
                        TempData["Error"] = "Error interno: entidad vehículo inválida.";
                        return RedirectToAction("Index", new { fecha = vm.FechaHoraDespacho.Date });
                    }

                    var vehiculoDb = await _context.Vehiculos
                        .FirstOrDefaultAsync(v =>
                            v.Marchamos == vehiculo.Marchamos &&
                            v.Chasis == vehiculo.Chasis);

                    if (vehiculoDb != null)
                    {
                        _context.Vehiculos.Remove(vehiculoDb);
                        await _context.SaveChangesAsync();
                    }
                    // else: no existe
                }

                // 🔎 HISTORIAL
                var historial = await _context.HistorialContenedores
                    .Where(h => h.Contenedor == contenedorBuscado && h.FechaHoraSalida == null)
                    .OrderByDescending(h => h.FechaHoraIngreso)
                    .FirstOrDefaultAsync();

                if (historial != null)
                {
                    historial.FechaHoraSalida = vm.FechaHoraDespacho;
                }
                else
                {
                    _context.HistorialContenedores.Add(new HistorialContenedor
                    {
                        Contenedor = contenedorBuscado,
                        FechaHoraIngreso = null,
                        FechaHoraSalida = vm.FechaHoraDespacho
                    });
                }
            }
            else
            {
                // 🧾 FURGÓN
                _context.HistorialContenedores.Add(new HistorialContenedor
                {
                    Contenedor = contenedorBuscado,
                    FechaHoraIngreso = null,
                    FechaHoraSalida = vm.FechaHoraDespacho
                });
            }

            // 🧹 LIMPIAR PRECARGAS
            var datos = await _context.DatosDespachosViajes
                .FirstOrDefaultAsync(x => x.Contenedor == vm.Contenedor);

            if (datos != null)
                _context.DatosDespachosViajes.Remove(datos);

            var precarga = await _context.DatosDespachosViajes
                .FirstOrDefaultAsync(x => x.Contenedor == vm.Contenedor);

            if (precarga != null)
                _context.DatosDespachosViajes.Remove(precarga);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { fecha = vm.FechaHoraDespacho.Date });
        }

        //Buscar ubicación del contenedor
        private async Task<(object? entidad, string tipo, string? ubicacion)> BuscarUnidadGlobal(string numero)
        {
            numero = numero.ToUpper();

            // 🔹 VEHÍCULO
            var vehiculo = await _context.Vehiculos
                .FirstOrDefaultAsync(v => v.Contenedor == numero && v.Activo);

            if (vehiculo != null)
                return (vehiculo, "VEHICULO", "Vehículos");

            // 🔹 CONTENEDOR (tu lógica actual)
            var (contenedor, patio) = await BuscarContenedorGlobal(numero);

            if (contenedor != null)
                return (contenedor, "CONTENEDOR", patio);

            return (null, "", null);
        }

        private async Task<(object? contenedor, string? patio)> BuscarContenedorGlobal(string numero)
        {
            numero = numero.ToUpper();

            var sinAsignar = await _context.ContenedoresSinAsignarPatio
                .FirstOrDefaultAsync(c => c.Contenedor == numero);
            if (sinAsignar != null) return (sinAsignar, "Sin Asignar");

            var q = await _context.PatioQuimicos.FirstOrDefaultAsync(c => c.Contenedor == numero);
            if (q != null) return (q, "Patio Químicos");

            var p1 = await _context.Patio1.FirstOrDefaultAsync(c => c.Contenedor == numero);
            if (p1 != null) return (p1, "Patio 1");

            var p2 = await _context.Patio2.FirstOrDefaultAsync(c => c.Contenedor == numero);
            if (p2 != null) return (p2, "Patio 2");

            var a = await _context.Anden2000.FirstOrDefaultAsync(c => c.Contenedor == numero);
            if (a != null) return (a, "Andén 2000");

            return (null, null);
        }

        //Traer informacion del contenedor
        [HttpGet]
        public async Task<IActionResult> BuscarParaDespacho(string contenedor)
        {
            if (string.IsNullOrWhiteSpace(contenedor))
                return Json(new { encontrado = false });

            contenedor = contenedor.ToUpper().Trim();

            // 🔥 1. BUSCAR PRECARGA (PRIORIDAD)
            var precarga = await _context.DatosDespachosViajes
                .FirstOrDefaultAsync(x => x.Contenedor == contenedor);

            // 🔥 2. BUSCAR EN INVENTARIO (patios o vehículos)
            var (entidad, tipo, ubicacion) = await BuscarUnidadGlobal(contenedor);

            if (entidad == null)
            {
                return Json(new
                {
                    encontrado = false,
                    mensaje = "No se encontró en inventario"
                });
            }

            // =========================
            // 🚗 VEHÍCULO
            // =========================
            if (tipo == "VEHICULO")
            {
                var v = (Vehiculo)entidad;

                if (!v.Activo)
                {
                    return Json(new
                    {
                        encontrado = false,
                        mensaje = "Este vehículo ya fue despachado"
                    });
                }

                return Json(new
                {
                    encontrado = true,

                    contenedor = v.Contenedor,
                    marchamos = precarga?.Marchamos ?? v.Marchamos ?? "",
                    chasis = precarga?.Chasis ?? v.Chasis ?? "",
                    transportista = precarga?.Transportista ?? v.Transportista ?? "",
                    cliente = precarga?.Cliente ?? v.Cliente ?? "",

                    // 🔥 datos precarga si existen
                    chofer = precarga?.Chofer ?? "",
                    placaCabezal = precarga?.PlacaCabezal ?? "",
                    viajeDua = precarga?.ViajeDua ?? "",

                    estado = "Activo",
                    patio = "Vehículos"
                });
            }

            // =========================
            // 📦 CONTENEDOR
            // =========================
            var c = (IContenedorInventario)entidad;

            return Json(new
            {
                encontrado = true,

                contenedor = c.Contenedor,
                marchamos = precarga?.Marchamos ?? c.Marchamos ?? "",
                chasis = precarga?.Chasis ?? c.Chasis ?? "",
                transportista = precarga?.Transportista ?? c.Transportista ?? "",
                cliente = precarga?.Cliente ?? c.Cliente ?? "",

                // 🔥 datos precarga
                chofer = precarga?.Chofer ?? "",
                placaCabezal = precarga?.PlacaCabezal ?? "",
                viajeDua = precarga?.ViajeDua ?? "",

                estado = c.EstadoCarga ?? "",
                patio = ubicacion
            });
        }

        //Exportar a excel
        public async Task<IActionResult> ExportarExcel(DateTime fecha)
        {
            var despachos = await _context.BitacoraDespachos
                .Where(d => d.FechaHoraDespacho.Date == fecha.Date)
                .OrderBy(d => d.FechaHoraDespacho)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Despachos");

            // Encabezados
            ws.Cell(1, 1).Value = "Contenedor";
            ws.Cell(1, 2).Value = "Marchamos";
            ws.Cell(1, 3).Value = "Fecha/Hora Despacho";
            ws.Cell(1, 4).Value = "Transportista";
            ws.Cell(1, 9).Value = "Información";
            ws.Cell(1, 5).Value = "Chofer";
            ws.Cell(1, 6).Value = "Placa Cabezal";
            ws.Cell(1, 7).Value = "Chasis";
            ws.Cell(1, 8).Value = "Viaje / DUA";            
            ws.Cell(1, 11).Value = "Contenedor Referencia";

            int fila = 2;

            foreach (var d in despachos)
            {
                ws.Cell(fila, 1).Value = d.Contenedor;
                ws.Cell(fila, 2).Value = d.Marchamos;
                ws.Cell(fila, 3).Value = d.FechaHoraDespacho.ToString("yyyy-MM-dd HH:mm");
                ws.Cell(fila, 4).Value = d.Transportista;
                ws.Cell(fila, 9).Value = d.Informacion;
                ws.Cell(fila, 5).Value = d.Chofer;
                ws.Cell(fila, 6).Value = d.PlacaCabezal;
                ws.Cell(fila, 7).Value = d.Chasis;
                ws.Cell(fila, 8).Value = d.ViajeDua;                
                ws.Cell(fila, 11).Value = d.ContenedorReferencia;
                fila++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Despachos_{fecha:yyyyMMdd}.xlsx"
            );
        }

        //Exportar a csv
        public async Task<IActionResult> ExportarCsv(DateTime fecha)
        {
            var despachos = await _context.BitacoraDespachos
                .Where(d => d.FechaHoraDespacho.Date == fecha.Date)
                .OrderBy(d => d.FechaHoraDespacho)
                .ToListAsync();

            var sb = new StringBuilder();

            sb.AppendLine("Contenedor,Marchamos,FechaHoraDespacho,Transportista,Informacion,Chofer,PlacaCabezal,Chasis,ViajeDUA,ContenedorReferencia");

            foreach (var d in despachos)
            {
                sb.AppendLine(
                    $"{d.Contenedor}," +
                    $"{d.Marchamos}," +
                    $"{d.FechaHoraDespacho:yyyy-MM-dd HH:mm}," +
                    $"{d.Transportista}," +
                    $"{d.Informacion}," +
                    $"{d.Chofer}," +
                    $"{d.PlacaCabezal}," +
                    $"{d.Chasis}," +
                    $"{d.ViajeDua}," +                    
                    $"{d.ContenedorReferencia}"
                );
            }

            return File(
                Encoding.UTF8.GetBytes(sb.ToString()),
                "text/csv",
                $"Despachos_{fecha:yyyyMMdd}.csv"
            );
        }

        //Exportar a pdf
        public async Task<IActionResult> ExportarPdf(DateTime? fecha)
        {
            var fechaSeleccionada = fecha ?? DateTime.Today;
            var despachos = await _context.BitacoraDespachos
                .Where(d => d.FechaHoraDespacho.Date == fechaSeleccionada)
                .OrderBy(d => d.FechaHoraDespacho)
                .ToListAsync();

            var document = new BitacoraDespachosPdf(fechaSeleccionada, despachos);

            byte[] pdf = document.GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                $"Despachos_{fechaSeleccionada:yyyy/MM/dd}.pdf"
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var despacho = await _context.BitacoraDespachos.FindAsync(id);
            if (despacho == null)
                return RedirectToAction("Index");

            var contenedor = despacho.Contenedor;

            // 🔥 SI ERA SALIDA EN FURGÓN
            if (despacho.EsSalidaEnFurgon)
            {
                // eliminar historial de esa salida
                var historialFurgon = await _context.HistorialContenedores
                    .Where(h => h.Contenedor == contenedor &&
                                h.FechaHoraIngreso == null &&
                                h.FechaHoraSalida == despacho.FechaHoraDespacho)
                    .FirstOrDefaultAsync();

                if (historialFurgon != null)
                    _context.HistorialContenedores.Remove(historialFurgon);
            }
            else
            {
                // 🛟 BUSCAR BACKUP
                var backup = await _context.ContenedoresBackupDespacho
                    .Where(b => b.Contenedor == contenedor)
                    .OrderByDescending(b => b.FechaRespaldo)
                    .FirstOrDefaultAsync();

                if (backup != null)
                {
                    // 🧱 RECREAR CONTENEDOR SEGÚN PATIO
                    switch (backup.PatioOrigen)
                    {
                        case "Sin Asignar":
                            _context.ContenedoresSinAsignarPatio.Add(new ContenedorSinAsignarPatio
                            {
                                Contenedor = backup.Contenedor,
                                Marchamos = backup.Marchamos,
                                EstadoCarga = backup.Estado,
                                Tamano = backup.Tamaño,
                                Transportista = backup.Transportista,
                                Cliente = backup.Cliente,
                                Chasis = backup.Chasis
                            });
                            break;

                        case "Patio Químicos":
                            _context.PatioQuimicos.Add(new PatioQuimicos
                            {
                                Contenedor = backup.Contenedor,
                                Marchamos = backup.Marchamos,
                                EstadoCarga = backup.Estado,
                                Tamano = backup.Tamaño,
                                Transportista = backup.Transportista,
                                Cliente = backup.Cliente,
                                Chasis = backup.Chasis
                            });
                            break;

                        case "Patio 1":
                            _context.Patio1.Add(new Patio1
                            {
                                Contenedor = backup.Contenedor,
                                Marchamos = backup.Marchamos,
                                EstadoCarga = backup.Estado,
                                Tamano = backup.Tamaño,
                                Transportista = backup.Transportista,
                                Cliente = backup.Cliente,
                                Chasis = backup.Chasis
                            });
                            break;

                        case "Patio 2":
                            _context.Patio2.Add(new Patio2
                            {
                                Contenedor = backup.Contenedor,
                                Marchamos = backup.Marchamos,
                                EstadoCarga = backup.Estado,
                                Tamano = backup.Tamaño,
                                Transportista = backup.Transportista,
                                Cliente = backup.Cliente,
                                Chasis = backup.Chasis
                            });
                            break;

                        case "Andén 2000":
                            _context.Anden2000.Add(new Anden2000
                            {
                                Contenedor = backup.Contenedor,
                                Marchamos = backup.Marchamos,
                                EstadoCarga = backup.Estado,
                                Tamano = backup.Tamaño,
                                Transportista = backup.Transportista,
                                Cliente = backup.Cliente,
                                Chasis = backup.Chasis
                            });
                            break;
                    }

                    _context.ContenedoresBackupDespacho.Remove(backup);
                }

                // 📚 REABRIR HISTORIAL
                var historial = await _context.HistorialContenedores
                    .Where(h => h.Contenedor == contenedor &&
                                h.FechaHoraSalida == despacho.FechaHoraDespacho)
                    .FirstOrDefaultAsync();

                if (historial != null)
                    historial.FechaHoraSalida = null;
            }

            // 🗑 BORRAR DESPACHO
            _context.BitacoraDespachos.Remove(despacho);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        private const int PageSize = 50;

        public async Task<IActionResult> Backups(DateTime? fecha, int page = 1)
        {
            if (page < 1) page = 1;

            var query = _context.ContenedoresBackupDespacho.AsQueryable();

            // 🔍 Filtrar por fecha
            if (fecha.HasValue)
            {
                // Filtrar por día completo
                var inicio = fecha.Value.Date;
                var fin = inicio.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.FechaRespaldo >= inicio && x.FechaRespaldo <= fin);
            }

            query = query.OrderByDescending(x => x.FechaRespaldo);

            int totalRegistros = await query.CountAsync();
            int totalPaginas = (int)Math.Ceiling(totalRegistros / (double)PageSize);

            if (page > totalPaginas && totalPaginas > 0)
                page = totalPaginas;

            var backups = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.FechaSeleccionada = fecha?.ToString("yyyy-MM-dd");
            ViewBag.PaginaActual = page;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalRegistros;

            return View(backups);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LimpiarBackups()
        {
            var backups = await _context.ContenedoresBackupDespacho.ToListAsync();

            _context.ContenedoresBackupDespacho.RemoveRange(backups);

            await _context.SaveChangesAsync();

            return RedirectToAction("Backups");
        }

        //edicion de despachos
        public async Task<IActionResult> Editar(int id)
        {
            var despacho = await _context.BitacoraDespachos
                .FirstOrDefaultAsync(x => x.Id == id);

            if (despacho == null)
                return NotFound();

            var vm = new BitacoraDespachosViewModel
            {
                // SOLO LECTURA
                Contenedor = despacho.Contenedor,

                // EDITABLES
                Marchamos = despacho.Marchamos,
                FechaHoraDespacho = despacho.FechaHoraDespacho,
                Transportista = despacho.Transportista,
                Informacion = despacho.Informacion,
                Chofer = despacho.Chofer,
                PlacaCabezal = despacho.PlacaCabezal,
                Chasis = despacho.Chasis,
                ViajeDua = despacho.ViajeDua,
                EsSalidaEnFurgon = despacho.EsSalidaEnFurgon,
                ContenedorReferencia = despacho.ContenedorReferencia,

                // IMPORTANTE
                Id = despacho.Id
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(BitacoraDespachosViewModel vm)
        {
            var despacho = await _context.BitacoraDespachos
                .FirstOrDefaultAsync(x => x.Id == vm.Id);

            if (despacho == null)
                return NotFound();

            // ❌ NO TOCAR CONTENEDOR
            // despacho.Contenedor = vm.Contenedor;

            // ✅ EDITABLES
            despacho.Marchamos = vm.Marchamos;
            despacho.FechaHoraDespacho = vm.FechaHoraDespacho;
            despacho.Transportista = vm.Transportista;
            despacho.Informacion = vm.Informacion;
            despacho.Chofer = vm.Chofer;
            despacho.PlacaCabezal = vm.PlacaCabezal;
            despacho.Chasis = vm.Chasis;
            despacho.ViajeDua = vm.ViajeDua;
            despacho.EsSalidaEnFurgon = vm.EsSalidaEnFurgon;
            despacho.ContenedorReferencia = vm.ContenedorReferencia;
            despacho.GuardarContenedorSalida = vm.GuardarContenedorSalida;

            // 🔥 OPCIONAL PERO IMPORTANTE: actualizar historial
            var historial = await _context.HistorialContenedores
                .Where(h => h.Contenedor == despacho.Contenedor)
                .OrderByDescending(h => h.FechaHoraIngreso)
                .FirstOrDefaultAsync();

            if (historial != null)
            {
                historial.FechaHoraSalida = vm.FechaHoraDespacho;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { fecha = vm.FechaHoraDespacho.Date });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarBackupsAntiguos()
        {
            var fechaLimite = DateTime.Now.AddDays(-7);

            var antiguos = await _context.ContenedoresBackupDespacho
                .Where(x => x.FechaRespaldo < fechaLimite)
                .ToListAsync();

            if (antiguos.Any())
            {
                _context.ContenedoresBackupDespacho.RemoveRange(antiguos);
                await _context.SaveChangesAsync();

                TempData["Ok"] = $"Se eliminaron {antiguos.Count} backups antiguos.";
            }
            else
            {
                TempData["Error"] = "No hay backups con más de 7 días.";
            }

            return RedirectToAction(nameof(Backups));
        }
    }
}
