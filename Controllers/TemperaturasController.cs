using BitacoraAlfipac.Data;
using BitacoraAlfipac.Documents;
using BitacoraAlfipac.Models.Entidades;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System.Text;


[Authorize]
public class TemperaturasController : Controller
{
    private readonly ApplicationDbContext _context;

    public TemperaturasController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var contenedores = await _context.ContenedoresRefrigerados
            .OrderByDescending(c => c.FechaHoraIngreso)
            .ToListAsync();

        var activos = contenedores
            .Where(c => c.FechaHoraDespacho == null)
            .ToList();

        var despachados = contenedores
            .Where(c => c.FechaHoraDespacho != null)
            .ToList();

        ViewBag.Activos = activos;
        ViewBag.Despachados = despachados;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string contenedor)
    {
        if (string.IsNullOrWhiteSpace(contenedor))
        {
            TempData["Error"] = "Debe ingresar un número de contenedor";
            return RedirectToAction(nameof(Index));
        }

        var nuevo = new ContenedorRefrigerado
        {
            Contenedor = contenedor.ToUpper(),
            FechaHoraIngreso = DateTime.Now
        };

        _context.ContenedoresRefrigerados.Add(nuevo);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Contenedor refrigerado creado correctamente";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var contenedor = await _context.ContenedoresRefrigerados
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contenedor != null)
        {
            _context.ContenedoresRefrigerados.Remove(contenedor);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var contenedor = await _context.ContenedoresRefrigerados
            .Include(c => c.RegistrosTemperatura)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contenedor == null)
            return NotFound();

        return View(contenedor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActualizarEncabezado(
    int id,
    string contenedor,
    DateTime? fechaHoraIngreso,
    DateTime? fechaHoraConexion,
    decimal? setPoint,
    DateTime? fechaHoraDespacho,
    DateTime? fechaHoraDesconexion)
    {
        var entidad = await _context.ContenedoresRefrigerados
            .FindAsync(id);

        if (entidad == null)
            return NotFound();

        entidad.Contenedor = contenedor;
        entidad.FechaHoraIngreso = fechaHoraIngreso;
        entidad.FechaHoraConexion = fechaHoraConexion;
        entidad.SetPoint = setPoint;
        entidad.FechaHoraDespacho = fechaHoraDespacho;
        entidad.FechaHoraDesconexion = fechaHoraDesconexion;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarRegistro(
    int contenedorId,
    DateTime fechaHora,
    decimal temperatura,
    string? observacion)
    {
        var contenedor = await _context.ContenedoresRefrigerados
            .FindAsync(contenedorId);

        if (contenedor == null)
            return NotFound();

        double setPoint = (double)contenedor.SetPoint;
        double temp = (double)temperatura;

        double diferencia = Math.Abs(temp - setPoint);

        string estado;
        string mensaje;

        if (diferencia <= 4)
        {
            estado = "Normal";
            mensaje = "Temperatura dentro de tolerancia.";
        }
        else if (diferencia <= 10)
        {
            estado = "Anormal";
            mensaje = "ANORMAL: Temperatura fuera de rango permitido.";
        }
        else
        {
            estado = "Emergencia";
            mensaje = "EMERGENCIA: Temperatura crítica.";
        }

        var registro = new RegistroTemperatura
        {
            ContenedorRefrigeradoId = contenedorId,
            FechaHora = fechaHora,
            Temperatura = temperatura,

            // Si el usuario no escribe observación → se autogenera
            Observacion = string.IsNullOrWhiteSpace(observacion)
                ? $"{mensaje} (SetPoint: {setPoint}°C | Dif: {diferencia:F1}°C)"
                : observacion,

        };

        _context.RegistrosTemperatura.Add(registro);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Detalle), new { id = contenedorId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarRegistro(int id, int contenedorId)
    {
        var registro = await _context.RegistrosTemperatura
            .FirstOrDefaultAsync(r => r.Id == id);

        if (registro != null)
        {
            _context.RegistrosTemperatura.Remove(registro);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Detalle), new { id = contenedorId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarRegistro(
    int id,
    DateTime fechaHora,
    decimal temperatura,
    string? observacion)
    {
        var registro = await _context.RegistrosTemperatura
            .FindAsync(id);

        if (registro == null)
            return NotFound();

        var contenedor = await _context.ContenedoresRefrigerados
            .FindAsync(registro.ContenedorRefrigeradoId);

        if (contenedor == null)
            return NotFound();

        // 🔥 MISMA LÓGICA QUE CREAR
        double setPoint = (double)contenedor.SetPoint;
        double temp = (double)temperatura;

        double diferencia = Math.Abs(temp - setPoint);

        string estado;
        string mensaje;

        if (diferencia <= 4)
        {
            estado = "Normal";
            mensaje = "Temperatura dentro de tolerancia.";
        }
        else if (diferencia <= 10)
        {
            estado = "Anormal";
            mensaje = "ANORMAL: Temperatura fuera de rango permitido.";
        }
        else
        {
            estado = "Emergencia";
            mensaje = "EMERGENCIA: Temperatura crítica.";
        }

        // 🔄 ACTUALIZAR DATOS
        registro.FechaHora = fechaHora;
        registro.Temperatura = temperatura;

        registro.Observacion = string.IsNullOrWhiteSpace(observacion)
            ? $"{mensaje} (SetPoint: {setPoint}°C | Dif: {diferencia:F1}°C)"
            : observacion;

        await _context.SaveChangesAsync();

        return RedirectToAction("Detalle", new { id = registro.ContenedorRefrigeradoId });
    }

    public async Task<IActionResult> ExportarExcel(int id)
    {
        var c = await _context.ContenedoresRefrigerados
            .Include(x => x.RegistrosTemperatura)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (c == null)
            return NotFound();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Temperaturas");

        int row = 1;

        // ===== TÍTULO =====
        ws.Cell(row, 1).Value = "CONTENEDOR REFRIGERADO";
        ws.Range(row, 1, row, 2).Merge();
        ws.Range(row, 1, row, 2).Style.Font.Bold = true;
        ws.Range(row, 1, row, 2).Style.Font.FontSize = 14;
        row += 2;

        // ===== FUNCIÓN ÚNICA =====
        void Info(string label, string value)
        {
            ws.Cell(row, 1).Value = label;
            ws.Cell(row, 2).Value = string.IsNullOrWhiteSpace(value) ? "-" : value;
            ws.Cell(row, 1).Style.Font.Bold = true;
            row++;
        }

        // ===== ENCABEZADO =====
        Info("Contenedor", c.Contenedor);

        Info("Fecha / Hora Ingreso",
            c.FechaHoraIngreso?.ToString("dd/MM/yyyy HH:mm") ?? "-");

        Info("Fecha / Hora Conexión",
            c.FechaHoraConexion?.ToString("dd/MM/yyyy HH:mm") ?? "-");

        Info("Set Point (°C)",
            c.SetPoint.HasValue ? c.SetPoint.Value.ToString("0.##") : "-");

        Info("Fecha / Hora Despacho",
            c.FechaHoraDespacho?.ToString("dd/MM/yyyy HH:mm") ?? "-");

        Info("Fecha / Hora Desconexión",
            c.FechaHoraDesconexion?.ToString("dd/MM/yyyy HH:mm") ?? "-");

        row += 2;

        // ===== TABLA DE TEMPERATURAS =====
        ws.Cell(row, 1).Value = "Fecha / Hora";
        ws.Cell(row, 2).Value = "Temperatura (°C)";
        ws.Cell(row, 3).Value = "Observación";
        ws.Range(row, 1, row, 3).Style.Font.Bold = true;
        row++;

        foreach (var r in c.RegistrosTemperatura.OrderBy(x => x.FechaHora))
        {
            ws.Cell(row, 1).Value = r.FechaHora.ToString("dd/MM/yyyy HH:mm");
            ws.Cell(row, 2).Value = r.Temperatura;
            ws.Cell(row, 3).Value = r.Observacion ?? "";
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        stream.Position = 0;

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Temperaturas_{c.Contenedor}.xlsx"
        );
    }

    public async Task<IActionResult> ExportarCsv(int id)
    {
        var c = await _context.ContenedoresRefrigerados
            .Include(x => x.RegistrosTemperatura)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (c == null) return NotFound();

        var sb = new StringBuilder();

        sb.AppendLine("BITÁCORA DE TEMPERATURAS");
        sb.AppendLine($"Contenedor,{c.Contenedor}");
        sb.AppendLine($"Ingreso,{c.FechaHoraIngreso}");
        sb.AppendLine($"Conexión,{c.FechaHoraConexion}");
        sb.AppendLine($"Set Point,{c.SetPoint}");
        sb.AppendLine($"Despacho,{c.FechaHoraDespacho}");
        sb.AppendLine($"Desconexión,{c.FechaHoraDesconexion}");
        sb.AppendLine();

        sb.AppendLine("FechaHora,Temperatura,Observacion");

        foreach (var r in c.RegistrosTemperatura.OrderBy(x => x.FechaHora))
        {
            sb.AppendLine(
                $"{r.FechaHora:yyyy-MM-dd HH:mm},{r.Temperatura},{r.Observacion}"
            );
        }

        return File(
            Encoding.UTF8.GetBytes(sb.ToString()),
            "text/csv",
            $"Temperaturas_{c.Contenedor}.csv"
        );
    }

    public async Task<IActionResult> ExportarPdf(int id)
    {
        var c = await _context.ContenedoresRefrigerados
            .Include(x => x.RegistrosTemperatura)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (c == null) return NotFound();

        var pdf = new ContenedorTemperaturasPdf(c);
        var bytes = pdf.GeneratePdf();

        return File(bytes, "application/pdf",
            $"Temperaturas_{c.Contenedor}.pdf");
    }

    // ===============================
    // 🔍 BÚSQUEDA CONTENEDOR REFRIGERADO
    // ===============================
    [HttpGet]
    public async Task<IActionResult> Buscar(string contenedor)
    {
        if (string.IsNullOrWhiteSpace(contenedor))
            return RedirectToAction(nameof(Index));

        contenedor = contenedor.Trim().ToUpper();

        var resultado = await _context.ContenedoresRefrigerados
            .Where(c => c.Contenedor.Contains(contenedor))
            .OrderByDescending(c => c.FechaHoraIngreso)
            .ToListAsync();

        return View("Index", resultado); // reutiliza la misma vista
    }

}

