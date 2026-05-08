using BitacoraAlfipac.Data;
using BitacoraAlfipac.Models.ViewModels;
using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class BitacoraController : Controller
{
    private readonly ApplicationDbContext _context;

    //ENTORNO
    private readonly IWebHostEnvironment _env;
    public BitacoraController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // ===============================
    // BITÁCORA POR DÍA
    // ===============================
    public async Task<IActionResult> Index(DateTime? fecha)
    {
        DateTime dia = fecha?.Date ?? DateTime.Today;

        // 🔹 INGRESOS DEL DÍA
        var ingresos = await _context.BitacoraIngresos
            .Where(i => i.FechaHoraIngreso.Date == dia)
            .Select(i => new BitacoraDiaVM
            {
                Contenedor = i.Contenedor,
                Marchamos = i.Marchamos,
                HoraEntrada = i.FechaHoraIngreso,
                HoraSalida = null,
                HoraOrden = i.FechaHoraIngreso,
                Transportista = i.Transportista,
                Informacion = i.Tamaño, // 🔑 Aquí va tamaño
                Chofer = i.Chofer,
                Placa = i.PlacaCabezal,
                Chasis = i.Chasis,
                ViajeODua = i.ViajeDua
            })
            .ToListAsync();

        // 🔹 DESPACHOS DEL DÍA (FASE 1: traer datos simples)
        var despachosRaw = await _context.BitacoraDespachos
            .Where(d => d.FechaHoraDespacho.Date == dia)
            .Select(d => new
            {
                d.Contenedor,
                d.ContenedorReferencia,
                d.GuardarContenedorSalida,
                d.Marchamos,
                d.FechaHoraDespacho,
                d.Transportista,
                d.Informacion,
                d.Chofer,
                d.PlacaCabezal,
                d.Chasis,
                d.ViajeDua
            })
            .ToListAsync();

        //fase 2
        var despachos = despachosRaw.Select(d => new BitacoraDiaVM
            {
                Contenedor =
            d.GuardarContenedorSalida && !string.IsNullOrEmpty(d.ContenedorReferencia)
                ? $"{d.Contenedor} / REF: {d.ContenedorReferencia}"
            : !string.IsNullOrEmpty(d.ContenedorReferencia)
                ? "REF: " + d.ContenedorReferencia
            : d.Contenedor,

                Marchamos = d.Marchamos,
                HoraEntrada = null,
                HoraSalida = d.FechaHoraDespacho,
                HoraOrden = d.FechaHoraDespacho,
                Transportista = d.Transportista,
                Informacion = d.Informacion,
                Chofer = d.Chofer,
                Placa = d.PlacaCabezal,
                Chasis = d.Chasis,
                ViajeODua = d.ViajeDua
            })
            .ToList();

        // 🔥 UNIÓN Y ORDEN CRONOLÓGICO
        var bitacora = ingresos
            .Concat(despachos)
            .OrderBy(x => x.HoraOrden)
            .ToList();

        ViewBag.FechaSeleccionada = dia;

        return View(bitacora);
    }

    [HttpGet]
    public IActionResult ExportarPdf(DateTime fecha, TimeSpan? horaInicio, TimeSpan? horaFin)
    {
        var movimientos = ObtenerMovimientosPorFecha(fecha);

        // FILTRO NUEVO
        if (horaInicio.HasValue && horaFin.HasValue)
        {
            movimientos = movimientos
                .Where(m =>
                    (m.HoraEntrada.HasValue &&
                     m.HoraEntrada.Value.TimeOfDay >= horaInicio &&
                     m.HoraEntrada.Value.TimeOfDay <= horaFin)
                 ||
                    (m.HoraSalida.HasValue &&
                     m.HoraSalida.Value.TimeOfDay >= horaInicio &&
                     m.HoraSalida.Value.TimeOfDay <= horaFin)
                )
                .ToList();
        }

        // CONTADORES
        int totalIngresos = movimientos.Count(m => m.HoraEntrada.HasValue);
        int totalDespachos = movimientos.Count(m => m.HoraSalida.HasValue);

        QuestPDF.Settings.License = LicenseType.Community;

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);

                page.Header().Row(row =>
                {
                    // LOGO IZQUIERDA
                    row.ConstantItem(60).Height(60).Image("wwwroot/logo.jpg");

                    // TEXTO CENTRO
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().AlignCenter().Text("ALFIPAC – BITÁCORA OPERATIVA DIARIA")
                            .Bold().FontSize(18);

                        col.Item().AlignCenter().Text("CONTROL INTERNO ENTRADA / SALIDA")
                            .Bold().FontSize(18);

                        col.Item().AlignCenter().Text($"Fecha operativa: {fecha:dd/MM/yyyy}")
                            .FontSize(11);

                        //col.Item().AlignCenter().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                        //    .FontSize(10)
                        //    .FontColor(Colors.Grey.Darken1);

                        // NUEVO: RANGO
                        if (horaInicio.HasValue && horaFin.HasValue)
                        {
                            col.Item().AlignCenter().Text($"Horario: {horaInicio:hh\\:mm} - {horaFin:hh\\:mm}")
                                .FontSize(11)
                                .FontColor(Colors.Grey.Darken1);
                        }
                        else
                        {
                        col.Item().AlignCenter().Text("Horario: Todo el día")
                                .FontSize(11)
                                .FontColor(Colors.Grey.Darken1);
                        }

                        // NUEVO: TOTAL
                        col.Item().AlignCenter().Text(
                            $"Total registros: {movimientos.Count} | " +
                            $"Ingresos: {totalIngresos} | " +
                            $"Despachos: {totalDespachos}"
                        )
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);

                        col.Item().PaddingTop(5).LineHorizontal(1);

                    });

                    // ESPACIO DERECHO (para balance visual)
                    row.ConstantItem(60);
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2.2f);
                        columns.RelativeColumn(2.0f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(2.4f);
                        columns.RelativeColumn(2.8f);
                        columns.RelativeColumn(2.1f);
                        columns.RelativeColumn(1.4f);
                        columns.RelativeColumn(1.8f);
                        columns.RelativeColumn(2.0f);
                    });

                    table.Header(header =>
                    {
                        void H(string t) =>
                            header.Cell().Element(HeaderStyle).Text(t).Bold().FontSize(10).AlignCenter();

                        H("Contenedor");
                        H("Marchamos");
                        H("Entrada");
                        H("Salida");
                        H("Transportista");
                        H("Información");
                        H("Chofer");
                        H("Placa");
                        H("Chasis");
                        H("Viaje/DUA");
                    });

                    int index = 0;

                    foreach (var m in movimientos)
                    {
                        var bg = index % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        table.Cell().Element(c => CellStyle(c, bg)).Text(m.Contenedor);
                        table.Cell().Element(c => CellStyle(c, bg)).Text(m.Marchamos);
                        table.Cell().Element(c => CellStyle(c, bg)).AlignCenter().Text(m.HoraEntrada?.ToString("HH:mm") ?? "");
                        table.Cell().Element(c => CellStyle(c, bg)).AlignCenter().Text(m.HoraSalida?.ToString("HH:mm") ?? "");
                        table.Cell().Element(c => CellStyle(c, bg)).Text(m.Transportista);
                        table.Cell().Element(c => CellStyle(c, bg)).Text(m.Informacion);
                        table.Cell().Element(c => CellStyle(c, bg)).Text(m.Chofer);
                        table.Cell().Element(c => CellStyle(c, bg)).AlignCenter().Text(m.Placa);
                        table.Cell().Element(c => CellStyle(c, bg)).Text(m.Chasis);
                        table.Cell().Element(c => CellStyle(c, bg)).Text(m.ViajeODua);

                        index++;
                    }

                    static IContainer HeaderStyle(IContainer c) =>
                        c.Background(Colors.Grey.Lighten2)
                         .Border(1)
                         .PaddingVertical(4)
                         .PaddingHorizontal(3);

                    static IContainer CellStyle(IContainer c, string bg) =>
                        c.Background(bg)
                         .Border(0.5f)
                         .BorderColor(Colors.Grey.Lighten1)
                         .Padding(3)
                         .DefaultTextStyle(x => x.FontSize(9))
                         .ShowEntire(); //No parte renglon entre paginas
                });

                page.Footer().Element(footer =>
                {
                    footer.AlignCenter().Text(txt =>
                    {
                        txt.Span("Página ");
                        txt.CurrentPageNumber();
                        txt.Span(" de ");
                        txt.TotalPages();
                    });
                });
            });
        }).GeneratePdf();

        string nombreArchivo;

        // SIN FILTRO (todo el día)
        if (!horaInicio.HasValue || !horaFin.HasValue)
        {
            nombreArchivo = $"Bitacora_{fecha:dd-MM-yyyy}.pdf";
        }
        else
        {
            // CON RANGO
            nombreArchivo = $"Bitacora_{fecha:dd-MM-yyyy}_{horaInicio:hh\\-mm}hrs_{horaFin:hh\\-mm}hrs.pdf";
        }

        return File(pdf, "application/pdf", nombreArchivo);
    }

    //Exportar a Excel
    [HttpGet]
    public IActionResult ExportarExcel(DateTime fecha)
    {
        var movimientos = ObtenerMovimientosPorFecha(fecha);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Bitácora");

        // =============================================
        // ENCABEZADO TIPO PDF
        // =============================================

        int fila = 1;

        // =============================================
        // 🖼️ LOGO (IZQUIERDA)
        // =============================================
        var logoPath = Path.Combine(_env.WebRootPath, "images", "logo.jpg");

        if (System.IO.File.Exists(logoPath))
        {
            var image = ws.AddPicture(logoPath)
                .MoveTo(ws.Cell("A1"))
                .WithPlacement(XLPicturePlacement.FreeFloating);

            image.Width = 80;   // 🔥 controla tamaño
            image.Height = 60;
        }

        // =============================================
        // 🧾 TEXTO (CENTRADO COMO PDF)
        // =============================================

        // usamos columnas C a J para centrar visualmente
        ws.Range(fila, 3, fila, 10).Merge().Value =
            "ALFIPAC – BITÁCORA OPERATIVA DIARIA";

        ws.Range(fila, 3, fila, 10).Style
            .Font.SetBold().Font.SetFontSize(16)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        fila++;

        ws.Range(fila, 3, fila, 10).Merge().Value =
            "CONTROL INTERNO ENTRADA / SALIDA";

        ws.Range(fila, 3, fila, 10).Style
            .Font.SetBold().Font.SetFontSize(14)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        fila++;

        ws.Range(fila, 3, fila, 10).Merge().Value =
            $"Fecha operativa: {fecha:dd/MM/yyyy}";

        ws.Range(fila, 3, fila, 10).Style
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        fila++;

        ws.Range(fila, 3, fila, 10).Merge().Value =
            $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

        ws.Range(fila, 3, fila, 10).Style
            .Font.SetFontColor(XLColor.Gray)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        fila++;

        // Línea separadora
        ws.Range(fila, 1, fila, 10).Style.Border.BottomBorder = XLBorderStyleValues.Thin;

        fila += 2;

        // =============================================
        // 📊 ENCABEZADOS TABLA
        // =============================================

        var headers = new[]
        {
        "Contenedor", "Marchamos", "Entrada", "Salida",
        "Transportista", "Información", "Chofer", "Placa",
        "Chasis", "Viaje/DUA"
    };

        for (int col = 1; col <= headers.Length; col++)
        {
            var cell = ws.Cell(fila, col);
            cell.Value = headers[col - 1];

            cell.Style.Font.Bold = true;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        // Congelar encabezado
        ws.SheetView.FreezeRows(fila);

        fila++;

        // =============================================
        // 📄 DATOS CON ESTILO ZEBRA
        // =============================================

        int index = 0;

        foreach (var m in movimientos)
        {
            var bgColor = index % 2 == 0 ? XLColor.White : XLColor.FromHtml("#F5F5F5");

            ws.Cell(fila, 1).Value = m.Contenedor ?? "";
            ws.Cell(fila, 2).Value = m.Marchamos ?? "";
            ws.Cell(fila, 3).Value = m.HoraEntrada?.ToString("HH:mm") ?? "";
            ws.Cell(fila, 4).Value = m.HoraSalida?.ToString("HH:mm") ?? "";
            ws.Cell(fila, 5).Value = m.Transportista ?? "";
            ws.Cell(fila, 6).Value = m.Informacion ?? "";
            ws.Cell(fila, 7).Value = m.Chofer ?? "";
            ws.Cell(fila, 8).Value = m.Placa ?? "";
            ws.Cell(fila, 9).Value = m.Chasis ?? "";
            ws.Cell(fila, 10).Value = m.ViajeODua ?? "";

            for (int col = 1; col <= 10; col++)
            {
                var cell = ws.Cell(fila, col);

                cell.Style.Fill.BackgroundColor = bgColor;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.OutsideBorderColor = XLColor.LightGray;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Font.FontSize = 10;
            }

            fila++;
            index++;
        }

        // =============================================
        // 🎯 FORMATO FINAL
        // =============================================

        // Alineaciones
        ws.Columns("A,B,E,F,G,H,I,J").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        ws.Columns("C:D").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Anchos tipo PDF
        ws.Column(1).Width = 18;
        ws.Column(2).Width = 14;
        ws.Column(3).Width = 10;
        ws.Column(4).Width = 10;
        ws.Column(5).Width = 25;
        ws.Column(6).Width = 40;
        ws.Column(7).Width = 22;
        ws.Column(8).Width = 14;
        ws.Column(9).Width = 14;
        ws.Column(10).Width = 18;

        ws.Columns().AdjustToContents();

        // =============================================
        // 📦 EXPORTAR
        // =============================================

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Bitacora_{fecha:dd-MM-yyyy}.xlsx"
        );
    }

    private List<BitacoraDiaVM> ObtenerMovimientosPorFecha(DateTime fecha)
{
    DateTime dia = fecha.Date;

    var ingresos = _context.BitacoraIngresos
        .Where(i => i.FechaHoraIngreso.Date == dia)
        .Select(i => new BitacoraDiaVM
        {
            Contenedor = i.Contenedor,
            Marchamos = i.Marchamos,
            HoraEntrada = i.FechaHoraIngreso,
            HoraSalida = null,
            HoraOrden = i.FechaHoraIngreso,
            Transportista = i.Transportista,
            Informacion = i.Tamaño,
            Chofer = i.Chofer,
            Placa = i.PlacaCabezal,
            Chasis = i.Chasis,
            ViajeODua = i.ViajeDua
        })
        .ToList();

    var despachos = _context.BitacoraDespachos
        .Where(d => d.FechaHoraDespacho.Date == dia)
        .Select(d => new BitacoraDiaVM
        {
            Contenedor =
            !string.IsNullOrWhiteSpace(d.ContenedorReferencia)
                ? $"{d.Contenedor} / REF: {d.ContenedorReferencia}"
                : d.Contenedor,
            Marchamos = d.Marchamos,
            HoraEntrada = null,
            HoraSalida = d.FechaHoraDespacho,
            HoraOrden = d.FechaHoraDespacho,
            Transportista = d.Transportista,
            Informacion = d.Informacion,
            Chofer = d.Chofer,
            Placa = d.PlacaCabezal,
            Chasis = d.Chasis,
            ViajeODua = d.ViajeDua
        })
        .ToList();

    return ingresos.Concat(despachos).OrderBy(x => x.HoraOrden).ToList();
}

}
