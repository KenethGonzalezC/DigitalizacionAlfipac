using BitacoraAlfipac.Data;
using BitacoraAlfipac.Models.Entidades;
using BitacoraAlfipac.Models.ViewModels;
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

            var pendientes = _context.RegistroTransportistas
                .Where(x =>
                    x.Ubicacion == "Patio"
                    && x.FechaRegistro.Date == fechaFiltro
                    && x.FechaHoraIngreso == null)
                .OrderByDescending(x => x.FechaRegistro)
                .ToList();

            var ingresados = _context.RegistroTransportistas
                .Where(x =>
                    x.Ubicacion == "Patio"
                    && x.FechaRegistro.Date == fechaFiltro
                    && x.FechaHoraIngreso != null)
                .OrderByDescending(x => x.FechaHoraIngreso)
                .ToList();

            var vm = new PatioViewModel
            {
                Pendientes = pendientes,
                Ingresados = ingresados,
                FechaFiltro = fechaFiltro
            };

            return View(vm);
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

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            var registro = _context.RegistroTransportistas
                .FirstOrDefault(x => x.Id == id);

            if (registro == null)
            {
                TempData["Error"] = "Registro no encontrado.";
                return RedirectToAction(nameof(Patio));
            }

            // eliminar firma física si existe
            if (!string.IsNullOrWhiteSpace(registro.RutaFirma))
            {
                var rutaFisica = Path.Combine(
                    _environment.WebRootPath,
                    registro.RutaFirma.TrimStart('/')
                        .Replace("/", "\\"));

                if (System.IO.File.Exists(rutaFisica))
                {
                    System.IO.File.Delete(rutaFisica);
                }
            }

            _context.RegistroTransportistas.Remove(registro);
            _context.SaveChanges();

            TempData["Ok"] = "Registro eliminado correctamente.";

            return RedirectToAction(nameof(Patio));
        }

        //Detalle modal
        public IActionResult DetalleModal(int id)
        {
            var registro = _context.RegistroTransportistas
                .FirstOrDefault(x => x.Id == id);

            if (registro == null)
            {
                return Content("Registro no encontrado");
            }

            return PartialView("_DetalleRegistro", registro);
        }

        // =====================================================
        // EDITAR
        // =====================================================
        [HttpGet]
        public IActionResult Editar(int id)
        {
            var registro = _context.RegistroTransportistas
                .FirstOrDefault(x => x.Id == id);

            if (registro == null)
            {
                TempData["Error"] = "Registro no encontrado.";
                return RedirectToAction(nameof(Patio));
            }

            return View(registro);
        }

        // =====================================================
        // GUARDAR EDICIÓN
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(RegistroTransportista model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var registro = _context.RegistroTransportistas
                .FirstOrDefault(x => x.Id == model.Id);

            if (registro == null)
            {
                TempData["Error"] = "Registro no encontrado.";
                return RedirectToAction(nameof(Patio));
            }

            registro.FechaRegistro = model.FechaRegistro;
            registro.Placa = model.Placa?.Trim().ToUpper() ?? "";
            registro.NombreChofer = model.NombreChofer?.Trim().ToUpper() ?? "";
            registro.Cliente = model.Cliente?.Trim().ToUpper() ?? "";
            registro.DUA = model.DUA?.Trim().ToUpper();
            registro.Tipo = model.Tipo?.Trim();
            registro.Ubicacion = model.Ubicacion;

            _context.SaveChanges();

            TempData["Ok"] = "Registro actualizado correctamente.";

            return RedirectToAction(nameof(Patio));
        }

        //REGISTRAR INGRESO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarIngreso(
        int id,
        DateTime fechaHoraIngreso)
        {
            var registro = _context.RegistroTransportistas
                .FirstOrDefault(x => x.Id == id);

            if (registro == null)
            {
                TempData["Error"] = "Registro no encontrado.";
                return RedirectToAction(nameof(Patio));
            }

            registro.FechaHoraIngreso = fechaHoraIngreso;

            _context.SaveChanges();

            TempData["Ok"] = "Ingreso registrado correctamente.";

            return RedirectToAction(nameof(Patio));
        }

        //REVERTIR INGRESO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RevertirIngreso(int id)
        {
            var registro = _context.RegistroTransportistas
                .FirstOrDefault(x => x.Id == id);

            if (registro == null)
            {
                TempData["Error"] = "Registro no encontrado.";
                return RedirectToAction(nameof(Patio));
            }

            registro.FechaHoraIngreso = null;

            _context.SaveChanges();

            TempData["Ok"] = "Ingreso revertido correctamente.";

            return RedirectToAction(nameof(Patio));
        }
    }
}