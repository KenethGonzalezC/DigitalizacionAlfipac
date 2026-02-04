using BitacoraAlfipac.Data;
using BitacoraAlfipac.Models.Entidades;
using BitacoraAlfipac.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Linq;
using QuestPDF.Fluent;
using ClosedXML.Excel;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;

namespace BitacoraAlfipac.Controllers
{
    public class Patio2Controller : Controller
    {
        private readonly ApplicationDbContext _context;

        public Patio2Controller(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===============================
        // INDEX
        // ===============================
        public IActionResult Index(string contenedor)
        {
            var query = _context.Patio2.AsQueryable();

            if (!string.IsNullOrWhiteSpace(contenedor))
            {
                query = query.Where(c =>
                    c.Contenedor.Contains(contenedor));
            }

            var lista = query
                .OrderBy(c => c.Contenedor)
                .ToList();

            return View(lista);
        }

        // ===============================
        // CONFIRMAR EDICIÓN (NO MUEVE)
        // ===============================
        [HttpPost]
        public IActionResult ConfirmarEdicion(
            int id,
            string marchamos,
            string estadoCarga,
            string chasis,
            string transportista)
        {
            var contenedor = _context.Patio2.FirstOrDefault(c => c.Id == id);

            if (contenedor == null)
                return NotFound();

            contenedor.Marchamos = marchamos;
            contenedor.EstadoCarga = estadoCarga;
            contenedor.Chasis = chasis;
            contenedor.Transportista = transportista;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // DEVOLVER A SIN ASIGNAR PATIO
        // ===============================
        [HttpPost]
        public IActionResult DevolverASinAsignar(int id)
        {
            var contenedor = _context.Patio2.FirstOrDefault(c => c.Id == id);

            if (contenedor == null)
                return NotFound();

            var nuevo = new ContenedorSinAsignarPatio
            {
                Contenedor = contenedor.Contenedor,
                Marchamos = contenedor.Marchamos,
                Tamano = contenedor.Tamano,
                Chasis = contenedor.Chasis,
                Transportista = contenedor.Transportista,
                EstadoCarga = contenedor.EstadoCarga
            };

            _context.Patio2.Remove(contenedor);
            _context.ContenedoresSinAsignarPatio.Add(nuevo);

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // LIMPIAR TODO EL PATIO
        // ===============================
        [HttpPost]
        public IActionResult LimpiarPatio()
        {
            var contenedores = _context.Patio2.ToList();

            foreach (var c in contenedores)
            {
                _context.ContenedoresSinAsignarPatio.Add(
                    new ContenedorSinAsignarPatio
                    {
                        Contenedor = c.Contenedor,
                        Marchamos = c.Marchamos,
                        Tamano = c.Tamano,
                        Chasis = c.Chasis,
                        Transportista = c.Transportista,
                        EstadoCarga = c.EstadoCarga
                    });
            }

            _context.Patio2.RemoveRange(contenedores);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult MoverDesdePatio2(int id, string destino)
        {
            var contenedor = _context.Patio2.FirstOrDefault(c => c.Id == id);

            if (contenedor == null)
                return NotFound();

            // 🛑 SI NO ELIGIÓ DESTINO → NO MOVER
            if (string.IsNullOrWhiteSpace(destino))
                return RedirectToAction(nameof(Index));

            switch (destino)
            {
                case "SinAsignar":
                    _context.ContenedoresSinAsignarPatio.Add(new ContenedorSinAsignarPatio
                    {
                        Contenedor = contenedor.Contenedor,
                        Marchamos = contenedor.Marchamos,
                        Tamano = contenedor.Tamano,
                        Chasis = contenedor.Chasis,
                        Transportista = contenedor.Transportista,
                        EstadoCarga = contenedor.EstadoCarga
                    });
                    break;

                case "Patio1":
                    _context.Patio1.Add(new Patio1
                    {
                        Contenedor = contenedor.Contenedor,
                        Marchamos = contenedor.Marchamos,
                        Tamano = contenedor.Tamano,
                        Chasis = contenedor.Chasis,
                        Transportista = contenedor.Transportista,
                        EstadoCarga = contenedor.EstadoCarga
                    });
                    break;

                case "Anden2000":
                    _context.Anden2000.Add(new Anden2000
                    {
                        Contenedor = contenedor.Contenedor,
                        Marchamos = contenedor.Marchamos,
                        Tamano = contenedor.Tamano,
                        Chasis = contenedor.Chasis,
                        Transportista = contenedor.Transportista,
                        EstadoCarga = contenedor.EstadoCarga
                    });
                    break;

                case "PatioQuimicos":
                    _context.PatioQuimicos.Add(new PatioQuimicos
                    {
                        Contenedor = contenedor.Contenedor,
                        Marchamos = contenedor.Marchamos,
                        Tamano = contenedor.Tamano,
                        Chasis = contenedor.Chasis,
                        Transportista = contenedor.Transportista,
                        EstadoCarga = contenedor.EstadoCarga
                    });
                    break;
            }

            _context.Patio2.Remove(contenedor);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // EXPORTAR DESDE PDF
        // ===============================
        [HttpPost]
        public IActionResult ExportarPDF(string nombre, DateTime fecha, string turno)
        {
            var datos = _context.Patio2.OrderBy(c => c.Contenedor).ToList();

            int total = datos.Count;
            int cargados = datos.Count(c => c.EstadoCarga == "Cargado");
            int vacios = datos.Count(c => c.EstadoCarga == "Vacio");

            QuestPDF.Settings.License = LicenseType.Community;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    // 🧾 HEADER
                    page.Header().Column(col =>
                    {
                        col.Item().Text("ALFIPAC – INVENTARIO DE CONTENEDORES").Bold().FontSize(18);
                        col.Item().Text("Patio 2").FontSize(14);

                        col.Item().LineHorizontal(1);

                        col.Item().Text($"Impreso por: {nombre}");
                        col.Item().Text($"Fecha operativa: {fecha:dd/MM/yyyy}");
                        col.Item().Text($"Turno: {turno}");
                        col.Item().Text($"Fecha de impresión: {DateTime.Now:dd/MM/yyyy HH:mm}");

                        col.Item().LineHorizontal(1);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Border(1).Padding(5).Column(x =>
                            {
                                x.Item().Text("TOTAL").Bold();
                                x.Item().Text(total.ToString()).FontSize(16);
                            });

                            row.RelativeItem().Border(1).Padding(5).Column(x =>
                            {
                                x.Item().Text("CARGADOS").Bold();
                                x.Item().Text(cargados.ToString()).FontSize(16);
                            });

                            row.RelativeItem().Border(1).Padding(5).Column(x =>
                            {
                                x.Item().Text("VACÍOS").Bold();
                                x.Item().Text(vacios.ToString()).FontSize(16);
                            });
                        });
                    });

                    // 📋 TABLA
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                        });

                        void HeaderCell(string text) =>
                            table.Cell().Element(CellStyle).Text(text).Bold();

                        HeaderCell("Contenedor");
                        HeaderCell("Marchamos");
                        HeaderCell("Tamaño");
                        HeaderCell("Chasis");
                        HeaderCell("Transportista");
                        HeaderCell("Estado");

                        foreach (var c in datos)
                        {
                            table.Cell().Element(CellStyle).Text(c.Contenedor);
                            table.Cell().Element(CellStyle).Text(c.Marchamos);
                            table.Cell().Element(CellStyle).Text(c.Tamano);
                            table.Cell().Element(CellStyle).Text(c.Chasis);
                            table.Cell().Element(CellStyle).Text(c.Transportista);
                            table.Cell().Element(CellStyle).Text(c.EstadoCarga);
                        }

                        static IContainer CellStyle(IContainer container)
                            => container.Border(1).Padding(3);
                    });
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", $"Inventario_Patio2 {DateTime.Now:dd/MM/yyyy} Turno: {turno}.pdf");
        }

