using BitacoraAlfipac.Data;
using BitacoraAlfipac.Documents;
using BitacoraAlfipac.Models.Entidades;
using BitacoraAlfipac.Models.ViewModels;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using System.Text;

namespace BitacoraAlfipac.Controllers;

[Authorize]
public class BitacoraIngresosController : Controller
{
    private readonly ApplicationDbContext _context;

    public BitacoraIngresosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET
    public async Task<IActionResult> Index(DateTime? fecha)
    {
        DateTime fechaSeleccionada = fecha?.Date ?? DateTime.Today;

        var ingresos = await _context.BitacoraIngresos
            .Where(i => i.FechaHoraIngreso.Date == fechaSeleccionada)
            .OrderBy(i => i.FechaHoraIngreso)
            .ToListAsync();

        var model = new BitacoraIngresosViewModel
        {
            FechaSeleccionada = fechaSeleccionada,
            FechaHoraIngreso = DateTime.Now,
            Ingresos = ingresos
        };

        return View(model);
    }

    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(BitacoraIngresosViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Ingresos = await _context.BitacoraIngresos
                .Where(i => i.FechaHoraIngreso.Date == model.FechaSeleccionada)
                .OrderBy(i => i.FechaHoraIngreso)
                .ToListAsync();

            return View(model);
        }

        // Limpiar segundos y milisegundos
        var fechaHora = model.FechaHoraIngreso;
        fechaHora = new DateTime(
            fechaHora.Year,
            fechaHora.Month,
            fechaHora.Day,
            fechaHora.Hour,
            fechaHora.Minute,
            0
        );

        var ingreso = new BitacoraIngreso
        {
            Contenedor = model.Contenedor,
            Marchamos = model.Marchamos,
            FechaHoraIngreso = fechaHora,
            Transportista = model.Transportista,
            Cliente = model.Cliente,
            Tamaño = model.Tamano,
            Chofer = model.Chofer,
            PlacaCabezal = model.PlacaCabezal,
            Chasis = model.Chasis,
            ViajeDua = model.ViajeDua
        };

        _context.BitacoraIngresos.Add(ingreso);

        _context.HistorialContenedores.Add(new HistorialContenedor
        {
            Contenedor = model.Contenedor,
            FechaHoraIngreso = fechaHora
        });

        //manejo de vehiculos
        var esVehiculo = model.Tamano.Trim().ToUpper() == "VEHICULO";

        if (!esVehiculo)
        {
            // 📦 CONTENEDOR NORMAL (SE MANTIENE IGUAL)
            _context.ContenedoresSinAsignarPatio.Add(new ContenedorSinAsignarPatio
            {
                Contenedor = model.Contenedor,
                Marchamos = model.Marchamos,
                Tamano = model.Tamano,
                Transportista = model.Transportista,
                Cliente = model.Cliente,
                Chasis = model.Chasis,
                EstadoCarga = "Cargado",
                Ubicacion = "Sin asignar"
            });
        }
        else
        {
            // 🚗 VEHÍCULO → NUEVA TABLA

            var existeVehiculo = await _context.Vehiculos
                .AnyAsync(v => v.Chasis == model.Chasis && v.Activo);

            if (existeVehiculo)
            {
                TempData["Error"] = "Este vehículo ya se encuentra registrado.";
                return RedirectToAction(nameof(Index), new { fecha = fechaHora.Date });
            }

            var vehiculo = new Vehiculo
            {
                Contenedor = model.Contenedor.ToUpper().Trim(),
                Marchamos = string.IsNullOrWhiteSpace(model.Marchamos) ? "S/M" : model.Marchamos,
                FechaHoraIngreso = fechaHora,
                Transportista = model.Transportista,
                Cliente = model.Cliente,
                Tamano = "VEHICULO",
                Chofer = model.Chofer,
                PlacaCabezal = string.IsNullOrWhiteSpace(model.PlacaCabezal) ? "S/P" : model.PlacaCabezal,
                Chasis = model.Chasis,
                ViajeDua = model.ViajeDua,
                Activo = true
            };

            _context.Vehiculos.Add(vehiculo);
        }

