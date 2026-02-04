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

        public async Task<IActionResult> Index(DateTime? fecha)
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
                FechaHoraDespacho = DateTime.Now
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

            object? contenedorObj = null;
            IContenedorInventario? inventario = null;
            string? patioOrigen = null;

            // 🔎 SOLO BUSCAR EN PATIOS SI NO ES FURGÓN
            if (!vm.EsSalidaEnFurgon)
            {
                var resultado = await BuscarContenedorGlobal(contenedorBuscado);
                contenedorObj = resultado.contenedor;
                patioOrigen = resultado.patio;

                if (contenedorObj == null)
                    return RedirectToAction("Index");

                inventario = (IContenedorInventario)contenedorObj;

                // ✏️ actualizar marchamo
                inventario.Marchamos = vm.Marchamos;

                // 🛟 BACKUP ANTES DE BORRAR DEL INVENTARIO
                var backup = new ContenedorBackupDespacho
                {
                    Contenedor = inventario.Contenedor,
                    PatioOrigen = patioOrigen ?? "",
                    Marchamos = inventario.Marchamos ?? "",
                    Estado = inventario.EstadoCarga ?? "",
                    Tamaño = inventario.Tamano ?? "",
                    Transportista = inventario.Transportista ?? "",
                    Chasis = inventario.Chasis ?? "",
                    FechaRespaldo = DateTime.Now
                };

                _context.ContenedoresBackupDespacho.Add(backup);
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
                ContenedorReferencia = vm.ContenedorReferencia
            };

            _context.BitacoraDespachos.Add(despacho);

            // 🗑 ELIMINAR DE PATIO SOLO CONTENEDORES FÍSICOS
            if (!vm.EsSalidaEnFurgon && contenedorObj != null)
            {
                switch (contenedorObj)
                {
                    case ContenedorSinAsignarPatio c: _context.ContenedoresSinAsignarPatio.Remove(c); break;
                    case PatioQuimicos c: _context.PatioQuimicos.Remove(c); break;
                    case Patio1 c: _context.Patio1.Remove(c); break;
                    case Patio2 c: _context.Patio2.Remove(c); break;
                    case Anden2000 c: _context.Anden2000.Remove(c); break;
                }

                var historial = await _context.HistorialContenedores
                    .Where(h => h.Contenedor == contenedorBuscado && h.FechaHoraSalida == null)
                    .OrderByDescending(h => h.FechaHoraIngreso)
                    .FirstOrDefaultAsync();

                if (historial != null)
                    historial.FechaHoraSalida = vm.FechaHoraDespacho;
            }

            // 🧾 HISTORIAL PARA FURGÓN
            if (vm.EsSalidaEnFurgon)
            {
                var historialFurgon = new HistorialContenedor
                {
                    Contenedor = contenedorBuscado,
                    FechaHoraIngreso = null,
                    FechaHoraSalida = vm.FechaHoraDespacho
                };

                _context.HistorialContenedores.Add(historialFurgon);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { fecha = vm.FechaHoraDespacho.Date });
        }


        //Buscar ubicación del contenedor
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

            // 🔍 Verificar si ya fue despachado
            var historial = await _context.HistorialContenedores
                .Where(h => h.Contenedor == contenedor && h.FechaHoraSalida != null)
                .OrderByDescending(h => h.FechaHoraSalida)
                .FirstOrDefaultAsync();

            if (historial != null)
            {
                return Json(new
                {
                    encontrado = false,
                    mensaje = $"Este contenedor ya fue despachado el {historial.FechaHoraSalida:dd/MM/yyyy HH:mm}"
                });
            }

            // 🔍 Buscar en patios
            var (data, patio) = await BuscarContenedorGlobal(contenedor);

            if (data == null)
                return Json(new { encontrado = false });

            var c = (IContenedorInventario)data;

            return Json(new
            {
                encontrado = true,
                contenedor = c.Contenedor,
                marchamos = c.Marchamos ?? "",
                chasis = c.Chasis ?? "",
                transportista = c.Transportista ?? "",
                estado = c.EstadoCarga ?? "",
                patio
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

        public async Task<IActionResult> Backups()
        {
            var backups = await _context.ContenedoresBackupDespacho
                .OrderByDescending(x => x.FechaRespaldo)
                .ToListAsync();

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


    }

}
