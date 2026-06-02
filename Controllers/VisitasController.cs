using BitacoraAlfipac.Data;
using BitacoraAlfipac.Models.Entidades;
using BitacoraAlfipac.Models.ViewModels;
using BitacoraAlfipac.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BitacoraAlfipac.Controllers
{
    public class VisitasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OcrCedulaService _ocrService;

        public VisitasController(
            ApplicationDbContext context,
            OcrCedulaService ocrService)
        {
            _context = context;
            _ocrService = ocrService;
        }

        // =====================================================
        // INDEX
        // =====================================================

        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.Today;

            var visitas = await _context.Visitas
                .Where(x => x.FechaIngreso.Date == hoy)
                .OrderByDescending(x => x.FechaIngreso)
                .ToListAsync();

            return View(visitas);
        }

        // =====================================================
        // CREAR
        // =====================================================

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            VisitaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var visita = new Visita
            {
                FechaIngreso = DateTime.Now,

                NombreCompletoVisitante =
                    model.NombreCompletoVisitante,

                NumeroCedula =
                    model.NumeroCedula,

                PersonaVisita =
                    model.PersonaVisita,

                Departamento =
                    model.Departamento,

                UsuarioRegistro =
                    User.Identity?.Name,

                FechaRegistroSistema =
                    DateTime.Now
            };

            _context.Visitas.Add(visita);

            await _context.SaveChangesAsync();

            TempData["Ok"] =
                "Visita registrada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // FORMULARIO VACIO
        // =====================================================

        [HttpGet]
        public IActionResult Formulario()
        {
            return PartialView(
                "_FormularioVisita",
                new VisitaViewModel()
            );
        }

        // =====================================================
        // FORMULARIO CON DATOS OCR
        // =====================================================

        [HttpGet]
        public IActionResult FormularioEscaneado(
            string cedula,
            string nombre)
        {
            var model = new VisitaViewModel
            {
                NumeroCedula = cedula,
                NombreCompletoVisitante = nombre
            };

            return PartialView(
                "_FormularioVisita",
                model
            );
        }

        // =====================================================
        // OCR DE CEDULA
        // =====================================================

        [HttpPost]
        public IActionResult LeerCedula(
            [FromBody] CapturaCedulaViewModel model)
        {
            string? archivoTemporal = null;

            try
            {
                if (model == null ||
                    string.IsNullOrWhiteSpace(
                        model.ImagenBase64))
                {
                    return Json(new
                    {
                        success = false,
                        mensaje = "Imagen vacía"
                    });
                }

                // ==========================================
                // BASE64 -> BYTES
                // ==========================================

                string base64 =
                    model.ImagenBase64;

                if (base64.Contains(","))
                {
                    base64 =
                        base64.Split(',')[1];
                }

                byte[] imageBytes =
                    Convert.FromBase64String(
                        base64
                    );

                // ==========================================
                // TEMP
                // ==========================================

                string carpetaTemp =
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "temp"
                    );

                if (!Directory.Exists(carpetaTemp))
                {
                    Directory.CreateDirectory(
                        carpetaTemp
                    );
                }

                string nombreArchivo =
                    $"cedula_{DateTime.Now:yyyyMMdd_HHmmssfff}.jpg";

                archivoTemporal =
                    Path.Combine(
                        carpetaTemp,
                        nombreArchivo
                    );

                System.IO.File.WriteAllBytes(
                    archivoTemporal,
                    imageBytes
                );

                // ==========================================
                // OCR
                // ==========================================

                var resultado =
                    _ocrService.LeerCedula(
                        archivoTemporal
                    );

                if (!resultado.Success)
                {
                    return Json(new
                    {
                        success = false,
                        mensaje = resultado.Mensaje
                    });
                }

                return Json(new
                {
                    success = true,

                    mensaje =
                        resultado.Mensaje,

                    cedula =
                        resultado.Cedula,

                    nombre =
                        resultado.NombreCompleto,

                    texto =
                        resultado.TextoDetectado
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    mensaje = ex.Message
                });
            }
            finally
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(
                            archivoTemporal)
                        &&
                        System.IO.File.Exists(
                            archivoTemporal))
                    {
                        System.IO.File.Delete(
                            archivoTemporal
                        );
                    }
                }
                catch
                {
                    // Ignorar errores de limpieza
                }
            }
        }
    }
}