using BitacoraAlfipac.Data;
using BitacoraAlfipac.Models.Entidades;
using Microsoft.AspNetCore.Mvc;

namespace BitacoraAlfipac.Controllers
{
    public class PabMercanciaSuceptibleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PabMercanciaSuceptibleController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var lista = _context.PabMercanciasSusceptibles.ToList();
            return View(lista);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PabMercanciaSusceptible model)
        {
            // Si el modelo no es válido, devolver la vista con el modelo para que se muestren errores.
            if (!ModelState.IsValid)
                return View(model);

            // Agregar y guardar el nuevo registro.
            _context.Add(model);
            _context.SaveChanges();

            // Tras guardar con éxito, redirigir a Index.
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Edit(int id)
        {
            var item = _context.PabMercanciasSusceptibles.Find(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(PabMercanciaSusceptible model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index));

            var item = _context.PabMercanciasSusceptibles.Find(model.Id);

            if (item == null)
                return NotFound();

            item.Descripcion = model.Descripcion;
            item.Codigo = model.Codigo;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var item = _context.PabMercanciasSusceptibles.Find(id);

            if (item != null)
            {
                _context.PabMercanciasSusceptibles.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
    }

}
