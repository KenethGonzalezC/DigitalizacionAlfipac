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
                    page.Margin(20);

                    // HEADER
                    page.Header().Column(col =>
                    {
                        col.Item().Text("ALFIPAC – ACTAS Y PAB PENDIENTES").Bold().FontSize(18);
                        col.Item().LineHorizontal(1);
                        col.Item().Text($"Impreso por: {nombre}");
                        col.Item().Text($"Fecha de impresión: {DateTime.Now:dd/MM/yyyy HH:mm}");
                        col.Item().LineHorizontal(1);
                    });

                    // TABLA
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1); // Tipo
                            columns.RelativeColumn(2); // Contenedor
                            columns.RelativeColumn(1); // Número
                            columns.RelativeColumn(2); // Viaje
                            columns.RelativeColumn(2); // Cliente
                            columns.RelativeColumn(3); // Detalle
                            //columns.RelativeColumn(1); // Estado
                        });

                        void HeaderCell(string text) =>
                            table.Cell().Element(CellStyle).Text(text).Bold().FontSize(12);

                        HeaderCell("Tipo");
                        HeaderCell("Contenedor");
                        HeaderCell("Número");
                        HeaderCell("Viaje");
                        HeaderCell("Cliente");
                        HeaderCell("Detalle");
                        //HeaderCell("Estado");

                        foreach (var r in registros)
                        {
                            table.Cell().Element(CellStyle).Text(r.Tipo);
                            table.Cell().Element(CellStyle).Text(r.Contenedor);
                            table.Cell().Element(CellStyle).Text(r.Numero);
                            table.Cell().Element(CellStyle).Text(r.Viaje);
                            table.Cell().Element(CellStyle).Text(r.Cliente);
                            table.Cell().Element(CellStyle).Text(r.Detalle);
                            //table.Cell().Element(CellStyle).Text("Pendiente");
                        }

                        static IContainer CellStyle(IContainer container)
                            => container.Border(1).Padding(3);
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
                    page.Margin(20);

                    // HEADER
                    page.Header().Column(col =>
                    {
                        col.Item().Text("ALFIPAC – HISTORIAL DE ACTAS Y PAB").Bold().FontSize(18);
                        col.Item().LineHorizontal(1);
                        col.Item().Text($"Impreso por: {nombre}");
                        col.Item().Text($"Fecha de impresión: {DateTime.Now:dd/MM/yyyy HH:mm}");
                        col.Item().LineHorizontal(1);
                    });

                    // TABLA
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);   // Tipo (3-4 letras, no necesita mucho espacio)
                            columns.ConstantColumn(100);  // Contenedor (4 letras + 7 números)
                            columns.ConstantColumn(60);   // Número (puede ser 1-2 dígitos, poco ancho)
                            columns.ConstantColumn(80);   // Viaje (hasta 9 números)
                            columns.RelativeColumn(2);    // Cliente (texto libre, espacio flexible)
                            columns.RelativeColumn(3);    // Detalle (texto libre, más espacio que cliente)
                            columns.ConstantColumn(120);  // Fecha Ingreso (dd/MM/yyyy HH:mm)
                            columns.ConstantColumn(80);   // Resultado (CORRECTO/INCORRECTO, no muy ancho)
                        });

                        void HeaderCell(string text) =>
                            table.Cell().Element(CellStyle).Text(text).Bold().FontSize(12);

                        HeaderCell("Tipo");
                        HeaderCell("Contenedor");
                        HeaderCell("Número");
                        HeaderCell("Viaje");
                        HeaderCell("Cliente");
                        HeaderCell("Detalle");
                        HeaderCell("Fecha Ingreso");
                        HeaderCell("Resultado");

                        foreach (var r in registros)
                        {
                            table.Cell().Element(CellStyle).Text(r.Tipo);
                            table.Cell().Element(CellStyle).Text(r.Contenedor);
                            table.Cell().Element(CellStyle).Text(r.Numero);
                            table.Cell().Element(CellStyle).Text(r.Viaje);
                            table.Cell().Element(CellStyle).Text(r.Cliente);
                            table.Cell().Element(CellStyle).Text(r.Detalle);
                            table.Cell().Element(CellStyle).Text(r.FechaHoraIngresoContenedor?.ToString("dd/MM/yyyy HH:mm") ?? "");
                            table.Cell().Element(CellStyle)
                                .Text(r.AplicadoCorrectamente ? "Correcta" : "Incorrecta");
                        }

                        static IContainer CellStyle(IContainer container)
                            => container.Border(1).Padding(3);
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

    }
}
