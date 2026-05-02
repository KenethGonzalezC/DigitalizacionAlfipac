using BitacoraAlfipac.Data;
using BitacoraAlfipac.Models.Entidades;
using BitacoraAlfipac.Models.ViewModels;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Linq;
using System.Text;

namespace BitacoraAlfipac.Controllers
{
    public class Patio1Controller : Controller
    {
        private readonly ApplicationDbContext _context;

        public Patio1Controller(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===============================
        // INDEX
        // ===============================
        public IActionResult Index(string contenedor)
        {
            var query = _context.Patio1.AsQueryable();

            if (!string.IsNullOrWhiteSpace(contenedor))
            {
                query = query.Where(c => c.Contenedor.Contains(contenedor));
            }

            var lista = query
                .OrderBy(c => c.Orden)
                .ThenBy(c => c.Contenedor)
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
            string transportista,
            string cliente)
        {
            var contenedor = _context.Patio1.FirstOrDefault(c => c.Id == id);

            if (contenedor == null)
                return NotFound();

            contenedor.Marchamos = marchamos;
            contenedor.EstadoCarga = estadoCarga;
            contenedor.Chasis = chasis;
            contenedor.Transportista = transportista;
            contenedor.Cliente = cliente;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // DEVOLVER A SIN ASIGNAR PATIO
        // ===============================
        [HttpPost]
        public IActionResult DevolverASinAsignar(int id)
        {
            var contenedor = _context.Patio1.FirstOrDefault(c => c.Id == id);

            if (contenedor == null)
                return NotFound();

            var nuevo = new ContenedorSinAsignarPatio
            {
                Contenedor = contenedor.Contenedor,
                Marchamos = contenedor.Marchamos,
                Tamano = contenedor.Tamano,
                Chasis = contenedor.Chasis,
                Transportista = contenedor.Transportista,
                Cliente = contenedor.Cliente,
                EstadoCarga = contenedor.EstadoCarga
            };

            _context.Patio1.Remove(contenedor);
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
            var contenedores = _context.Patio1.ToList();

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
                        Cliente = c.Cliente,
                        EstadoCarga = c.EstadoCarga
                    });
            }

            _context.Patio1.RemoveRange(contenedores);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult MoverDesdePatio1(int id, string destino)
        {
            var contenedor = _context.Patio1.FirstOrDefault(c => c.Id == id);

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
                        Cliente = contenedor.Cliente,
                        EstadoCarga = contenedor.EstadoCarga
                    });
                    break;

