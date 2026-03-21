using BitacoraAlfipac.Data;
using BitacoraAlfipac.Models.ViewModels;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class BitacoraController : Controller
{
    private readonly ApplicationDbContext _context;

    public BitacoraController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ===============================
    // 📅 BITÁCORA POR DÍA
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

        // 🔹 DESPACHOS DEL DÍA
        var despachos = await _context.BitacoraDespachos
            .Where(d => d.FechaHoraDespacho.Date == dia)
            .Select(d => new BitacoraDiaVM
            {
                Contenedor = d.ContenedorReferencia != null && d.ContenedorReferencia != ""
                                ? "REF: " + d.ContenedorReferencia
                                : d.Contenedor,
                Marchamos = d.Marchamos,
                HoraEntrada = null,
                HoraSalida = d.FechaHoraDespacho,
                HoraOrden = d.FechaHoraDespacho,
                Transportista = d.Transportista,
                Informacion = d.Informacion, // 🔑 Aquí va cliente
                Chofer = d.Chofer,
                Placa = d.PlacaCabezal,
                Chasis = d.Chasis,
                ViajeODua = d.ViajeDua
            })
            .ToListAsync();

        // 🔥 UNIÓN Y ORDEN CRONOLÓGICO
        var bitacora = ingresos
            .Concat(despachos)
            .OrderBy(x => x.HoraOrden)
            .ToList();

        ViewBag.FechaSeleccionada = dia;

        return View(bitacora);
    }

    [HttpGet]
    public IActionResult ExportarPdf(DateTime fecha)
    {
        var movimientos = ObtenerMovimientosPorFecha(fecha);

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

                        col.Item().AlignCenter().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
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

                    void Header(string t) =>
                        table.Cell().Element(HeaderStyle).Text(t).Bold().FontSize(10).AlignCenter();

                    Header("Contenedor");
                    Header("Marchamos");
                    Header("Entrada");
                    Header("Salida");
                    Header("Transportista");
                    Header("Información");
                    Header("Chofer");
                    Header("Placa");
                    Header("Chasis");
                    Header("Viaje/DUA");

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
                         .DefaultTextStyle(x => x.FontSize(9));
                });
            });
        }).GeneratePdf();

        return File(pdf, "application/pdf",
            $"Bitacora_{fecha:dd-MM-yyyy}.pdf");
    }

    //Exportar a Excel
    [HttpGet]
    public IActionResult ExportarExcel(DateTime fecha)
    {
        var movimientos = ObtenerMovimientosPorFecha(fecha);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Bitácora");

        // =============================================
        // Encabezados
        // =============================================
        var headers = new[]
        {
        "Contenedor", "Marchamos", "Entrada", "Salida",
        "Transportista", "Información", "Chofer", "Placa",
        "Chasis", "Viaje/DUA"
    };

        var headerRow = ws.Row(1);
        for (int col = 1; col <= headers.Length; col++)
        {
            var cell = ws.Cell(1, col);
            cell.Value = headers[col - 1];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.Black;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E0E0E0"); // gris claro
        }

        // Congelar encabezados
        ws.SheetView.FreezeRows(1);

        // =============================================
        // Datos
        // =============================================
        int fila = 2;
        foreach (var m in movimientos)
        {
            ws.Cell(fila, 1).Value = m.Contenedor ?? "";
            ws.Cell(fila, 2).Value = m.Marchamos ?? "";
            ws.Cell(fila, 3).Value = m.HoraEntrada?.ToString("HH:mm") ?? "";
            ws.Cell(fila, 4).Value = m.HoraSalida?.ToString("HH:mm") ?? "";
            ws.Cell(fila, 5).Value = m.Transportista ?? "";
            ws.Cell(fila, 6).Value = m.Informacion ?? "";
            ws.Cell(fila, 7).Value = m.Chofer ?? "";
            ws.Cell(fila, 8).Value = m.Placa ?? "";
            ws.Cell(fila, 9).Value = m.Chasis ?? "";
            ws.Cell(fila, 10).Value = m.ViajeODua ?? "";   // ← ajusta si el nombre real es ViajeDua o similar

            fila++;
        }

        // =============================================
        // Formato de columnas
        // =============================================
        var dataRange = ws.RangeUsed();

        // Alineaciones generales
        dataRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        // Columnas de texto (izquierda)
        ws.Columns("A,B,E,F,G,H,I,J,K").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        // Columnas de hora (derecha)
        ws.Columns("C:D").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

        // Anchos recomendados (puedes ajustar estos valores)
        ws.Column(1).Width = 18;   // Contenedor
        ws.Column(2).Width = 14;   // Marchamos
        ws.Column(3).Width = 10;   // Entrada
        ws.Column(4).Width = 10;   // Salida
        ws.Column(5).Width = 25;   // Transportista
        ws.Column(6).Width = 40;   // Información (la más ancha normalmente)
        ws.Column(7).Width = 22;   // Chofer
        ws.Column(8).Width = 14;   // Placa
        ws.Column(9).Width = 14;   // Chasis
        ws.Column(10).Width = 18;  // Viaje/DUA

        // Bordes alrededor de toda la tabla
        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        dataRange.Style.Border.OutsideBorderColor = XLColor.Gray;
        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        dataRange.Style.Border.InsideBorderColor = XLColor.LightGray;

        // Ajuste final automático (por si algunos textos son muy largos)
        ws.Columns().AdjustToContents();

        // =============================================
        // Exportar
        // =============================================
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        string fileName = $"Bitacora_{fecha:dd-MM-yyyy}.xlsx";

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName
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
            Contenedor = d.ContenedorReferencia != null && d.ContenedorReferencia != ""
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

    return ingresos.Concat(despachos).OrderBy(x => x.HoraOrden).ToList();
}

}


