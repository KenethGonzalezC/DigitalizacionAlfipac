using BitacoraAlfipac.Data;
using BitacoraAlfipac.Models.Entidades;
using BitacoraAlfipac.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QIContainer = QuestPDF.Infrastructure.IContainer;
using ClosedXML.Excel;

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

            //historial de hoy
            ViewBag.HistorialHoy = _context.Vacios
            .Where(x => x.Fecha.Date == DateTime.Today)
            .OrderByDescending(x => x.Fecha)
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
            var registro = new Vacio
            {
                Fecha = DateTime.Now,
                Contenedor = contenedor,
                Cliente = cliente,
                Transportista = transportista,
                Consecutivo = consecutivo,
                Usuario = User.Identity?.Name ?? "Sistema"
            };

            //if (_context.Vacios.Any(x => x.Consecutivo == consecutivo))
            //{
            //    consecutivo = GenerarNuevoConsecutivo();
            //}

            _context.Vacios.Add(registro);
            _context.SaveChanges();

            var pdfBytes = GenerarReporteVacio(
                contenedor,
                cliente,
                transportista,
                consecutivo);

            return File(
                pdfBytes,
                "application/pdf",
                $"ReporteVacio_{contenedor}_{consecutivo}_{DateTime.Now:ddMMyyyy}.pdf"
            );
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

        //CONSECUTIVO AUTOINCREMENTAL
        [HttpGet]
        public JsonResult ObtenerSiguienteConsecutivo()
        {
            int maximo = 0;

            var consecutivos = _context.Vacios
                .Select(x => x.Consecutivo)
                .ToList();

            foreach (var item in consecutivos)
            {
                if (string.IsNullOrWhiteSpace(item))
                    continue;

                var texto = item.Trim().ToUpper();

                if (texto.StartsWith("CCV"))
                {
                    var numeroTexto = texto.Substring(3);

                    if (int.TryParse(numeroTexto, out int numero))
                    {
                        if (numero > maximo)
                            maximo = numero;
                    }
                }
            }

            var siguiente = $"CCV{(maximo + 1):D5}";

            return Json(new { consecutivo = siguiente });
        }

        //DELETE REGISTRO VACIO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarRegistroVacio(int id)
        {
            var item = _context.Vacios.Find(id);

            if (item != null)
            {
                _context.Vacios.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        //recuperar PDF
        [HttpGet]
        public IActionResult Reimprimir(int id)
        {
            var registro = _context.Vacios.FirstOrDefault(x => x.Id == id);

            if (registro == null)
                return NotFound();

            var pdfBytes = GenerarReporteVacio(
                registro.Contenedor,
                registro.Cliente,
                registro.Transportista,
                registro.Consecutivo
            );

            return File(
                pdfBytes,
                "application/pdf",
                $"ReporteVacio_{registro.Consecutivo}.pdf"
            );
        }

        //historial
        public IActionResult Historial(DateTime? fecha)
        {
            DateTime fechaFiltro = fecha?.Date ?? DateTime.Today;

            var historial = _context.Vacios
                .Where(x => x.Fecha.Date == fechaFiltro)
                .OrderByDescending(x => x.Fecha)
                .ToList();

            ViewBag.Fecha = fechaFiltro.ToString("yyyy-MM-dd");

            return View(historial);
        }

        //exportar historial a exel
        [HttpPost]
        public IActionResult ExportarHistorialExcel(
    DateTime fechaInicio,
    DateTime fechaFin,
    TimeSpan? horaInicio,
    TimeSpan? horaFin)
        {
            // =====================================================
            // FILTRO FECHA + HORA
            // =====================================================
            var desde = fechaInicio.Date;
            var hasta = fechaFin.Date.AddDays(1).AddTicks(-1);

            var datos = _context.Vacios
                .Where(x => x.Fecha >= desde && x.Fecha <= hasta)
                .ToList();

            if (horaInicio.HasValue)
                datos = datos.Where(x => x.Fecha.TimeOfDay >= horaInicio.Value).ToList();

            if (horaFin.HasValue)
                datos = datos.Where(x => x.Fecha.TimeOfDay <= horaFin.Value).ToList();

            datos = datos
                .OrderBy(x => x.Fecha)
                .ToList();

            // =====================================================
            // EXCEL
            // =====================================================
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Historial Vacios");

            // =====================================================
            // TÍTULO
            // =====================================================
            ws.Cell("A1").Value = "ALFIPAC";
            ws.Range("A1:F1").Merge();

            ws.Range("A1:F1").Style
                .Font.SetBold()
                .Font.SetFontSize(18)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Fill.SetBackgroundColor(XLColor.DarkBlue)
                .Font.SetFontColor(XLColor.White);

            ws.Cell("A2").Value = "HISTORIAL DE REPORTES VACÍOS";
            ws.Range("A2:F2").Merge();

            ws.Range("A2:F2").Style
                .Font.SetBold()
                .Font.SetFontSize(14)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell("A3").Value = "Desde:";
            ws.Cell("B3").Value = fechaInicio.ToString("dd/MM/yyyy");

            ws.Cell("C3").Value = "Hasta:";
            ws.Cell("D3").Value = fechaFin.ToString("dd/MM/yyyy");

            ws.Cell("E3").Value = "Hora:";
            ws.Cell("F3").Value =
                $"{horaInicio:hh\\:mm} - {horaFin:hh\\:mm}";

            ws.Range("A3:F3").Style.Font.SetBold();

            // =====================================================
            // ENCABEZADOS (como imagen)
            // =====================================================
            int fila = 5;

            ws.Cell(fila, 1).Value = "Fecha";
            ws.Cell(fila, 2).Value = "Hora";
            ws.Cell(fila, 3).Value = "Contenedor";
            ws.Cell(fila, 4).Value = "Cliente";
            ws.Cell(fila, 5).Value = "Transportista";
            ws.Cell(fila, 6).Value = "Consecutivo";

            var header = ws.Range(fila, 1, fila, 6);

            header.Style
                .Font.SetBold()
                .Font.SetFontColor(XLColor.Black)
                .Fill.SetBackgroundColor(XLColor.Lime)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin);

            fila++;

            // =====================================================
            // DETALLE
            // =====================================================
            foreach (var item in datos)
            {
                ws.Cell(fila, 1).Value = item.Fecha.ToString("d/M/yyyy");
                ws.Cell(fila, 2).Value = item.Fecha.ToString("HH:mm");
                ws.Cell(fila, 3).Value = item.Contenedor;
                ws.Cell(fila, 4).Value = item.Cliente;
                ws.Cell(fila, 5).Value = item.Transportista;
                ws.Cell(fila, 6).Value = item.Consecutivo;

                fila++;
            }

            // =====================================================
            // FORMATO CUERPO
            // =====================================================
            var body = ws.Range(6, 1, fila - 1, 6);

            body.Style
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin);

            // Zebra rows (similar visual)
            for (int i = 6; i < fila; i++)
            {
                if (i % 2 == 0)
                    ws.Range(i, 1, i, 6)
                      .Style.Fill.SetBackgroundColor(XLColor.White);
                else
                    ws.Range(i, 1, i, 6)
                      .Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F2F2F2"));
            }

            // =====================================================
            // ANCHOS PARECIDOS A IMAGEN
            // =====================================================
            ws.Column(1).Width = 12;
            ws.Column(2).Width = 10;
            ws.Column(3).Width = 18;
            ws.Column(4).Width = 55;
            ws.Column(5).Width = 22;
            ws.Column(6).Width = 15;

            // Freeze header
            ws.SheetView.FreezeRows(5);

            // Filtro automático
            ws.Range(5, 1, fila - 1, 6).SetAutoFilter();

            // =====================================================
            // DESCARGA
            // =====================================================
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Historial_Vacios_{DateTime.Now:ddMMyyyy_HHmm}.xlsx");
        }

        //editar historial
        [HttpPost]
        public IActionResult EditarHistorial(Vacio model)
        {
            var item = _context.Vacios.Find(model.Id);

            if (item == null)
                return RedirectToAction("Historial");

            item.Contenedor = model.Contenedor;
            item.Cliente = model.Cliente;
            item.Transportista = model.Transportista;
            item.Consecutivo = model.Consecutivo;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