                case "Patio2":
                    _context.Patio2.Add(new Patio2
                    {
                        Contenedor = contenedor.Contenedor,
                        Marchamos = contenedor.Marchamos,
                        Tamano = contenedor.Tamano,
                        Chasis = contenedor.Chasis,
                        Transportista = contenedor.Transportista,
                        Cliente = contenedor.Cliente,
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
                        Cliente = contenedor.Cliente,
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
                        Cliente = contenedor.Cliente,
                        EstadoCarga = contenedor.EstadoCarga
                    });
                    break;
            }

            _context.Patio1.Remove(contenedor);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // EXPORTAR A PDF
        // ===============================
        [HttpPost]
        public IActionResult ExportarPDF(string nombre, DateTime fecha, string turno)
        {
            var datos = _context.Patio1.OrderBy(c => c.Orden).ToList();

            int total = datos.Count;
            int cargados = datos.Count(c => c.EstadoCarga == "Cargado");
            int vacios = datos.Count(c => c.EstadoCarga == "Vacio");

            QuestPDF.Settings.License = LicenseType.Community;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);

                    // ================= HEADER =================
                    page.Header().Column(col =>
                    {
                        col.Item().Text("ALFIPAC – INVENTARIO DE CONTENEDORES")
                            .Bold().FontSize(18);

                        col.Item().Text("Patio 1")
                            .FontSize(14);

                        col.Item().LineHorizontal(1);

                        col.Item().Text($"Impreso por: {nombre}");
                        col.Item().Text($"Fecha operativa: {fecha:dd/MM/yyyy}");
                        col.Item().Text($"Turno: {turno}");
                        col.Item().Text($"Fecha de impresión: {DateTime.Now:dd/MM/yyyy HH:mm}");

                        col.Item().LineHorizontal(1);
                    });

                    // ================= CONTENT =================
                    page.Content().Column(content =>
                    {
                        // 🔹 RESUMEN (solo primera página)
                        content.Item().Row(row =>
                        {
                            row.RelativeItem().Element(BoxStyle).Column(x =>
                            {
                                x.Item().Text("TOTAL").Bold();
                                x.Item().Text(total.ToString()).FontSize(16);
                            });

                            row.RelativeItem().Element(BoxStyle).Column(x =>
                            {
                                x.Item().Text("CARGADOS").Bold();
                                x.Item().Text(cargados.ToString()).FontSize(16);
                            });

                            row.RelativeItem().Element(BoxStyle).Column(x =>
                            {
                                x.Item().Text("VACÍOS").Bold();
                                x.Item().Text(vacios.ToString()).FontSize(16);
                            });
                        });

                        content.Item().PaddingVertical(10);

                        // 🔽 TABLA
                        content.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                            });

                            // ✅ HEADER REPETIDO EN TODAS LAS PÁGINAS
                            table.Header(header =>
                            {
                                void HeaderCellText(string text) =>
                                    header.Cell().Element(HeaderCell).Text(text).Bold();

                                HeaderCellText("Contenedor");
                                HeaderCellText("Marchamos");
                                HeaderCellText("Tamaño");
                                HeaderCellText("Chasis");
                                HeaderCellText("Transportista");
                                HeaderCellText("Cliente");
                                HeaderCellText("Estado");
                            });

                            foreach (var c in datos)
                            {
                                table.Cell().Element(Cell).ShowEntire().Text(c.Contenedor ?? "");
                                table.Cell().Element(Cell).ShowEntire().Text(c.Marchamos ?? "");
                                table.Cell().Element(Cell).ShowEntire().Text(c.Tamano ?? "");
                                table.Cell().Element(Cell).ShowEntire().Text(c.Chasis ?? "");
                                table.Cell().Element(Cell).ShowEntire().Text(c.Transportista ?? "");
                                table.Cell().Element(Cell).ShowEntire().Text(c.Cliente ?? "");
                                table.Cell().Element(Cell).ShowEntire().Text(c.EstadoCarga ?? "");
                            }
                        });
                    });

                    // ================= FOOTER ================= 
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf",
                $"Inventario_Patio1_{DateTime.Now:dd-MM-yyyy}_Turno_{turno}.pdf");


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

        // ===============================
        // EXPORTAR A EXCEL
        // ===============================
        [HttpPost]
        public IActionResult ExportarExcel(string nombre, DateTime fecha, string turno)
        {
            var datos = _context.Patio1.OrderBy(c => c.Orden).ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Inventario Patio 1");

            ws.Cell("A1").Value = "ALFIPAC";
            ws.Range("A1:F1").Merge().Style.Font.SetBold().Font.SetFontSize(18)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell("A2").Value = "SISTEMA DE CONTROL DE CONTENEDORES";
            ws.Range("A2:F2").Merge().Style.Font.SetFontSize(12)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell("A3").Value = "INVENTARIO OFICIAL – PATIO 1";
            ws.Range("A3:F3").Merge().Style.Font.SetBold()
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell("A5").Value = "Encargado:";
            ws.Cell("B5").Value = nombre;

            ws.Cell("D5").Value = "Fecha:";
            ws.Cell("E5").Value = fecha.ToString("dd/MM/yyyy");

            ws.Cell("A6").Value = "Turno:";
            ws.Cell("B6").Value = turno;

            int fila = 8;

            ws.Cell(fila, 1).Value = "Contenedor";
            ws.Cell(fila, 2).Value = "Marchamos";
            ws.Cell(fila, 3).Value = "Tamaño";
            ws.Cell(fila, 4).Value = "Chasis";
            ws.Cell(fila, 5).Value = "Transportista";
            ws.Cell(fila, 6).Value = "Cliente";
            ws.Cell(fila, 7).Value = "Estado";

            ws.Range(fila, 1, fila, 7).Style.Font.SetBold()
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
                ws.Cell(fila, 6).Value = c.Cliente;
                ws.Cell(fila, 7).Value = c.EstadoCarga;
                fila++;
            }

            ws.Columns().AdjustToContents();
            ws.Range(8, 1, fila - 1, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(8, 1, fila - 1, 6).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Inventario_Patio1_{DateTime.Now:dd-MM-yyyy}_Turno_{turno}.xlsx");
        }

        // ===============================
        // EXPORTAR A CSV
        // ===============================
        [HttpPost]
        public IActionResult ExportarCSV(string nombre, DateTime fecha, string turno)
        {
            var datos = _context.Patio1.OrderBy(c => c.Contenedor).ToList();

            int total = datos.Count;
            int cargados = datos.Count(c => c.EstadoCarga == "Cargado");
            int vacios = datos.Count(c => c.EstadoCarga == "Vacio");

            var sb = new StringBuilder();

            sb.AppendLine("ALFIPAC – INVENTARIO DE CONTENEDORES");
            sb.AppendLine("Patio 1");
            sb.AppendLine($"Impreso por: {nombre}");
            sb.AppendLine($"Fecha operativa: {fecha:dd/MM/yyyy}");
            sb.AppendLine($"Turno: {turno}");
            sb.AppendLine($"Fecha impresión: {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine("");
            sb.AppendLine($"TOTAL,{total}");
            sb.AppendLine($"CARGADOS,{cargados}");
            sb.AppendLine($"VACÍOS,{vacios}");
            sb.AppendLine("");
            sb.AppendLine("Contenedor,Marchamos,Tamaño,Chasis,Transportista, Cliente,Estado");

            foreach (var c in datos)
            {
                sb.AppendLine($"{c.Contenedor},{c.Marchamos},{c.Tamano},{c.Chasis},{c.Transportista}, {c.Cliente},{c.EstadoCarga}");
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()),
                "text/csv",
                $"Inventario_Patio1_{DateTime.Now:dd-MM-yyyy}_Turno_{turno}.csv");
        }

        [HttpPost]
        public IActionResult GuardarOrden([FromBody] List<OrdenVM> lista)
        {
            if (lista == null || !lista.Any())
                return BadRequest();

            foreach (var item in lista)
            {
                var cont = _context.Patio1.FirstOrDefault(x => x.Id == item.Id);

                if (cont == null)
                    continue;

                // 🔹 actualización de orden
                cont.Orden = item.Orden;

                // 🔹 actualización de edición (solo si vienen datos)
                if (!string.IsNullOrEmpty(item.Marchamos))
                    cont.Marchamos = item.Marchamos;

                if (!string.IsNullOrEmpty(item.EstadoCarga))
                    cont.EstadoCarga = item.EstadoCarga;

                if (!string.IsNullOrEmpty(item.Chasis))
                    cont.Chasis = item.Chasis;

                if (!string.IsNullOrEmpty(item.Transportista))
                    cont.Transportista = item.Transportista;

                if (!string.IsNullOrEmpty(item.Cliente))
                    cont.Cliente = item.Cliente;
            }

            _context.SaveChanges();

            return Json(new { success = true });
        }

        public class OrdenVM
        {
            public int Id { get; set; }
            public int Orden { get; set; }

            // campos de edición
            public string? Marchamos { get; set; }
            public string? EstadoCarga { get; set; }
            public string? Chasis { get; set; }
            public string? Transportista { get; set; }
            public string? Cliente { get; set; }
        }

    }

    }
