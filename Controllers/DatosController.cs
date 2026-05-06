using BitacoraAlfipac.Data;
using BitacoraAlfipac.Models.Entidades;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    string marchamo,
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

        // ✅ TOTAL GENERAL (sin filtros)
        ViewBag.TotalContenedores = _context.DatosIngresosViajes.Count();

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
            string.IsNullOrEmpty(marchamo) &&
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
        {
            var fecha = fechaCreacion.Value.Date;

            query = query.Where(x =>
                x.FechaCreacionViaje != null &&
                x.FechaCreacionViaje.Value.Date == fecha);
        }

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

        var existe = _context.DatosIngresosViajes
            .Any(x => x.Contenedor == model.Contenedor);

        if (existe)
        {
            ModelState.AddModelError("Contenedor", "Ya existe un ingreso con este contenedor.");
            return RedirectToAction(nameof(Ingresos));
        }

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
        registro.Marchamo = model.Marchamo;
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
                var marchamo = row.Cell(3).GetValue<string>()?.Trim();

                if (string.IsNullOrWhiteSpace(viaje) || string.IsNullOrWhiteSpace(contenedor))
                    continue;

                if (contenedoresExistentes.Contains(contenedor))
                {
                    duplicados.Add(contenedor);
                    continue;
                }

                DateTime fechaCreacion;
                var celdaFecha = row.Cell(5);

                if (celdaFecha.DataType == XLDataType.DateTime)
                    fechaCreacion = celdaFecha.GetDateTime();
                else if (!DateTime.TryParse(celdaFecha.GetValue<string>(), out fechaCreacion))
                    fechaCreacion = DateTime.Now;

                var registro = new DatosIngresoViaje
                {
                    Viaje = viaje,
                    Contenedor = contenedor,
                    Marchamo = marchamo ?? "",
                    RecintoOrigen = row.Cell(4).GetValue<string>()?.Trim() ?? "",
                    FechaCreacionViaje = fechaCreacion,
                    Declarante = row.Cell(6).GetValue<string>()?.Trim() ?? "",
                    Transportista = row.Cell(7).GetValue<string>()?.Trim() ?? "",
                    Mercancia = row.Cell(8).GetValue<string>()?.Trim() ?? "",
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

    //Precargar despacho
    private async Task<(object? contenedor, string? patio)> BuscarContenedorGlobal(string numero)
    {
        numero = numero.ToUpper();

        var sinAsignar = await _context.ContenedoresSinAsignarPatio
            .FirstOrDefaultAsync(c => c.Contenedor == numero);
        if (sinAsignar != null) return (sinAsignar, "Sin Asignar");

        var q = await _context.PatioQuimicos.FirstOrDefaultAsync(c => c.Contenedor == numero);
        if (q != null) return (q, "Patio Químicos");

        var p1 = await _context.Patio1.FirstOrDefaultAsync(c => c.Contenedor == numero);
        if (p1 != null) return (p1, "Patio 1");

        var p2 = await _context.Patio2.FirstOrDefaultAsync(c => c.Contenedor == numero);
        if (p2 != null) return (p2, "Patio 2");

        var a = await _context.Anden2000.FirstOrDefaultAsync(c => c.Contenedor == numero);
        if (a != null) return (a, "Andén 2000");

        return (null, null);
    }

    //Despachos
    //Traer informacion del contenedor
    [HttpGet]
    public async Task<IActionResult> BuscarParaDespacho(string contenedor)
    {
        if (string.IsNullOrWhiteSpace(contenedor))
            return Json(new { encontrado = false });

        contenedor = contenedor.ToUpper().Trim();

        // 🔍 Verificar si ya fue despachado
        var historial = await _context.HistorialContenedores
            .Where(h => h.Contenedor == contenedor && h.FechaHoraSalida != null)
            .OrderByDescending(h => h.FechaHoraSalida)
            .FirstOrDefaultAsync();

        if (historial != null)
        {
            return Json(new
            {
                encontrado = false,
                mensaje = $"Este contenedor ya fue despachado el {historial.FechaHoraSalida:dd/MM/yyyy HH:mm}"
            });
        }

        // 🔍 Buscar en patios
        var (data, patio) = await BuscarContenedorGlobal(contenedor);

        if (data == null)
            return Json(new { encontrado = false });

        var c = (IContenedorInventario)data;

        return Json(new
        {
            encontrado = true,
            contenedor = c.Contenedor,
            marchamos = c.Marchamos ?? "",
            chasis = c.Chasis ?? "",
            transportista = c.Transportista ?? "",
            cliente = c.Cliente ?? "",
            estado = c.EstadoCarga ?? "",
            patio
        });
    }

    //precarga
    // POST: Datos/RegistrarPrecargaDespacho
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarPrecargaDespacho(string contenedor)
    {
        if (string.IsNullOrWhiteSpace(contenedor))
            return RedirectToAction("Despachos");

        contenedor = contenedor.ToUpper().Trim();

        // 🔍 Buscar en patios
        var (data, patio) = await BuscarContenedorGlobal(contenedor);

        if (data == null)
        {
            TempData["Error"] = "Contenedor no encontrado en patios";
            return RedirectToAction("Despachos");
        }

        var c = (IContenedorInventario)data;

        if (string.IsNullOrWhiteSpace(c.Contenedor))
        {
            TempData["Warning"] = "El contenedor es requerido";
            return RedirectToAction("Despachos");
        }

        bool existe = await _context.DatosDespachosViajes
            .AnyAsync(x => x.Contenedor == c.Contenedor);

        if (existe)
        {
            TempData["Warning"] = "El contenedor ya tiene una precarga registrada";
            return RedirectToAction("Despachos");
        }

        _context.DatosDespachosViajes.Add(new DatosDespachoViaje
        {
            Contenedor = c.Contenedor,
            Marchamos = c.Marchamos,
            Transportista = c.Transportista,
            Cliente = c.Cliente,
            Chasis = c.Chasis,

            Chofer = null,
            PlacaCabezal = null,
            ViajeDua = null,
            Boleta = null,

            FechaCreacion = DateTime.Now
        });

        await _context.SaveChangesAsync();


        TempData["Ok"] = "Datos de despacho precargados correctamente";

        return RedirectToAction("Despachos");
    }

    // Página hija → Despachos
    public IActionResult Despachos(
        string contenedor,
        string marchamos,
        string transportista,
        string cliente,
        string chofer,
        string placa,
        string chasis,
        string viajeDua,
        string fechaRegistro,
        string boleta,
        int pagina = 1)
    {
        int registrosPorPagina = 50;

        var baseQuery = _context.DatosDespachosViajes.AsQueryable();
        var query = baseQuery;
        var queryContador = baseQuery;

        DateTime? fechaRegistroDate = null;

        if (!string.IsNullOrEmpty(fechaRegistro) &&
            DateTime.TryParseExact(fechaRegistro, "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var fechaParsed))
        {
            fechaRegistroDate = fechaParsed;
        }

        bool sinFiltros =
            string.IsNullOrEmpty(contenedor) &&
            string.IsNullOrEmpty(marchamos) &&
            string.IsNullOrEmpty(transportista) &&
            string.IsNullOrEmpty(cliente) &&
            string.IsNullOrEmpty(chofer) &&
            string.IsNullOrEmpty(placa) &&
            string.IsNullOrEmpty(chasis) &&
            string.IsNullOrEmpty(viajeDua) &&
            string.IsNullOrEmpty(boleta) &&
            !fechaRegistroDate.HasValue;

        //if (sinFiltros)
            //fechaRegistroDate = DateTime.Today;

        // 🔎 FILTROS
        if (!string.IsNullOrEmpty(contenedor))
            queryContador = queryContador.Where(x => x.Contenedor.Contains(contenedor));

        if (!string.IsNullOrEmpty(marchamos))
            queryContador = queryContador.Where(x => x.Marchamos.Contains(marchamos));

        if (!string.IsNullOrEmpty(transportista))
            queryContador = queryContador.Where(x => x.Transportista.Contains(transportista));

        if (!string.IsNullOrEmpty(cliente))
            queryContador = queryContador.Where(x => x.Cliente.Contains(cliente));

        if (!string.IsNullOrEmpty(chofer))
            queryContador = queryContador.Where(x => x.Chofer.Contains(chofer));

        if (!string.IsNullOrEmpty(placa))
            queryContador = queryContador.Where(x => x.PlacaCabezal.Contains(placa));

        if (!string.IsNullOrEmpty(chasis))
            queryContador = queryContador.Where(x => x.Chasis.Contains(chasis));

        if (!string.IsNullOrEmpty(viajeDua))
            queryContador = queryContador.Where(x => x.ViajeDua.Contains(viajeDua));

        if (!string.IsNullOrEmpty(boleta))
            queryContador = queryContador.Where(x => x.Boleta.Contains(boleta));

        if (fechaRegistroDate.HasValue)
            query = query.Where(x => x.FechaCreacion.Date == fechaRegistroDate.Value.Date);

        var totalRegistros = query.Count(); // tabla (paginación)

        var totalContenedores = queryContador.Count(); // independiente
        ViewBag.TotalContenedores = totalContenedores;

        var datos = query
            .OrderByDescending(x => x.FechaCreacion)
            .Skip((pagina - 1) * registrosPorPagina)
            .Take(registrosPorPagina)
            .ToList();

        ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);
        ViewBag.PaginaActual = pagina;
        ViewBag.FechaRegistro = fechaRegistroDate?.ToString("yyyy-MM-dd");

        return View(datos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearDespacho(DatosDespachoViaje model)
    {
        model.Contenedor = model.Contenedor.ToUpper().Trim();
        model.FechaCreacion = DateTime.Now;

        _context.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("Despachos");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarDespacho(DatosDespachoViaje model)
    {
        var data = await _context.DatosDespachosViajes.FindAsync(model.Id);

        if (data == null)
            return RedirectToAction("Despachos");

        data.Marchamos = model.Marchamos;
        data.Transportista = model.Transportista;
        data.Cliente = model.Cliente;
        data.Chasis = model.Chasis;
        data.Chofer = model.Chofer;
        data.PlacaCabezal = model.PlacaCabezal;
        data.ViajeDua = model.ViajeDua;
        data.Boleta = model.Boleta;

        await _context.SaveChangesAsync();

        return RedirectToAction("Despachos");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarDespacho(int id)
    {
        var data = await _context.DatosDespachosViajes.FindAsync(id);

        if (data != null)
        {
            _context.Remove(data);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Despachos");
    }

    [HttpGet]
    public async Task<IActionResult> BuscarDespacho(string contenedor)
    {
        if (string.IsNullOrWhiteSpace(contenedor))
            return Json(new { encontrado = false });

        contenedor = contenedor.ToUpper().Trim();

        var ingreso = await _context.DatosDespachosViajes
            .Where(x => x.Contenedor == contenedor)
            .Select(x => new
            {
                contenedor = x.Contenedor,
                marchamos = x.Marchamos,
                transportista = x.Transportista,
                cliente = x.Cliente,
                chofer = x.Chofer,
                placaCabezal = x.PlacaCabezal,
                chasis = x.Chasis,
                viajeDua = x.ViajeDua
            })
            .FirstOrDefaultAsync();

        if (ingreso == null)
            return Json(new { encontrado = false });

        return Json(new
        {
            encontrado = true,
            contenedor = ingreso.contenedor,
            marchamos = ingreso.marchamos,
            transportista = ingreso.transportista,
            cliente = ingreso.cliente,
            chofer = ingreso.chofer,
            placaCabezal = ingreso.placaCabezal,
            chasis = ingreso.chasis,
            viajeDua = ingreso.viajeDua
        });
    }
}
