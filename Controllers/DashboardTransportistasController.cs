using BitacoraAlfipac.Data;
using BitacoraAlfipac.Models.Entidades;
using BitacoraAlfipac.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BitacoraAlfipac.Controllers
{
    public class DashboardTransportistasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardTransportistasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? fecha)
        {
            DateTime fechaFiltro = fecha?.Date ?? DateTime.Today;

            var registros = await _context.RegistroTransportistas
                .Where(x => x.FechaRegistro.Date == fechaFiltro)
                .ToListAsync();

            var vm = new DashboardTransportistasGlobalViewModel
            {
                FechaFiltro = fechaFiltro,

                Registrados = registros
                    .Where(x => x.FechaHoraIngreso == null && x.FechaHoraSalida == null)
                    .OrderByDescending(x => x.FechaRegistro)
                    .ToList(),

                Ingresados = registros
                    .Where(x => x.FechaHoraIngreso != null && x.FechaHoraSalida == null)
                    .OrderByDescending(x => x.FechaHoraIngreso)
                    .ToList(),

                Salidos = registros
                    .Where(x => x.FechaHoraSalida != null)
                    .OrderByDescending(x => x.FechaHoraSalida)
                    .ToList()
            };

            return View(vm);
        }

        // =====================================================
        // DETALLE
        // =====================================================
        public async Task<IActionResult> Detalle(int id)
        {
            var registro = await _context
                .RegistroTransportistas
                .FirstOrDefaultAsync(x => x.Id == id);

            if (registro == null)
                return NotFound();

            return View(registro);
        }

        private async Task<List<RegistroTransportista>> ObtenerRegistrosRango(
        DateTime fechaInicio,
        DateTime fechaFin)
        {
            fechaInicio = fechaInicio.Date;
            fechaFin = fechaFin.Date.AddDays(1).AddSeconds(-1);

            return await _context.RegistroTransportistas
                .Where(x => x.FechaRegistro >= fechaInicio
                         && x.FechaRegistro <= fechaFin)
                .ToListAsync();
        }

        public async Task<IActionResult> TestRango(DateTime inicio, DateTime fin)
        {
            var data = await ObtenerRegistrosRango(inicio, fin);
            return Json(data.Count);
        }

        //METODOS REUTILIZABLES
        private void PdfHeaderStandard(
    QuestPDF.Infrastructure.IContainer container,
    string titulo,
    DateTime inicio,
    DateTime fin,
    int total,
    int ingresos,
    int salidas)
        {
            container.Row(row =>
            {
                row.ConstantItem(60)
                    .Height(60)
                    .Image("wwwroot/logo.jpg");

                row.RelativeItem().Column(col =>
                {
                    col.Item().AlignCenter().Text(titulo)
                        .Bold().FontSize(18);

                    col.Item().AlignCenter().Text("CONTROL INTERNO ENTRADA / SALIDA")
                        .Bold().FontSize(16);

                    col.Item().AlignCenter().Text($"Fecha operativa: {inicio:dd/MM/yyyy} - {fin:dd/MM/yyyy}")
                        .FontSize(11);

                    col.Item().AlignCenter().Text(
                        $"Total registros: {total} | Ingresos: {ingresos} | Salidas: {salidas}"
                    )
                    .FontSize(10);

                    col.Item().PaddingTop(5).LineHorizontal(1);
                });

                row.ConstantItem(60);
            });
        }

        [HttpGet]
        public async Task<IActionResult> ExportarPdfRegistrados(DateTime inicio, DateTime fin)
        {
            var registros = await ObtenerRegistrosRango(inicio, fin);

            var data = registros
                .Where(x => x.FechaHoraIngreso == null && x.FechaHoraSalida == null)
                .OrderByDescending(x => x.FechaRegistro)
                .ToList();

            int ingresos = registros.Count(x => x.FechaHoraIngreso != null);
            int salidas = registros.Count(x => x.FechaHoraSalida != null);

            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var pdf = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4.Landscape());
                    page.Margin(20);

                    // =========================
                    // HEADER CORPORATIVO
                    // =========================
                    page.Header().Element(header =>
                    {
                        header.Row(row =>
                        {
                            row.ConstantItem(60)
                                .Height(60)
                                .Image("wwwroot/logo.jpg");

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().AlignCenter().Text("ALFIPAC – REGISTRO DE TRANSPORTISTAS")
                                    .Bold().FontSize(18);

                                col.Item().AlignCenter().Text("CONTROL INTERNO - PENDIENTES DE INGRESO")
                                    .Bold().FontSize(14);

                                col.Item().AlignCenter().Text($"Rango: {inicio:dd/MM/yyyy} - {fin:dd/MM/yyyy}")
                                    .FontSize(11);

                                col.Item().AlignCenter().Text(
                                    $"Total registros: {data.Count}"
                                )
                                .FontSize(10);

                                col.Item().PaddingTop(5).LineHorizontal(1);
                            });

                            row.ConstantItem(60);
                        });
                    });

                    // =========================
                    // TABLA
                    // =========================
                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.5f); // Hora
                            columns.RelativeColumn(1.5f); // Ubicación
                            columns.RelativeColumn(2.0f); // Placa
                            columns.RelativeColumn(2.5f); // Chofer
                            columns.RelativeColumn(2.5f); // Cliente
                            columns.RelativeColumn(1.5f); // DUA
                            columns.RelativeColumn(1.5f); // Tipo
                            columns.RelativeColumn(2.0f); // Firma
                        });

                        // HEADER
                        table.Header(header =>
                        {
                            void H(string t) =>
                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Border(1)
                                    .Padding(4)
                                    .AlignCenter()
                                    .Text(t)
                                    .Bold()
                                    .FontSize(10);

                            H("Hora");
                            H("Ubicación");
                            H("Placa");
                            H("Chofer");
                            H("Cliente");
                            H("DUA");
                            H("Tipo");
                            H("Firma");
                        });

                        int index = 0;

                        foreach (var item in data)
                        {
                            var bg = index % 2 == 0
                                ? Colors.White
                                : Colors.Grey.Lighten4;

                            static IContainer Cell(IContainer c, string bgColor) =>
                                c.Background(bgColor)
                                 .Border(0.5f)
                                 .BorderColor(Colors.Grey.Lighten1)
                                 .Padding(3)
                                 .DefaultTextStyle(x => x.FontSize(9))
                                 .ShowEntire();

                            table.Cell().Element(c => Cell(c, bg)).Text(item.FechaRegistro.ToString("HH:mm"));

                            table.Cell().Element(c => Cell(c, bg)).Text(item.Ubicacion);

                            table.Cell().Element(c => Cell(c, bg)).Text(item.Placa);

                            table.Cell().Element(c => Cell(c, bg)).Text(item.NombreChofer);

                            table.Cell().Element(c => Cell(c, bg)).Text(item.Cliente);

                            table.Cell().Element(c => Cell(c, bg)).Text(item.DUA ?? "");

                            table.Cell().Element(c => Cell(c, bg)).Text(item.Tipo);

                            // =========================
                            // FIRMA (PEQUEÑA)
                            // =========================
                            table.Cell().Element(c => Cell(c, bg)).AlignCenter().Element(cell =>
                            {
                                if (!string.IsNullOrEmpty(item.RutaFirma))
                                {
                                    cell.Height(28)
                                        .Image("wwwroot" + item.RutaFirma);
                                }
                            });

                            index++;
                        }
                    });

                    // =========================
                    // FOOTER
                    // =========================
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();

            string nombreArchivo = $"Pendientes_{inicio:yyyyMMdd}_{fin:yyyyMMdd}.pdf";

            return File(pdf, "application/pdf", nombreArchivo);
        }

        //PDF ingresados
        [HttpGet]
        public async Task<IActionResult> ExportarPdfIngresados(DateTime inicio, DateTime fin)
        {
            var registros = await ObtenerRegistrosRango(inicio, fin);

            var data = registros
                .Where(x => x.FechaHoraIngreso != null && x.FechaHoraSalida == null)
                .OrderByDescending(x => x.FechaHoraIngreso)
                .ToList();

            int ingresos = registros.Count(x => x.FechaHoraIngreso != null);
            int salidas = registros.Count(x => x.FechaHoraSalida != null);

            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var pdf = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4.Landscape());
                    page.Margin(20);

                    // =========================
                    // HEADER
                    // =========================
                    page.Header().Element(header =>
                    {
                        header.Row(row =>
                        {
                            row.ConstantItem(60)
                                .Height(60)
                                .Image("wwwroot/logo.jpg");

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().AlignCenter().Text("ALFIPAC – REGISTRO DE TRANSPORTISTAS")
                                    .Bold().FontSize(18);

                                col.Item().AlignCenter().Text("CONTROL INTERNO - INGRESADOS")
                                    .Bold().FontSize(14);

                                col.Item().AlignCenter().Text($"Rango: {inicio:dd/MM/yyyy} - {fin:dd/MM/yyyy}")
                                    .FontSize(11);

                                col.Item().AlignCenter().Text(
                                    $"Total registros: {data.Count}"
                                )
                                .FontSize(10);

                                col.Item().PaddingTop(5).LineHorizontal(1);
                            });

                            row.ConstantItem(60);
                        });
                    });

                    // =========================
                    // TABLA
                    // =========================
                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.5f); // Registro
                            columns.RelativeColumn(1.5f); // Ingreso
                            columns.RelativeColumn(1.5f); // Ubicación
                            columns.RelativeColumn(2.0f); // Placa
                            columns.RelativeColumn(2.5f); // Chofer
                            columns.RelativeColumn(2.5f); // Cliente
                            columns.RelativeColumn(1.5f); // DUA
                            columns.RelativeColumn(1.5f); // Tipo
                            columns.RelativeColumn(2.0f); // Firma
                        });

                        table.Header(header =>
                        {
                            void H(string t) =>
                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Border(1)
                                    .Padding(4)
                                    .AlignCenter()
                                    .Text(t)
                                    .Bold()
                                    .FontSize(10);

                            H("Registro");
                            H("Ingreso");
                            H("Ubicación");
                            H("Placa");
                            H("Chofer");
                            H("Cliente");
                            H("DUA");
                            H("Tipo");
                            H("Firma");
                        });

                        int index = 0;

                        foreach (var item in data)
                        {
                            var bg = index % 2 == 0
                                ? Colors.White
                                : Colors.Grey.Lighten4;

                            static IContainer Cell(IContainer c, string bgColor) =>
                                c.Background(bgColor)
                                 .Border(0.5f)
                                 .BorderColor(Colors.Grey.Lighten1)
                                 .Padding(3)
                                 .DefaultTextStyle(x => x.FontSize(9))
                                 .ShowEntire();

                            table.Cell().Element(c => Cell(c, bg))
                                .Text(item.FechaRegistro.ToString("HH:mm"));

                            table.Cell().Element(c => Cell(c, bg))
                                .Text(item.FechaHoraIngreso?.ToString("HH:mm") ?? "");

                            table.Cell().Element(c => Cell(c, bg))
                                .Text(item.Ubicacion);

                            table.Cell().Element(c => Cell(c, bg))
                                .Text(item.Placa);

                            table.Cell().Element(c => Cell(c, bg))
                                .Text(item.NombreChofer);

                            table.Cell().Element(c => Cell(c, bg))
                                .Text(item.Cliente);

                            table.Cell().Element(c => Cell(c, bg))
                                .Text(item.DUA ?? "");

                            table.Cell().Element(c => Cell(c, bg))
                                .Text(item.Tipo);

                            table.Cell().Element(c => Cell(c, bg)).AlignCenter().Element(cell =>
                            {
                                if (!string.IsNullOrEmpty(item.RutaFirma))
                                {
                                    cell.Height(28)
                                        .Image("wwwroot" + item.RutaFirma);
                                }
                            });

                            index++;
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();

            string nombreArchivo = $"Ingresados_{inicio:yyyyMMdd}_{fin:yyyyMMdd}.pdf";

            return File(pdf, "application/pdf", nombreArchivo);
        }

        // PDF SALIDOS
        [HttpGet]
        public async Task<IActionResult> ExportarPdfSalidos(DateTime inicio, DateTime fin)
        {
            var registros = await ObtenerRegistrosRango(inicio, fin);

            var data = registros
                .Where(x => x.FechaHoraSalida != null)
                .OrderByDescending(x => x.FechaHoraSalida)
                .ToList();

            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var pdf = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4.Landscape());
                    page.Margin(20);

                    // =========================
                    // HEADER
                    // =========================
                    page.Header().Row(row =>
                    {
                        row.ConstantItem(60)
                            .Height(60)
                            .Image("wwwroot/logo.jpg");

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignCenter().Text("ALFIPAC – REGISTRO DE TRANSPORTISTAS")
                                .Bold().FontSize(18);

                            col.Item().AlignCenter().Text("CONTROL INTERNO - SALIDOS")
                                .Bold().FontSize(14);

                            col.Item().AlignCenter().Text($"Rango: {inicio:dd/MM/yyyy} - {fin:dd/MM/yyyy}")
                                .FontSize(11);

                            col.Item().AlignCenter().Text(
                                $"Total registros: {data.Count}"
                            )
                            .FontSize(10);

                            col.Item().PaddingTop(5).LineHorizontal(1);
                        });

                        row.ConstantItem(60);
                    });

                    // =========================
                    // TABLA
                    // =========================
                    page.Content().PaddingTop(10).Table(table =>
                    {
                        // =========================
                        // COLUMNAS AJUSTADAS (BALANCE FINAL)
                        // =========================
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.1f); // Registro
                            columns.RelativeColumn(1.1f); // Ingreso
                            columns.RelativeColumn(1.1f); // Salida
                            columns.RelativeColumn(1.3f); // Tiempo E/S
                            columns.RelativeColumn(1.6f); // Ubicación
                            columns.RelativeColumn(1.3f); // Placa
                            columns.RelativeColumn(2.0f); // Chofer
                            columns.RelativeColumn(2.0f); // Cliente
                            columns.RelativeColumn(1.8f); // DUA
                            columns.RelativeColumn(2.2f); // Tipo
                            columns.RelativeColumn(1.8f); // Firma
                        });

                        // =========================
                        // HEADER TABLA
                        // =========================
                        table.Header(header =>
                        {
                            void H(string t) =>
                                header.Cell()
                                    .Background(Colors.Grey.Lighten2)
                                    .Border(1)
                                    .Padding(4)
                                    .AlignCenter()
                                    .Text(t)
                                    .Bold()
                                    .FontSize(10);

                            H("Registro");
                            H("Ingreso");
                            H("Salida");
                            H("E/S");
                            H("Ubicación");
                            H("Placa");
                            H("Chofer");
                            H("Cliente");
                            H("DUA");
                            H("Tipo");
                            H("Firma");
                        });

                        int index = 0;

                        foreach (var item in data)
                        {
                            var bg = index % 2 == 0
                                ? Colors.White
                                : Colors.Grey.Lighten4;

                            static IContainer Cell(IContainer c, string bgColor) =>
                                c.Background(bgColor)
                                 .Border(0.5f)
                                 .BorderColor(Colors.Grey.Lighten1)
                                 .Padding(3)
                                 .DefaultTextStyle(x => x.FontSize(9))
                                 .ShowEntire();

                            string tiempo = "";

                            if (item.FechaHoraIngreso.HasValue && item.FechaHoraSalida.HasValue)
                            {
                                var diff = item.FechaHoraSalida.Value - item.FechaHoraIngreso.Value;
                                tiempo = $"{(int)diff.TotalHours}h {diff.Minutes}m";
                            }

                            table.Cell().Element(c => Cell(c, bg))
                                .Text(item.FechaRegistro.ToString("HH:mm"));

                            table.Cell().Element(c => Cell(c, bg))
                                .Text(item.FechaHoraIngreso?.ToString("HH:mm") ?? "");

                            table.Cell().Element(c => Cell(c, bg))
                                .Text(item.FechaHoraSalida?.ToString("HH:mm") ?? "");

                            table.Cell().Element(c => Cell(c, bg))
                                .AlignCenter()
                                .Text(tiempo);

                            table.Cell().Element(c => Cell(c, bg))
                                .Text(item.Ubicacion);

                            table.Cell().Element(c => Cell(c, bg))
                                .Text(item.Placa);

                            table.Cell().Element(c => Cell(c, bg))
                                .Text(item.NombreChofer);

                            table.Cell().Element(c => Cell(c, bg))
                                .Text(item.Cliente);

                            table.Cell().Element(c => Cell(c, bg))
                                .Text(item.DUA ?? "");

                            // =========================
                            // TIPO (MÁXIMA PRIORIDAD VISUAL)
                            // =========================
                            table.Cell().Element(c => Cell(c, bg))
                                .Text(item.Tipo);

                            // =========================
                            // FIRMA
                            // =========================
                            table.Cell().Element(c => Cell(c, bg))
                                .AlignCenter()
                                .Element(cell =>
                                {
                                    if (!string.IsNullOrEmpty(item.RutaFirma))
                                    {
                                        cell.Height(28)
                                            .Image("wwwroot" + item.RutaFirma);
                                    }
                                    else
                                    {
                                        cell.Text("Sin firma");
                                    }
                                });

                            index++;
                        }
                    });

                    // =========================
                    // FOOTER
                    // =========================
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();

            string nombreArchivo = $"Salidos_{inicio:yyyyMMdd}_{fin:yyyyMMdd}.pdf";

            return File(pdf, "application/pdf", nombreArchivo);
        }
    }
}