        // ===============================
        // EXPORTAR DESDE EXCEL
        // ===============================
        [HttpPost]
        public IActionResult ExportarExcel(string nombre, DateTime fecha, string turno)
        {
            var datos = _context.Patio2.OrderBy(c => c.Contenedor).ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Inventario Patio 2");

            // ===============================
            // ENCABEZADO INSTITUCIONAL
            // ===============================
            ws.Cell("A1").Value = "ALFIPAC";
            ws.Range("A1:F1").Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(18)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell("A2").Value = "SISTEMA DE CONTROL DE CONTENEDORES";
            ws.Range("A2:F2").Merge().Style
                .Font.SetFontSize(12)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell("A3").Value = "INVENTARIO OFICIAL – PATIO 2";
            ws.Range("A3:F3").Merge().Style
                .Font.SetBold()
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // ===============================
            // DATOS DE IMPRESIÓN
            // ===============================
            ws.Cell("A5").Value = "Encargado:";
            ws.Cell("B5").Value = nombre;

            ws.Cell("D5").Value = "Fecha:";
            ws.Cell("E5").Value = fecha.ToString("dd/MM/yyyy");

            ws.Cell("A6").Value = "Turno:";
            ws.Cell("B6").Value = turno;

            // ===============================
            // TABLA
            // ===============================
            int fila = 8;

            ws.Cell(fila, 1).Value = "Contenedor";
            ws.Cell(fila, 2).Value = "Marchamos";
            ws.Cell(fila, 3).Value = "Tamaño";
            ws.Cell(fila, 4).Value = "Chasis";
            ws.Cell(fila, 5).Value = "Transportista";
            ws.Cell(fila, 6).Value = "Estado";

            ws.Range(fila, 1, fila, 6).Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.LightGray)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            fila++;

