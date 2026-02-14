using BitacoraAlfipac.Data;
using BitacoraAlfipac.Models.Entidades;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Globalization;

public class DatosController : Controller
{
    private readonly ApplicationDbContext _context;

    public DatosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 📌 Página principal del módulo Datos
    public IActionResult Index()
    {
        return View(); // Views/Datos/Index.cshtml
    }

    // Página hija → Ingresos
    public IActionResult Ingresos(
    string viaje,
    string contenedor,
    string recinto,
    DateTime? fechaCreacion,
    string declarante,
    string transportista,
    string mercancia,
    string fechaRegistro,   // 👈 ahora es STRING
    int pagina = 1)
    {
        int registrosPorPagina = 50;

        var query = _context.DatosIngresosViajes.AsQueryable();

        // 🔹 Convertir fechaRegistro (string) a DateTime?
        DateTime? fechaRegistroDate = null;

        if (!string.IsNullOrEmpty(fechaRegistro) &&
            DateTime.TryParseExact(fechaRegistro, "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var fechaParsed))
        {
            fechaRegistroDate = fechaParsed;
        }

        // 🧠 Detectar si NO hay filtros (para cargar HOY por defecto)
        bool sinFiltros =
            string.IsNullOrEmpty(viaje) &&
            string.IsNullOrEmpty(contenedor) &&
            string.IsNullOrEmpty(recinto) &&
            !fechaCreacion.HasValue &&
            string.IsNullOrEmpty(declarante) &&
            string.IsNullOrEmpty(transportista) &&
            string.IsNullOrEmpty(mercancia) &&
            !fechaRegistroDate.HasValue;

        // 🔥 Si no hay filtros → mostrar registros de HOY
        if (sinFiltros)
        {
            fechaRegistroDate = DateTime.Today;
        }

        // 🔎 FILTROS
        if (!string.IsNullOrEmpty(viaje))
            query = query.Where(x => x.Viaje.Contains(viaje));

        if (!string.IsNullOrEmpty(contenedor))
            query = query.Where(x => x.Contenedor.Contains(contenedor));

        if (!string.IsNullOrEmpty(recinto))
            query = query.Where(x => x.RecintoOrigen.Contains(recinto));

        if (fechaCreacion.HasValue)
            query = query.Where(x => x.FechaCreacionViaje.Date == fechaCreacion.Value.Date);

        if (!string.IsNullOrEmpty(declarante))
            query = query.Where(x => x.Declarante.Contains(declarante));

        if (!string.IsNullOrEmpty(transportista))
            query = query.Where(x => x.Transportista.Contains(transportista));

        if (!string.IsNullOrEmpty(mercancia))
            query = query.Where(x => x.Mercancia.Contains(mercancia));

        if (fechaRegistroDate.HasValue)
            query = query.Where(x => x.FechaRegistroSistema.Date == fechaRegistroDate.Value.Date);

        var totalRegistros = query.Count();

        var datos = query
            .OrderByDescending(x => x.FechaRegistroSistema)
            .Skip((pagina - 1) * registrosPorPagina)
            .Take(registrosPorPagina)
            .ToList();

        ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);
        ViewBag.PaginaActual = pagina;
        ViewBag.FechaRegistro = fechaRegistroDate?.ToString("yyyy-MM-dd"); // 👈 formato correcto para el input

        return View(datos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CrearIngreso(DatosIngresoViaje model)
    {
        model.FechaRegistroSistema = DateTime.Now;
        _context.Add(model);
        _context.SaveChanges();

        return RedirectToAction(nameof(Ingresos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditarIngreso(DatosIngresoViaje model)
    {
        var registro = _context.DatosIngresosViajes.Find(model.Id);
        if (registro == null) return NotFound();

        registro.Viaje = model.Viaje;
        registro.Contenedor = model.Contenedor;
        registro.RecintoOrigen = model.RecintoOrigen;
        registro.FechaCreacionViaje = model.FechaCreacionViaje;
        registro.Declarante = model.Declarante;
        registro.Transportista = model.Transportista;
        registro.Mercancia = model.Mercancia;

        _context.SaveChanges();

        return RedirectToAction(nameof(Ingresos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EliminarIngreso(int id)
    {
        var registro = _context.DatosIngresosViajes.Find(id);
        if (registro == null) return NotFound();

        _context.DatosIngresosViajes.Remove(registro);
        _context.SaveChanges();

        return RedirectToAction(nameof(Ingresos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ImportarExcel(IFormFile archivoExcel)
    {
        if (archivoExcel == null || archivoExcel.Length == 0)
        {
            TempData["Error"] = "Debe seleccionar un archivo Excel.";
            return RedirectToAction(nameof(Ingresos));
        }

        try
        {
            using var stream = new MemoryStream();
            archivoExcel.CopyTo(stream);

            using var workbook = new XLWorkbook(stream);
            var hoja = workbook.Worksheets.FirstOrDefault();

            if (hoja == null || hoja.LastRowUsed() == null)
            {
                TempData["Error"] = "El archivo no contiene datos.";
                return RedirectToAction(nameof(Ingresos));
            }

            var ultimaFila = hoja.LastRowUsed().RowNumber();

            var contenedoresExistentes = _context.DatosIngresosViajes
                .Select(x => x.Contenedor.Trim())
                .ToHashSet();

            var nuevosRegistros = new List<DatosIngresoViaje>();
            var duplicados = new List<string>();

            for (int fila = 2; fila <= ultimaFila; fila++)
            {
                var row = hoja.Row(fila);

                var viaje = row.Cell(1).GetValue<string>()?.Trim();
                var contenedor = row.Cell(2).GetValue<string>()?.Trim();

                if (string.IsNullOrWhiteSpace(viaje) || string.IsNullOrWhiteSpace(contenedor))
                    continue;

                if (contenedoresExistentes.Contains(contenedor))
                {
                    duplicados.Add(contenedor);
                    continue;
                }

                DateTime fechaCreacion;
                var celdaFecha = row.Cell(4);

                if (celdaFecha.DataType == XLDataType.DateTime)
                    fechaCreacion = celdaFecha.GetDateTime();
                else if (!DateTime.TryParse(celdaFecha.GetValue<string>(), out fechaCreacion))
                    fechaCreacion = DateTime.Now;

                var registro = new DatosIngresoViaje
                {
                    Viaje = viaje,
                    Contenedor = contenedor,
                    RecintoOrigen = row.Cell(3).GetValue<string>()?.Trim() ?? "",
                    FechaCreacionViaje = fechaCreacion,
                    Declarante = row.Cell(5).GetValue<string>()?.Trim() ?? "",
                    Transportista = row.Cell(6).GetValue<string>()?.Trim() ?? "",
                    Mercancia = row.Cell(7).GetValue<string>()?.Trim() ?? "",
                    FechaRegistroSistema = DateTime.Now
                };

                nuevosRegistros.Add(registro);
                contenedoresExistentes.Add(contenedor);
            }

            if (nuevosRegistros.Any())
            {
                _context.DatosIngresosViajes.AddRange(nuevosRegistros);
                _context.SaveChanges();
            }

            // 🔔 Mensajes
            if (duplicados.Any())
            {
                TempData["Warning"] =
                    $"Se importaron {nuevosRegistros.Count} registros. " +
                    $"No se insertaron {duplicados.Count} contenedores duplicados: " +
                    $"{string.Join(", ", duplicados.Distinct())}";
            }
            else
            {
                TempData["Success"] =
                    $"Registros importados correctamente: {nuevosRegistros.Count}";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Error al importar: " + ex.Message;
        }

        return RedirectToAction(nameof(Ingresos));
    }

    //limpiar tabla
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult BorrarPorFecha(DateTime fechaRegistro)
    {
        // Tomamos solo la fecha (sin horas)
        var fecha = fechaRegistro.Date;

        var registros = _context.DatosIngresosViajes
            .Where(x => x.FechaRegistroSistema.Date == fecha)
            .ToList();

        if (registros.Any())
        {
            _context.DatosIngresosViajes.RemoveRange(registros);
            _context.SaveChanges();
            // TempData["Success"] = $"Se eliminaron {registros.Count} registros del {fecha:dd/MM/yyyy}";
        }
        else
        {
            // TempData["Error"] = "No hay registros para esa fecha";
        }

        return RedirectToAction(nameof(Ingresos));
    }

}
