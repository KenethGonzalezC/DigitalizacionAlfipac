using BitacoraAlfipac.Data;
using BitacoraAlfipac.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QIContainer = QuestPDF.Infrastructure.IContainer;

namespace BitacoraAlfipac.Controllers
{
    public class VaciosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;


        public VaciosController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env; 
        }

        //lista de contenedores vacios
        public async Task<IActionResult> Index(
    string? contenedor,
    string? cliente,
    string? transportista,
    string? patio)
        {
            var sinAsignar = await _context.ContenedoresSinAsignarPatio
                .Select(x => new InventarioItemVM
                {
                    Id = x.Id,
                    Contenedor = x.Contenedor,
                    Marchamos = x.Marchamos,
                    Tamano = x.Tamano,
                    Chasis = x.Chasis,
                    Transportista = x.Transportista,
                    Cliente = x.Cliente,
                    EstadoCarga = x.EstadoCarga,
                    Patio = "S/Pat"
                }).ToListAsync();

            var patio1 = await _context.Patio1
                .Select(x => new InventarioItemVM
                {
                    Id = x.Id,
                    Contenedor = x.Contenedor,
                    Marchamos = x.Marchamos,
                    Tamano = x.Tamano,
                    Chasis = x.Chasis,
                    Transportista = x.Transportista,
                    Cliente = x.Cliente,
                    EstadoCarga = x.EstadoCarga,
                    Patio = "P1"
                }).ToListAsync();

            var patio2 = await _context.Patio2
                .Select(x => new InventarioItemVM
                {
                    Id = x.Id,
                    Contenedor = x.Contenedor,
                    Marchamos = x.Marchamos,
                    Tamano = x.Tamano,
                    Chasis = x.Chasis,
                    Transportista = x.Transportista,
                    Cliente = x.Cliente,
                    EstadoCarga = x.EstadoCarga,
                    Patio = "P2"
                }).ToListAsync();

            var anden = await _context.Anden2000
                .Select(x => new InventarioItemVM
                {
                    Id = x.Id,
                    Contenedor = x.Contenedor,
                    Marchamos = x.Marchamos,
                    Tamano = x.Tamano,
                    Chasis = x.Chasis,
                    Transportista = x.Transportista,
                    Cliente = x.Cliente,
                    EstadoCarga = x.EstadoCarga,
                    Patio = "2000"
                }).ToListAsync();

            var quimicos = await _context.PatioQuimicos
                .Select(x => new InventarioItemVM
                {
                    Id = x.Id,
                    Contenedor = x.Contenedor,
                    Marchamos = x.Marchamos,
                    Tamano = x.Tamano,
                    Chasis = x.Chasis,
                    Transportista = x.Transportista,
                    Cliente = x.Cliente,
                    EstadoCarga = x.EstadoCarga,
                    Patio = "AgroQui"
                }).ToListAsync();

            var todos = sinAsignar
                .Concat(patio1)
                .Concat(patio2)
                .Concat(anden)
                .Concat(quimicos)
                .AsQueryable();

            // ✅ SOLO VACÍOS
            todos = todos.Where(x =>
                x.EstadoCarga != null &&
                x.EstadoCarga.Trim().ToLower() == "vacio");

            // 🔎 FILTROS
            if (!string.IsNullOrWhiteSpace(contenedor))
                todos = todos.Where(x => x.Contenedor.Contains(contenedor));

            if (!string.IsNullOrWhiteSpace(cliente))
                todos = todos.Where(x => x.Cliente.Contains(cliente));

            if (!string.IsNullOrWhiteSpace(transportista))
                todos = todos.Where(x => x.Transportista.Contains(transportista));

            if (!string.IsNullOrWhiteSpace(patio))
                todos = todos.Where(x => x.Patio == patio);

            var resultado = todos
                .OrderBy(x => x.Patio)
                .ThenBy(x => x.Contenedor)
                .ToList();

            return View(resultado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GenerarReporteVacioPDF(
    string contenedor,
    string cliente,
    string transportista,
    string consecutivo)
        {
            var pdfBytes = GenerarReporteVacio(
                contenedor,
                cliente,
                transportista,
                consecutivo);

            return File(pdfBytes,
                "application/pdf",
                $"ReporteVacio_{contenedor}+{consecutivo}_{DateTime.Now:ddMMyyyy}.pdf");
        }

        public byte[] GenerarReporteVacio(
    string contenedor,
    string consignatario,
    string transportista,
    string consecutivo)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var ahora = DateTime.Now;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(0);

                    page.Content().Column(col =>
                    {
                        // ================= HEADER =================
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Cell().ColumnSpan(1)
                                .Border(1)
                                .Image("wwwroot/images/logo.jpg");

                            table.Cell().ColumnSpan(3)
                                .Background("#2F3A7C")
                                .Border(1)
                                .AlignCenter()
                                .AlignMiddle()
                                .PaddingVertical(10) // 👈 altura controlada
                                .Text("REPORTE DE VACIO")
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.White);
                        });

                        // ================= TABLA =================
                        col.Item().Layers(layers =>
                        {
                            layers.Layer()
                            .AlignCenter()
                            .AlignMiddle()
                            .Image("wwwroot/images/logo-watermark.png", ImageScaling.FitArea);

                            layers.PrimaryLayer().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(); // FECHA
                                    columns.RelativeColumn(); // valor
                                    columns.RelativeColumn(); // HORA
                                    columns.RelativeColumn(); // valor
                                });

                                IContainer BlueCell(IContainer c) =>
                                    c.Border(1)
                                     .Background("#2F3A7C")
                                     .Padding(10)
                                     .AlignMiddle();

                                IContainer LightCell(IContainer c) =>
                                    c.Border(1)
                                     .Background("#D6E3F3")
                                     .Padding(10)
                                     .AlignMiddle();

                                // ================= FILA FECHA / HORA =================
                                table.Cell().Element(BlueCell)
                                    .Text("📅 FECHA:")
                                    .FontColor(Colors.White)
                                    .Bold();

                                table.Cell().Element(LightCell)
                                    .AlignCenter()
                                    .Text(ahora.ToString("d/M/yyyy"));

                                table.Cell().Element(BlueCell)
                                    .Text("🕒 HORA:")
                                    .FontColor(Colors.White)
                                    .Bold();

                                table.Cell().Element(LightCell)
                                    .AlignCenter()
                                    .Text(ahora.ToString("HH:mm"));

                                // ================= CONTENEDOR =================
                                table.Cell().ColumnSpan(1).Element(BlueCell)
                                    .Text("CONTENEDOR:")
                                    .FontColor(Colors.White)
                                    .Bold();

                                table.Cell().ColumnSpan(3).Element(LightCell)
                                    .AlignCenter()
                                    .Text(contenedor ?? "-")
                                    .FontSize(20)
                                    .Bold();

                                // ================= ESTADO =================
                                table.Cell().Element(BlueCell)
                                    .Text("ESTADO:")
                                    .FontColor(Colors.White)
                                    .Bold();

                                table.Cell().ColumnSpan(3).Element(LightCell)
                                    .AlignCenter()
                                    .Text("VACIO")
                                    .FontSize(22)
                                    .Bold();

                                // ================= CONSIGNATARIO =================
                                table.Cell().Element(BlueCell)
                                    .Text("CONSIGNATARIO:")
                                    .FontColor(Colors.White)
                                    .Bold();

                                table.Cell().ColumnSpan(3).Element(LightCell)
                                    .AlignCenter()
                                    .Text(consignatario ?? "0");

                                // ================= TRANSPORTISTA =================
                                table.Cell().Element(BlueCell)
                                    .Text("TRANSPORTISTA:")
                                    .FontColor(Colors.White)
                                    .Bold();

                                table.Cell().ColumnSpan(3).Element(LightCell)
                                    .AlignCenter()
                                    .Text(transportista ?? "0");

                                // ================= CONSECUTIVO =================
                                table.Cell().Element(BlueCell)
                                    .Text("CONSECUTIVO:")
                                    .FontColor(Colors.White)
                                    .Bold();

                                table.Cell().ColumnSpan(3).Element(LightCell)
                                    .AlignCenter()
                                    .Text(consecutivo ?? "0");
                            });
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
