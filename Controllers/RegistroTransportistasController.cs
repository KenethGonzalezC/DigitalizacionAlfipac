using BitacoraAlfipac.Data;
using BitacoraAlfipac.Models.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BitacoraAlfipac.Controllers
{
    public class RegistroTransportistasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public RegistroTransportistasController(
            ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // =====================================================
        // INDEX
        // =====================================================
        public IActionResult Index()
        {
            return View();
        }

        // =====================================================
        // PATIO
        // =====================================================
        public IActionResult Patio(DateTime? fecha)
        {
            DateTime fechaFiltro = fecha?.Date ?? DateTime.Today;

            var registros = _context.RegistroTransportistas
                .Where(x =>
                    x.Ubicacion == "Patio" &&
                    x.FechaRegistro.Date == fechaFiltro)
                .OrderByDescending(x => x.FechaRegistro)
                .ToList();

            ViewBag.Fecha = fechaFiltro.ToString("yyyy-MM-dd");

            return View(registros);
        }

        // =====================================================
        // QUIMICOS
        // =====================================================
        public IActionResult Quimicos()
        {
            var registros = _context.RegistroTransportistas
                .Where(x => x.Ubicacion == "Agroquimicos")
                .OrderByDescending(x => x.FechaRegistro)
                .ToList();

            return View(registros);
        }

        // =====================================================
        // BODEGA 2000
        // =====================================================
        public IActionResult Bodega2000()
        {
            var registros = _context.RegistroTransportistas
                .Where(x => x.Ubicacion == "Bodega2000")
                .OrderByDescending(x => x.FechaRegistro)
                .ToList();

            return View(registros);
        }

        // =====================================================
        // CREAR PARA PATIO
        // =====================================================
        public IActionResult CrearPatio()
        {
            var model = new RegistroTransportista
            {
                FechaRegistro = DateTime.Now,
                Ubicacion = "Patio"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearPatio(
        RegistroTransportista model,
        string? FirmaBase64)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FirmaBase64))
                {
                    ModelState.AddModelError("", "Debe firmar antes de guardar.");
                    return View(model);
                }

                // Guardar firma
                string rutaFirma = await GuardarFirma(FirmaBase64);

                model.RutaFirma = rutaFirma;
                model.FechaRegistro = DateTime.Now;
                model.Ubicacion = "Patio";

                model.UsuarioRegistro = User.Identity?.Name;

                _context.RegistroTransportistas.Add(model);
                await _context.SaveChangesAsync();

                TempData["Ok"] = "Registro guardado correctamente.";

                return RedirectToAction(nameof(Patio));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
        }

        private async Task<string> GuardarFirma(string base64)
        {
            var carpeta = Path.Combine(
                _environment.WebRootPath,
                "firmas",
                "transportistas");

            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            var nombreArchivo =
                $"firma_{Guid.NewGuid():N}.png";

            var rutaCompleta =
                Path.Combine(carpeta, nombreArchivo);

            string imagenBase64 =
                base64.Substring(base64.IndexOf(",") + 1);

            byte[] bytes =
                Convert.FromBase64String(imagenBase64);

            await System.IO.File.WriteAllBytesAsync(
                rutaCompleta,
                bytes);

            return $"/firmas/transportistas/{nombreArchivo}";
        }

        // =====================================================
        // GUARDAR
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            RegistroTransportista model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.FechaRegistro = DateTime.Now;

            model.UsuarioRegistro =
                User?.Identity?.Name ?? "Sistema";

            _context.RegistroTransportistas.Add(model);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Detalle),
                new { id = model.Id });
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
    }
}