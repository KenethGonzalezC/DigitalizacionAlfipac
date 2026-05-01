using BitacoraAlfipac.Data;
using BitacoraAlfipac.Documents;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace BitacoraAlfipac.Controllers;

[Authorize]
public class HistorialContenedoresController : Controller
{
    private readonly ApplicationDbContext _context;

    private const int PageSize = 50;

    public HistorialContenedoresController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? contenedor, int page = 1)
    {
        if (page < 1)
            page = 1;

        var query = _context.HistorialContenedores.AsQueryable();

        // 🔍 Filtro por contenedor
        if (!string.IsNullOrWhiteSpace(contenedor))
        {
            query = query.Where(h => h.Contenedor.Contains(contenedor));
        }

        // 📦 Orden lógico: más reciente primero
        query = query.OrderByDescending(h => h.FechaHoraSalida);

        int totalRegistros = await query.CountAsync();
        int totalPaginas = (int)Math.Ceiling(totalRegistros / (double)PageSize);

        var historial = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        ViewBag.ContenedorBuscado = contenedor;
        ViewBag.PaginaActual = page;
        ViewBag.TotalPaginas = totalPaginas;
        ViewBag.TotalRegistros = totalRegistros;

        return View(historial);
    }

    public async Task<IActionResult> ExportarPdf(string? contenedor)
    {
        var query = _context.HistorialContenedores.AsQueryable();

        if (!string.IsNullOrWhiteSpace(contenedor))
            query = query.Where(h => h.Contenedor.Contains(contenedor));

        var historial = await query
            .OrderByDescending(h => h.FechaHoraIngreso)
            .ToListAsync();

        var doc = new HistorialContenedoresPdf(historial, contenedor);
        var pdf = doc.GeneratePdf();

        return File(pdf, "application/pdf", "HistorialContenedores.pdf");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LimpiarAntiguos(int meses = 6)
    {
        var fechaLimite = DateTime.Now.AddMonths(-meses);

        var viejos = await _context.HistorialContenedores
            .Where(h => h.FechaHoraSalida != null && h.FechaHoraSalida < fechaLimite)
            .ToListAsync();

        _context.HistorialContenedores.RemoveRange(viejos);

        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> ExportarExcel(string? contenedor)
    {
        var query = _context.HistorialContenedores.AsQueryable();

        if (!string.IsNullOrWhiteSpace(contenedor))
            query = query.Where(h => h.Contenedor.Contains(contenedor));

        var historial = await query
            .OrderByDescending(h => h.FechaHoraIngreso)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Historial");

        // 🧾 Encabezados
        ws.Cell(1, 1).Value = "Contenedor";
        ws.Cell(1, 2).Value = "Fecha Ingreso";
        ws.Cell(1, 3).Value = "Fecha Salida";

        ws.Range(1, 1, 1, 3).Style.Font.Bold = true;
        ws.Range(1, 1, 1, 3).Style.Fill.BackgroundColor = XLColor.LightGray;

        int fila = 2;

        foreach (var h in historial)
        {
            ws.Cell(fila, 1).Value = h.Contenedor;
            ws.Cell(fila, 2).Value = h.FechaHoraIngreso?.ToString("yyyy-MM-dd HH:mm") ?? "-";
            ws.Cell(fila, 3).Value = h.FechaHoraSalida?.ToString("yyyy-MM-dd HH:mm") ?? "-";
            fila++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "HistorialContenedores.xlsx"
        );
    }

}
