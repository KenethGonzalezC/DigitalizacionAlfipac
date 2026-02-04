using BitacoraAlfipac.Data;
using BitacoraAlfipac.Models.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class ActasPermanenciasController : Controller
{
    private readonly ApplicationDbContext _context;

    public ActasPermanenciasController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var lista = await _context.ActasPermanencias
            .OrderByDescending(a => a.FechaCreacion)
            .ToListAsync();

        return View(lista);
    }

    // ➕ CREAR ACTA / PAB
    [HttpPost]
    public async Task<IActionResult> Create(ActaPermanencia model)
    {
        model.Contenedor = model.Contenedor.ToUpper().Trim();
        model.FechaCreacion = DateTime.Now;

        _context.ActasPermanencias.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // ✅ MARCAR COMO APLICADA (desde popup)
    [HttpPost]
    public async Task<IActionResult> Aplicar(int id, DateTime fechaIngreso, bool correcta)
    {
        var acta = await _context.ActasPermanencias.FindAsync(id);
        if (acta == null) return NotFound();

        acta.Aplicada = true;
        acta.AplicadaCorrectamente = correcta;
        acta.FechaHoraIngresoContenedor = fechaIngreso;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // 📜 HISTORIAL COMPLETO
    public async Task<IActionResult> Historial()
    {
        var lista = await _context.ActasPermanencias
            .OrderByDescending(a => a.FechaCreacion)
            .ToListAsync();

        return View(lista);
    }
}