        // 🔹 SI ES REFRIGERADO, CREARLO EN EL MODULO REEFER
        if (model.EsRefrigerado)
        {
            bool yaExiste = await _context.ContenedoresRefrigerados
                .AnyAsync(r => r.Contenedor == model.Contenedor && r.FechaHoraIngreso == null);

            if (!yaExiste)
            {
                _context.ContenedoresRefrigerados.Add(new ContenedorRefrigerado
                {
                    Contenedor = model.Contenedor,
                    FechaHoraIngreso = fechaHora,
                });
            }
        }

        var datosIngreso = await _context.DatosIngresosViajes
        .FirstOrDefaultAsync(x => x.Contenedor == model.Contenedor);

        if (datosIngreso != null)
        {
            _context.DatosIngresosViajes.Remove(datosIngreso);
        }

        await _context.SaveChangesAsync();

        // 🔑 CLAVE: refrescar la MISMA página con la fecha seleccionada
        return RedirectToAction(nameof(Index), new { fecha = fechaHora.Date });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(BitacoraIngresosViewModel model)
    {
        var ingreso = await _context.BitacoraIngresos
            .FirstOrDefaultAsync(i => i.Id == model.Id);

        if (ingreso == null)
            return NotFound();

        // ❌ NO se edita el contenedor
        // ingreso.Contenedor = model.Contenedor;

        ingreso.Marchamos = model.Marchamos;
        ingreso.Transportista = model.Transportista;
        ingreso.Cliente = model.Cliente;
        ingreso.Tamaño = model.Tamano;
        ingreso.Chofer = model.Chofer;
        ingreso.PlacaCabezal = model.PlacaCabezal;
        ingreso.Chasis = model.Chasis;
        ingreso.ViajeDua = model.ViajeDua;

        ingreso.FechaHoraIngreso = new DateTime(
            model.FechaHoraIngreso.Year,
            model.FechaHoraIngreso.Month,
            model.FechaHoraIngreso.Day,
            model.FechaHoraIngreso.Hour,
            model.FechaHoraIngreso.Minute,
            0
        );

        // =========================
        // 🔄 SINCRONIZAR INVENTARIO
        // =========================
        var resultado = await BuscarContenedorGlobal(ingreso.Contenedor);

        if (resultado.contenedor != null)
        {
            var inv = (IContenedorInventario)resultado.contenedor;

            inv.Marchamos = ingreso.Marchamos;
            inv.Transportista = ingreso.Transportista;
            inv.Cliente = ingreso.Cliente;
            inv.Chasis = ingreso.Chasis;
            inv.Tamano = ingreso.Tamaño;

            // 💡 Solo si aplica
            if (!string.IsNullOrEmpty(inv.EstadoCarga))
                inv.EstadoCarga = "Cargado";
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { fecha = ingreso.FechaHoraIngreso.Date });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, DateTime fecha)
    {
        var ingreso = await _context.BitacoraIngresos.FindAsync(id);
        if (ingreso == null)
            return RedirectToAction(nameof(Index), new { fecha });

        string contenedor = ingreso.Contenedor;

        // Bitácora ingresos
        _context.BitacoraIngresos.Remove(ingreso);

        // Historial activo
        var historial = await _context.HistorialContenedores
            .FirstOrDefaultAsync(h => h.Contenedor == contenedor && h.FechaHoraSalida == null);

        if (historial != null)
            _context.HistorialContenedores.Remove(historial);

        // Sin asignar patio
        var sinPatio = await _context.ContenedoresSinAsignarPatio
            .FirstOrDefaultAsync(c => c.Contenedor == contenedor);

        if (sinPatio != null)
            _context.ContenedoresSinAsignarPatio.Remove(sinPatio);

        // Patios
        _context.PatioQuimicos.RemoveRange(
            _context.PatioQuimicos.Where(p => p.Contenedor == contenedor));

        _context.Patio1.RemoveRange(
            _context.Patio1.Where(p => p.Contenedor == contenedor));

        _context.Patio2.RemoveRange(
            _context.Patio2.Where(p => p.Contenedor == contenedor));

        _context.Anden2000.RemoveRange(
            _context.Anden2000.Where(p => p.Contenedor == contenedor));

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { fecha });
    }

    private void RefrescarDatosContenedor(
    ContenedorBase contenedorActivo,
    BitacoraIngreso ingreso)
    {
        contenedorActivo.Contenedor = ingreso.Contenedor;
        contenedorActivo.Marchamos = ingreso.Marchamos;
        contenedorActivo.Tamano = ingreso.Tamaño;
        contenedorActivo.Transportista = ingreso.Transportista;
        contenedorActivo.Cliente = ingreso.Cliente;
        contenedorActivo.Chasis = ingreso.Chasis;
    }

    private async Task<ContenedorBase?> ObtenerContenedorActivo(string contenedor)
    {
        return null;
    }

    public async Task<IActionResult> ExportarExcel(DateTime fecha)
    {
        var ingresos = await _context.BitacoraIngresos
            .Where(i => i.FechaHoraIngreso.Date == fecha.Date)
            .OrderBy(i => i.FechaHoraIngreso)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Bitacora");

        // Encabezados
        ws.Cell(1, 1).Value = "Contenedor";
        ws.Cell(1, 2).Value = "Marchamos";
        ws.Cell(1, 3).Value = "Fecha/Hora";
        ws.Cell(1, 4).Value = "Transportista";
        ws.Cell(1, 5).Value = "Cliente";
        ws.Cell(1, 6).Value = "Tamaño";
        ws.Cell(1, 7).Value = "Chofer";
        ws.Cell(1, 8).Value = "Placa Cabezal";
        ws.Cell(1, 9).Value = "Chasis";
        ws.Cell(1, 10).Value = "Viaje / DUA";

        int fila = 2;

        foreach (var i in ingresos)
        {
            ws.Cell(fila, 1).Value = i.Contenedor;
            ws.Cell(fila, 2).Value = i.Marchamos;
            ws.Cell(fila, 3).Value = i.FechaHoraIngreso.ToString("yyyy-MM-dd HH:mm");
            ws.Cell(fila, 4).Value = i.Transportista;
            ws.Cell(fila, 5).Value = i.Cliente;
            ws.Cell(fila, 6).Value = i.Tamaño;
            ws.Cell(fila, 7).Value = i.Chofer;
            ws.Cell(fila, 8).Value = i.PlacaCabezal;
            ws.Cell(fila, 9).Value = i.Chasis;
            ws.Cell(fila, 10).Value = i.ViajeDua;
            fila++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Ingresos{fecha:yyyyMMdd}.xlsx"
        );
    }

    public async Task<IActionResult> ExportarCsv(DateTime fecha)
    {
        var ingresos = await _context.BitacoraIngresos
            .Where(i => i.FechaHoraIngreso.Date == fecha.Date)
            .OrderBy(i => i.FechaHoraIngreso)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine(
            "Contenedor,Marchamos,FechaHora,Transportista,Cliente,Tamaño,Chofer,PlacaCabezal,Chasis,ViajeDUA"
        );

        foreach (var i in ingresos)
        {
            sb.AppendLine(
                $"{i.Contenedor}," +
                $"{i.Marchamos}," +
                $"{i.FechaHoraIngreso:yyyy-MM-dd HH:mm}," +
                $"{i.Transportista}," +
                $"{i.Cliente}," +
                $"{i.Tamaño}," +
                $"{i.Chofer}," +
                $"{i.PlacaCabezal}," +
                $"{i.Chasis}," +
                $"{i.ViajeDua}"
            );
        }

        return File(
            Encoding.UTF8.GetBytes(sb.ToString()),
            "text/csv",
            $"Ingresos{fecha:yyyyMMdd}.csv"
        );
    }

    public async Task<IActionResult> ExportarPdf(DateTime? fecha)
    {
        DateTime fechaSeleccionada = fecha?.Date ?? DateTime.Today;
        var ingresos = await _context.BitacoraIngresos
            .Where(i => i.FechaHoraIngreso.Date == fechaSeleccionada)
            .OrderBy(i => i.FechaHoraIngreso)
            .ToListAsync();

        var document = new BitacoraIngresosPdf(fechaSeleccionada, ingresos);

        byte[] pdf = document.GeneratePdf();

        return File(
            pdf,
            "application/pdf",
            $"Ingresos_{fechaSeleccionada:yyyy/MM/dd}.pdf"
        );

    }

    [HttpGet]
    public IActionResult ObtenerDatosPorContenedor(string contenedor)
    {
        if (string.IsNullOrWhiteSpace(contenedor))
            return Json(null);

        var ingreso = _context.DatosIngresosViajes
            .FirstOrDefault(x => x.Contenedor == contenedor);

        if (ingreso == null)
            return Json(null);

        // Buscar transportista autorizado por cédula jurídica
        var transportistaAutorizado = _context.TransportistasAutorizados
            .FirstOrDefault(t => t.CedulaJuridica == ingreso.Transportista);

        var resultado = new
        {
            cliente = ingreso.Declarante,
            transportista = transportistaAutorizado?.Codigo ?? ingreso.Transportista,
            viaje = ingreso.Viaje
        };

        return Json(resultado);
    }
    private async Task<(object? contenedor, string? patio)> BuscarContenedorGlobal(string contenedor)
    {
        if (string.IsNullOrWhiteSpace(contenedor))
            return (null, null);

        contenedor = contenedor.ToUpper().Trim();

        // =========================
        // 🔵 VEHICULOS (PRIORIDAD)
        // =========================
        var vehiculo = await _context.ContenedoresSinAsignarPatio
            .FirstOrDefaultAsync(c => c.Contenedor == contenedor && c.Tamano == "VEHICULO");

        if (vehiculo != null)
            return (vehiculo, "Vehiculos");

        // =========================
        // 🔵 SIN ASIGNAR
        // =========================
        var sinAsignar = await _context.ContenedoresSinAsignarPatio
            .FirstOrDefaultAsync(c => c.Contenedor == contenedor);

        if (sinAsignar != null)
            return (sinAsignar, "SinAsignar");

        // =========================
        // 🔵 PATIOS
        // =========================
        var p1 = await _context.Patio1
            .FirstOrDefaultAsync(c => c.Contenedor == contenedor);

        if (p1 != null)
            return (p1, "Patio1");

        var p2 = await _context.Patio2
            .FirstOrDefaultAsync(c => c.Contenedor == contenedor);

        if (p2 != null)
            return (p2, "Patio2");

        var anden = await _context.Anden2000
            .FirstOrDefaultAsync(c => c.Contenedor == contenedor);

        if (anden != null)
            return (anden, "Anden2000");

        var quimicos = await _context.PatioQuimicos
            .FirstOrDefaultAsync(c => c.Contenedor == contenedor);

        if (quimicos != null)
            return (quimicos, "PatioQuimicos");

        return (null, null);
    }

    //vehiculos
    //eliminar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarVehiculo(int id)
    {
        var vehiculo = await _context.Vehiculos
            .FirstOrDefaultAsync(x => x.Id == id);

        if (vehiculo == null)
            return NotFound();

        _context.Vehiculos.Remove(vehiculo);

        await _context.SaveChangesAsync();

        return RedirectToAction("Vehiculos");
    }

    //filtros
    public async Task<IActionResult> Vehiculos(
    string? contenedor,
    string? chasis,
    string? cliente,
    DateTime? fechaInicio,
    DateTime? fechaFin)
    {
        var query = _context.Vehiculos.AsQueryable();

        // 🔍 FILTROS
        if (!string.IsNullOrWhiteSpace(contenedor))
            query = query.Where(x => x.Contenedor.Contains(contenedor));

        if (!string.IsNullOrWhiteSpace(chasis))
            query = query.Where(x => x.Chasis.Contains(chasis));

        if (!string.IsNullOrWhiteSpace(cliente))
            query = query.Where(x => x.Cliente.Contains(cliente));

        if (fechaInicio.HasValue)
            query = query.Where(x => x.FechaHoraIngreso >= fechaInicio);

        if (fechaFin.HasValue)
            query = query.Where(x => x.FechaHoraIngreso <= fechaFin);

        var lista = await query
            .OrderByDescending(x => x.FechaHoraIngreso)
            .ToListAsync();

        // 📊 KPIs
        ViewBag.TotalVehiculos = lista.Count;

        ViewBag.VehiculosHoy = lista.Count(x =>
            x.FechaHoraIngreso.Date == DateTime.Today);

        ViewBag.VehiculosAntiguos = lista.Count(x =>
            (DateTime.Now - x.FechaHoraIngreso).TotalHours > 24);

        return View(lista);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearVehiculoManual(
    string contenedor,
    string chasis,
    string? cliente,
    string? transportista,
    string? marchamos,
    string? chofer,
    string? placaCabezal,
    string? viajeDua,
    DateTime? fechaHoraIngreso,
    string? tamano,
    bool activo = true)
    {
        if (string.IsNullOrWhiteSpace(contenedor) || string.IsNullOrWhiteSpace(chasis))
            return RedirectToAction("Vehiculos");

        contenedor = contenedor.ToUpper().Trim();
        chasis = chasis.ToUpper().Trim();

        var existe = await _context.Vehiculos
            .AnyAsync(x => x.Chasis == chasis && x.Activo);

        if (existe)
            return RedirectToAction("Vehiculos");

        var vehiculo = new Vehiculo
        {
            Contenedor = contenedor,
            Chasis = chasis,
            Cliente = cliente ?? "",
            Transportista = transportista ?? "-",
            Marchamos = string.IsNullOrWhiteSpace(marchamos) ? "S/M" : marchamos,
            Chofer = chofer ?? "-",
            PlacaCabezal = string.IsNullOrWhiteSpace(placaCabezal) ? "S/P" : placaCabezal,
            ViajeDua = viajeDua ?? "-",
            FechaHoraIngreso = fechaHoraIngreso ?? DateTime.Now,
            Tamano = string.IsNullOrWhiteSpace(tamano) ? "VEHICULO" : tamano,
            Activo = activo
        };

        _context.Vehiculos.Add(vehiculo);
        await _context.SaveChangesAsync();

        return RedirectToAction("Vehiculos");
    }

    //exportaciones
    //PDF
    [HttpPost]
    public IActionResult ExportarVehiculosPDF(string nombre)
    {
        var datos = _context.Vehiculos
            .OrderByDescending(x => x.FechaHoraIngreso)
            .ToList();

        int total = datos.Count;

        QuestPDF.Settings.License = LicenseType.Community;

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);

                // ===== HEADER =====
                page.Header().Column(col =>
                {
                    col.Item().Text("ALFIPAC – VEHÍCULOS EN ALMACÉN")
                        .Bold().FontSize(18);

                    col.Item().LineHorizontal(1);

                    col.Item().Text($"Impreso por: {nombre}");
                    col.Item().Text($"Fecha de impresión: {DateTime.Now:dd/MM/yyyy HH:mm}");

                    col.Item().LineHorizontal(1);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Element(BoxStyle).Column(x =>
                        {
                            x.Item().Text("TOTAL VEHÍCULOS").Bold();
                            x.Item().Text(total.ToString()).FontSize(16);
                        });
                    });
                });

                // ===== TABLA =====
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    void Header(string text) =>
                        table.Cell().Element(HeaderCell).Text(text).Bold();

                    Header("Ingreso");
                    Header("Vehículo");
                    Header("VIN");
                    Header("Cliente");

                    foreach (var v in datos)
                    {
                        table.Cell().Element(Cell)
                            .Text(v.FechaHoraIngreso.ToString("dd/MM/yyyy HH:mm"));

                        table.Cell().Element(Cell).Text(v.Contenedor);
                        table.Cell().Element(Cell).Text(v.Chasis);
                        table.Cell().Element(Cell).Text(v.Cliente);
                    }
                });
            });
        }).GeneratePdf();

        return File(pdf, "application/pdf",
            $"Vehiculos_{DateTime.Now:dd-MM-yyyy_HH-mm}.pdf");


        // ===== ESTILOS =====
        static IContainer HeaderCell(IContainer c) =>
            c.Background(Colors.Grey.Lighten3)
             .BorderBottom(1)
             .BorderColor(Colors.Grey.Medium)
             .Padding(5)
             .AlignCenter();

        static IContainer Cell(IContainer c) =>
            c.BorderBottom(1)
             .BorderColor(Colors.Grey.Lighten2)
             .Padding(4);

        static IContainer BoxStyle(IContainer c) =>
            c.Border(1)
             .BorderColor(Colors.Grey.Medium)
             .Padding(6)
             .Background(Colors.Grey.Lighten4);
    }

    //Excel
    [HttpPost]
    public IActionResult ExportarVehiculosExcel(string nombre)
    {
        var datos = _context.Vehiculos
            .OrderByDescending(x => x.FechaHoraIngreso)
            .ToList();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Vehículos");

        // ===== TITULOS =====
        ws.Cell("A1").Value = "ALFIPAC";
        ws.Range("A1:D1").Merge().Style.Font.SetBold().Font.SetFontSize(18)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Cell("A2").Value = "VEHÍCULOS EN ALMACÉN";
        ws.Range("A2:D2").Merge().Style.Font.SetBold()
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Cell("A4").Value = "Encargado:";
        ws.Cell("B4").Value = nombre;

        ws.Cell("C4").Value = "Fecha:";
        ws.Cell("D4").Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        int fila = 6;

        // ===== HEADERS =====
        ws.Cell(fila, 1).Value = "Ingreso";
        ws.Cell(fila, 2).Value = "Vehículo";
        ws.Cell(fila, 3).Value = "VIN";
        ws.Cell(fila, 4).Value = "Cliente";

        ws.Range(fila, 1, fila, 4).Style.Font.SetBold()
            .Fill.SetBackgroundColor(XLColor.LightGray)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        fila++;

        // ===== DATA =====
        foreach (var v in datos)
        {
            ws.Cell(fila, 1).Value = v.FechaHoraIngreso.ToString("dd/MM/yyyy HH:mm");
            ws.Cell(fila, 2).Value = v.Contenedor;
            ws.Cell(fila, 3).Value = v.Chasis;
            ws.Cell(fila, 4).Value = v.Cliente;

            fila++;
        }

        ws.Columns().AdjustToContents();

        ws.Range(6, 1, fila - 1, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        ws.Range(6, 1, fila - 1, 4).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Vehiculos_{DateTime.Now:dd-MM-yyyy_HH-mm}.xlsx");
    }

    //editar vehiculo
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarVehiculo(
    int id,
    string contenedor,
    string chasis,
    string? cliente,
    string? transportista,
    string? marchamos,
    string? chofer,
    string? placaCabezal,
    string? viajeDua,
    bool activo)
    {
        var vehiculo = await _context.Vehiculos.FindAsync(id);

        if (vehiculo == null)
            return RedirectToAction("Vehiculos");

        contenedor = contenedor.ToUpper().Trim();
        chasis = chasis.ToUpper().Trim();

        // 🔥 evitar duplicados de VIN en otro registro activo
        var existe = await _context.Vehiculos
            .AnyAsync(x => x.Chasis == chasis && x.Id != id && x.Activo);

        if (existe)
            return RedirectToAction("Vehiculos");

        vehiculo.Contenedor = contenedor;
        vehiculo.Chasis = chasis;
        vehiculo.Cliente = cliente ?? "";
        vehiculo.Transportista = transportista ?? "-";
        vehiculo.Marchamos = string.IsNullOrWhiteSpace(marchamos) ? "S/M" : marchamos;
        vehiculo.Chofer = chofer ?? "-";
        vehiculo.PlacaCabezal = string.IsNullOrWhiteSpace(placaCabezal) ? "S/P" : placaCabezal;
        vehiculo.ViajeDua = viajeDua ?? "-";
        vehiculo.Activo = activo;

        await _context.SaveChangesAsync();

        return RedirectToAction("Vehiculos");
    }

}