            foreach (var c in datos)
            {
                ws.Cell(fila, 1).Value = c.Contenedor;
                ws.Cell(fila, 2).Value = c.Marchamos;
                ws.Cell(fila, 3).Value = c.Tamano;
                ws.Cell(fila, 4).Value = c.Chasis;
                ws.Cell(fila, 5).Value = c.Transportista;
                ws.Cell(fila, 6).Value = c.EstadoCarga;
                fila++;
            }

            ws.Columns().AdjustToContents();

            // Bordes de tabla
            ws.Range(8, 1, fila - 1, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(8, 1, fila - 1, 6).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Inventario_Patio2 {DateTime.Now:dd/MM/yyyy} Turno: {turno}.xlsx");
        }

        // ===============================
        // EXPORTAR DESDE CSV
        // ===============================
        [HttpPost]
        public IActionResult ExportarCSV(string nombre, DateTime fecha, string turno)
        {
            var datos = _context.Patio2.OrderBy(c => c.Contenedor).ToList();

            int total = datos.Count;
            int cargados = datos.Count(c => c.EstadoCarga == "Cargado");
            int vacios = datos.Count(c => c.EstadoCarga == "Vacio");

            var sb = new StringBuilder();

            sb.AppendLine("ALFIPAC – INVENTARIO DE CONTENEDORES");
            sb.AppendLine("Patio 2");
            sb.AppendLine($"Impreso por: {nombre}");
            sb.AppendLine($"Fecha operativa: {fecha:dd/MM/yyyy}");
            sb.AppendLine($"Turno: {turno}");
            sb.AppendLine($"Fecha impresión: {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine("");
            sb.AppendLine($"TOTAL,{total}");
            sb.AppendLine($"CARGADOS,{cargados}");
            sb.AppendLine($"VACÍOS,{vacios}");
            sb.AppendLine("");
            sb.AppendLine("Contenedor,Marchamos,Tamaño,Chasis,Transportista,Estado");

            foreach (var c in datos)
            {
                sb.AppendLine($"{c.Contenedor},{c.Marchamos},{c.Tamano},{c.Chasis},{c.Transportista},{c.EstadoCarga}");
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()),
                "text/csv",
                $"Inventario_Patio2 {DateTime.Now:dd/MM/yyyy} Turno: {turno}.csv");
        }
    }
}
