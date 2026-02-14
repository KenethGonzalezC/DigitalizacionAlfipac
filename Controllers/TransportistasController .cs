using BitacoraAlfipac.Data;
using BitacoraAlfipac.Models.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BitacoraAlfipac.Controllers
{
    public class TransportistasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransportistasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // 📋 INDEX
        // =========================
        public async Task<IActionResult> Index()
        {
            var lista = await _context.TransportistasAutorizados.ToListAsync();
            return View(lista);
        }

        // =========================
        // ➕ CREAR
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransportistaAutorizado model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index));

            _context.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // ✏️ EDITAR
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TransportistaAutorizado model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index));

            _context.Update(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // 🗑️ ELIMINAR
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TransportistasAutorizados.FindAsync(id);

            if (item != null)
            {
                _context.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
