using BitacoraAlfipac.Data;
using BitacoraAlfipac.Models.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;

namespace BitacoraAlfipac.Controllers
{
    public class ActasPermanenciasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActasPermanenciasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // 📌 PENDIENTES (INDEX)
        // =========================
        public async Task<IActionResult> Index()
        {
            var pendientes = await _context.ActasPermanencias
                .Where(x => x.FechaHoraIngresoContenedor == null)
                .ToListAsync();

            return View(pendientes);
        }

        // =========================
        // 📌 HISTORIAL (INGRESADOS)
        // =========================
        public async Task<IActionResult> Historial()
        {
            var ingresados = await _context.ActasPermanencias
                .Where(x => x.FechaHoraIngresoContenedor != null)
                .ToListAsync();

            return View(ingresados);
        }

        // =========================
        // ➕ CREAR ACTA / PAB
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActasPermanencias model)
        {
            // 🚫 evitar contenedor repetido en el módulo
            var existe = await _context.ActasPermanencias
                .AnyAsync(x => x.Contenedor == model.Contenedor);

            if (existe)
            {
                TempData["Error"] = "Este contenedor ya existe en Actas/PAB.";
                return RedirectToAction(nameof(Index));
            }

            _context.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // 🔍 OBTENER DESCRIPCIÓN PAB POR CÓDIGO
        // =========================
        [HttpGet]
        public async Task<IActionResult> ObtenerDescripcionPab(int codigo)
        {
            var pab = await _context.PabMercanciasSusceptibles
                .Where(x => x.Codigo == codigo)
                .Select(x => new { x.Descripcion })
                .FirstOrDefaultAsync();

            if (pab == null)
                return Json(new { success = false });

            return Json(new { success = true, descripcion = pab.Descripcion });
        }

        // =========================
        // 🔍 BUSCAR CONTENEDOR
        // =========================
        public async Task<IActionResult> BuscarContenedor(string contenedor)
        {
            var item = await _context.ActasPermanencias
                .FirstOrDefaultAsync(x => x.Contenedor == contenedor);

            if (item == null)
                return Json(new { encontrado = false });

            return Json(new
            {
                encontrado = true,
                id = item.Id,
                tipo = item.Tipo,
                contenedor = item.Contenedor,
                numero = item.Numero,
                detalle = item.Detalle,
                viaje = item.Viaje,
                cliente = item.Cliente,
                fechaIngreso = item.FechaHoraIngresoContenedor?.ToString("yyyy-MM-ddTHH:mm"),
                aplicadoCorrectamente = item.AplicadoCorrectamente
            });
        }

        // =========================
        // ✏️ EDITAR DESDE CUALQUIER PANTALLA
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRegistro(ActasPermanencias model, DateTime? FechaIngreso)
        {
            var registro = await _context.ActasPermanencias.FindAsync(model.Id);
            if (registro == null) return RedirectToAction(nameof(Index));

            // ✏️ datos siempre editables
            registro.Numero = model.Numero;
            registro.Detalle = model.Detalle;
            registro.Viaje = model.Viaje;
            registro.Cliente = model.Cliente;

            // ✅ SOLO se guarda aplicado si existe ingreso
            registro.AplicadoCorrectamente = FechaIngreso.HasValue ? model.AplicadoCorrectamente : false;

            // 🔄 controla movimiento entre páginas
            registro.FechaHoraIngresoContenedor = FechaIngreso;

            await _context.SaveChangesAsync();

            // Redirección inteligente
            if (registro.FechaHoraIngresoContenedor == null)
                return RedirectToAction(nameof(Index));
            else
                return RedirectToAction(nameof(Historial));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarPendiente(int id)
        {
            var registro = await _context.ActasPermanencias.FindAsync(id);
            if (registro == null)
                return RedirectToAction(nameof(Index));

            // 🚫 No permitir borrar si ya ingresó
            if (registro.FechaHoraIngresoContenedor != null)
            {
                TempData["Error"] = "No se puede eliminar un registro que ya ingresó.";
                return RedirectToAction(nameof(Index));
            }

            _context.ActasPermanencias.Remove(registro);
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Registro eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuitarIngreso(int id)
        {
            var registro = await _context.ActasPermanencias.FindAsync(id);
            if (registro == null)
                return RedirectToAction(nameof(Historial));

            registro.FechaHoraIngresoContenedor = null;

            await _context.SaveChangesAsync();

            TempData["Ok"] = "El contenedor volvió a estado pendiente.";
            return RedirectToAction(nameof(Index)); // vuelve a pendientes
        }

        [HttpPost]
        public IActionResult ExportarPendientesPDF(string nombre)
        {
            var registros = _context.ActasPermanencias
                                .Where(x => x.FechaHoraIngresoContenedor == null)
                                .OrderBy(x => x.Contenedor)
                                .ToList();

            QuestPDF.Settings.License = LicenseType.Community;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);

                    // =========================
                    // HEADER
                    // =========================
                    page.Header().Column(col =>
                    {
                        col.Spacing(5);

                        col.Item().Text("ALFIPAC")
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);

                        col.Item().Text("ACTAS Y PAB PENDIENTES")
                            .FontSize(14)
                            .SemiBold();

                        col.Item().LineHorizontal(1)
                            .LineColor(Colors.Grey.Lighten2);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"👤 Usuario: {nombre}").FontSize(10);
                            row.RelativeItem().AlignRight().Text($"📅 {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);
                        });

                        col.Item().Text($"Total de registros: {registros.Count}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);

                        col.Item().LineHorizontal(1)
                            .LineColor(Colors.Grey.Lighten2);
                    });

                    // =========================
                    // TABLA
                    // =========================
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1); // Tipo
                            columns.RelativeColumn(2); // Contenedor
                            columns.RelativeColumn(1); // Número
                            columns.RelativeColumn(2); // Viaje
                            columns.RelativeColumn(2); // Cliente
                            columns.RelativeColumn(4); // Detalle (más espacio)
                        });

                        // ===== HEADER =====
                        void HeaderCell(string text) =>
                            table.Cell().Background(Colors.Blue.Lighten3)
                                .Border(1)
                                .BorderColor(Colors.White)
                                .Padding(6)
                                .AlignCenter()
                                .AlignMiddle()
                                .Text(text)
                                .Bold()
                                .FontSize(11);

                        HeaderCell("Tipo");
                        HeaderCell("Contenedor");
                        HeaderCell("Número");
                        HeaderCell("Viaje");
                        HeaderCell("Cliente");
                        HeaderCell("Detalle");

                        // ===== FILAS =====
                        int i = 0;

                        foreach (var r in registros)
                        {
                            var bg = i % 2 == 0
                                ? Colors.Grey.Lighten4
                                : Colors.White;

                            void Cell(string text) =>
                                table.Cell()
                                    .Background(bg)
                                    .BorderBottom(1)
                                    .BorderColor(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text(text)
                                    .FontSize(10);

                            Cell(r.Tipo ?? "");
                            Cell(r.Contenedor ?? "");
                            Cell(r.Numero ?? "");
                            Cell(r.Viaje ?? "-");
                            Cell(r.Cliente ?? "-");
                            Cell(r.Detalle ?? "");

                            i++;
                        }
                    });

                    // =========================
                    // FOOTER
                    // =========================
                    page.Footer()
                        .AlignCenter()
                        .Text(txt =>
                        {
                            txt.Span("Reporte generado automáticamente por SCL · ")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);

                            txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                                .SemiBold()
                                .FontSize(9);
                        });
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf",
                $"Actas_PAB_Pendientes_{DateTime.Now:dd-MM-yyyy_HH-mm}.pdf");
        }

        [HttpPost]
        public IActionResult ExportarHistorialPDF(string nombre)
        {
            var registros = _context.ActasPermanencias
                                .Where(x => x.FechaHoraIngresoContenedor != null)
                                .OrderBy(x => x.Contenedor)
                                .ToList();

            QuestPDF.Settings.License = LicenseType.Community;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);

                    // =========================
                    // HEADER
                    // =========================
                    page.Header().Column(col =>
                    {
                        col.Spacing(5);

                        col.Item().Text("ALFIPAC")
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);

                        col.Item().Text("HISTORIAL DE ACTAS Y PAB")
                            .FontSize(14)
                            .SemiBold();

                        col.Item().LineHorizontal(1)
                            .LineColor(Colors.Grey.Lighten2);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"👤 Usuario: {nombre}").FontSize(10);
                            row.RelativeItem().AlignRight().Text($"📅 {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);
                        });

                        col.Item().Text($"Total de registros: {registros.Count}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);

                        col.Item().LineHorizontal(1)
                            .LineColor(Colors.Grey.Lighten2);
                    });

                    // =========================
                    // TABLA
                    // =========================
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(60);   // Tipo
                            columns.ConstantColumn(110);  // Contenedor
                            columns.ConstantColumn(70);   // Número
                            columns.ConstantColumn(90);   // Viaje
                            columns.RelativeColumn(2);    // Cliente
                            columns.RelativeColumn(3);    // Detalle
                            columns.ConstantColumn(130);  // Fecha
                            columns.ConstantColumn(100);  // Resultado
                        });

                        // ===== HEADER =====
                        void HeaderCell(string text) =>
                            table.Cell()
                                .Background(Colors.Blue.Lighten3)
                                .Border(1)
                                .BorderColor(Colors.White)
                                .Padding(6)
                                .AlignCenter()
                                .AlignMiddle()
                                .Text(text)
                                .Bold()
                                .FontSize(11);

                        HeaderCell("Tipo");
                        HeaderCell("Contenedor");
                        HeaderCell("Número");
                        HeaderCell("Viaje");
                        HeaderCell("Cliente");
                        HeaderCell("Detalle");
                        HeaderCell("Fecha Ingreso");
                        HeaderCell("Resultado");

                        // ===== FILAS =====
                        int i = 0;

                        foreach (var r in registros)
                        {
                            var bg = i % 2 == 0
                                ? Colors.Grey.Lighten4
                                : Colors.White;

                            void Cell(string text) =>
                                table.Cell()
                                    .Background(bg)
                                    .BorderBottom(1)
                                    .BorderColor(Colors.Grey.Lighten2)
                                    .Padding(5)
                                    .Text(text)
                                    .FontSize(10);

                            Cell(r.Tipo ?? "");
                            Cell(r.Contenedor ?? "");
                            Cell(r.Numero ?? "");
                            Cell(r.Viaje ?? "-");
                            Cell(r.Cliente ?? "-");
                            Cell(r.Detalle ?? "");
                            Cell(r.FechaHoraIngresoContenedor?.ToString("dd/MM/yyyy HH:mm") ?? "-");

                            // =========================
                            // RESULTADO CON COLOR
                            // =========================
                            var resultadoTexto = r.AplicadoCorrectamente ? "✔ Correcta" : "✖ Incorrecta";
                            var resultadoColor = r.AplicadoCorrectamente
                                ? Colors.Green.Medium
                                : Colors.Red.Medium;

                            table.Cell()
                                .Background(bg)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(5)
                                .AlignCenter()
                                .Text(resultadoTexto)
                                .FontColor(resultadoColor)
                                .Bold()
                                .FontSize(10);

                            i++;
                        }
                    });

                    // =========================
                    // FOOTER
                    // =========================
                    page.Footer()
                        .AlignCenter()
                        .Text(txt =>
                        {
                            txt.Span("Reporte histórico generado por SCL · ")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);

                            txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                                .SemiBold()
                                .FontSize(9);
                        });
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf",
                $"Historial_Actas_PAB_{DateTime.Now:dd-MM-yyyy_HH-mm}.pdf");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LimpiarHistorialIngresados()
        {
            // Solo seleccionamos los registros que ya tienen fecha de ingreso
            var registros = _context.ActasPermanencias
                                    .Where(r => r.FechaHoraIngresoContenedor != null)
                                    .ToList();

            if (registros.Any())
            {
                _context.ActasPermanencias.RemoveRange(registros);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Se eliminaron todos los registros ingresados del historial.";
            }
            else
            {
                TempData["Mensaje"] = "No hay registros ingresados para eliminar.";
            }

            return RedirectToAction(nameof(Historial));
        }

        [HttpGet]
        public JsonResult BuscarContenedorEnPendientes(string contenedor)
        {
            if (string.IsNullOrEmpty(contenedor))
                return Json(new { encontrado = false });

            var registro = _context.ActasPermanencias
                .Where(a => a.Contenedor == contenedor && a.FechaHoraIngresoContenedor == null)
                .FirstOrDefault();

            if (registro == null)
                return Json(new { encontrado = false });

            return Json(new
            {
                encontrado = true,
                tipo = registro.Tipo,
                numero = registro.Numero
            });
        }

        // =========================
        // 🔍 OBTENER DATOS POR CONTENEDOR
        // =========================
        [HttpGet]
        public async Task<IActionResult> ObtenerDatosContenedor(string contenedor)
        {
            if (string.IsNullOrEmpty(contenedor))
                return Json(new { success = false });

            var datos = await _context.DatosIngresosViajes
                .Where(x => x.Contenedor == contenedor)
                .Select(x => new
                {
                    viaje = x.Viaje,
                    cliente = x.Declarante
                })
                .FirstOrDefaultAsync();

            if (datos == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                viaje = datos.viaje,
                cliente = datos.cliente
            });
        }

        public async Task<IActionResult> HistorialFiltro(DateTime? fechaFiltro = null)
        {
            var query = _context.ActasPermanencias
                                .Where(x => x.FechaHoraIngresoContenedor != null)
                                .AsQueryable();

            if (fechaFiltro.HasValue)
            {
                var fecha = fechaFiltro.Value.Date;
                query = query.Where(x => x.FechaHoraIngresoContenedor.Value.Date == fecha);
            }

            var registros = await query.OrderBy(x => x.Contenedor).ToListAsync();

            ViewBag.FechaFiltro = fechaFiltro?.ToString("yyyy-MM-dd");

            // Forzar que use la vista Historial.cshtml
            return View("Historial", registros);
        }

    }
}
