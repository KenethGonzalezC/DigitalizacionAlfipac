using BitacoraAlfipac.Data;
using BitacoraAlfipac.Documents;
using BitacoraAlfipac.Models.Entidades;
using BitacoraAlfipac.Models.ViewModels;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System.IO;
using System.Text;

namespace BitacoraAlfipac.Controllers;

[Authorize]
public class BitacoraIngresosController : Controller
{
    private readonly ApplicationDbContext _context;

    public BitacoraIngresosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET
    public async Task<IActionResult> Index(DateTime? fecha)
    {
        DateTime fechaSeleccionada = fecha?.Date ?? DateTime.Today;

        var ingresos = await _context.BitacoraIngresos
            .Where(i => i.FechaHoraIngreso.Date == fechaSeleccionada)
            .OrderBy(i => i.FechaHoraIngreso)
            .ToListAsync();

        var model = new BitacoraIngresosViewModel
        {
            FechaSeleccionada = fechaSeleccionada,
            FechaHoraIngreso = DateTime.Now,
            Ingresos = ingresos
        };

        return View(model);
    }

    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(BitacoraIngresosViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Ingresos = await _context.BitacoraIngresos
                .Where(i => i.FechaHoraIngreso.Date == model.FechaSeleccionada)
                .OrderBy(i => i.FechaHoraIngreso)
                .ToListAsync();

            return View(model);
        }

        // Limpiar segundos y milisegundos
        var fechaHora = model.FechaHoraIngreso;
        fechaHora = new DateTime(
            fechaHora.Year,
            fechaHora.Month,
            fechaHora.Day,
            fechaHora.Hour,
            fechaHora.Minute,
            0
        );

        var ingreso = new BitacoraIngreso
        {
            Contenedor = model.Contenedor,
            Marchamos = model.Marchamos,
            FechaHoraIngreso = fechaHora,
            Transportista = model.Transportista,
            Cliente = model.Cliente,
            Tamaño = model.Tamano,
            Chofer = model.Chofer,
            PlacaCabezal = model.PlacaCabezal,
            Chasis = model.Chasis,
            ViajeDua = model.ViajeDua
        };

        _context.BitacoraIngresos.Add(ingreso);

        _context.HistorialContenedores.Add(new HistorialContenedor
        {
            Contenedor = model.Contenedor,
            FechaHoraIngreso = fechaHora
        });

        _context.ContenedoresSinAsignarPatio.Add(new ContenedorSinAsignarPatio
        {
            Contenedor = model.Contenedor,
            Marchamos = model.Marchamos,
            Tamano = model.Tamano,
            Transportista = model.Transportista,
            Cliente = model.Cliente,
            Chasis = model.Chasis,
            EstadoCarga = "Cargado",
            Ubicacion = "Sin asignar"
        });

        // 🔹 SI ES REFRIGERADO, CREARLO EN EL MODULO REEFER
        if (model.EsRefrigerado)
        {
            bool yaExiste = await _context.ContenedoresRefrigerados
                .AnyAsync(r => r.Contenedor == model.Contenedor && r.FechaHoraIngreso == null);

            if (!yaExiste)
            {
                _context.ContenedoresRefrigerados.Add(new ContenedorRefrigerado
                {
                    Contenedor = model.Contenedor,
                    FechaHoraIngreso = fechaHora,
                });
            }
        }

        await _context.SaveChangesAsync();

        // 🔑 CLAVE: refrescar la MISMA página con la fecha seleccionada
        return RedirectToAction(nameof(Index), new { fecha = fechaHora.Date });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(BitacoraIngresosViewModel model)
    {
        var ingreso = await _context.BitacoraIngresos
            .FirstOrDefaultAsync(i => i.Id == model.Id);

        if (ingreso == null)
            return NotFound();

        ingreso.Contenedor = model.Contenedor;
        ingreso.Marchamos = model.Marchamos;
        ingreso.Transportista = model.Transportista;
        ingreso.Cliente = model.Cliente;
        ingreso.Tamaño = model.Tamano;
        ingreso.Chasis = model.Chasis;
        ingreso.ViajeDua = model.ViajeDua;

        ingreso.FechaHoraIngreso = new DateTime(
            model.FechaHoraIngreso.Year,
            model.FechaHoraIngreso.Month,
            model.FechaHoraIngreso.Day,
            model.FechaHoraIngreso.Hour,
            model.FechaHoraIngreso.Minute,
            0
        );

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { fecha = ingreso.FechaHoraIngreso.Date });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, DateTime fecha)
    {
        var ingreso = await _context.BitacoraIngresos.FindAsync(id);
        if (ingreso == null)
            return RedirectToAction(nameof(Index), new { fecha });

        string contenedor = ingreso.Contenedor;

        // Bitácora ingresos
        _context.BitacoraIngresos.Remove(ingreso);

        // Historial activo
        var historial = await _context.HistorialContenedores
            .FirstOrDefaultAsync(h => h.Contenedor == contenedor && h.FechaHoraSalida == null);

        if (historial != null)
            _context.HistorialContenedores.Remove(historial);

        // Sin asignar patio
        var sinPatio = await _context.ContenedoresSinAsignarPatio
            .FirstOrDefaultAsync(c => c.Contenedor == contenedor);

        if (sinPatio != null)
            _context.ContenedoresSinAsignarPatio.Remove(sinPatio);

        // Patios
        _context.PatioQuimicos.RemoveRange(
            _context.PatioQuimicos.Where(p => p.Contenedor == contenedor));

        _context.Patio1.RemoveRange(
            _context.Patio1.Where(p => p.Contenedor == contenedor));

        _context.Patio2.RemoveRange(
            _context.Patio2.Where(p => p.Contenedor == contenedor));

        _context.Anden2000.RemoveRange(
            _context.Anden2000.Where(p => p.Contenedor == contenedor));

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { fecha });
    }

    private void RefrescarDatosContenedor(
    ContenedorBase contenedorActivo,
    BitacoraIngreso ingreso)
    {
        contenedorActivo.Contenedor = ingreso.Contenedor;
        contenedorActivo.Marchamos = ingreso.Marchamos;
        contenedorActivo.Tamano = ingreso.Tamaño;
        contenedorActivo.Transportista = ingreso.Transportista;
        contenedorActivo.Cliente = ingreso.Cliente;
        contenedorActivo.Chasis = ingreso.Chasis;
    }

    private async Task<ContenedorBase?> ObtenerContenedorActivo(string contenedor)
    {
        return null;
    }

    public async Task<IActionResult> ExportarExcel(DateTime fecha)
    {
        var ingresos = await _context.BitacoraIngresos
            .Where(i => i.FechaHoraIngreso.Date == fecha.Date)
            .OrderBy(i => i.FechaHoraIngreso)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Bitacora");

        // Encabezados
        ws.Cell(1, 1).Value = "Contenedor";
        ws.Cell(1, 2).Value = "Marchamos";
        ws.Cell(1, 3).Value = "Fecha/Hora";
        ws.Cell(1, 4).Value = "Transportista";
        ws.Cell(1, 5).Value = "Cliente";
        ws.Cell(1, 6).Value = "Tamaño";
        ws.Cell(1, 7).Value = "Chofer";
        ws.Cell(1, 8).Value = "Placa Cabezal";
        ws.Cell(1, 9).Value = "Chasis";
        ws.Cell(1, 10).Value = "Viaje / DUA";

        int fila = 2;

        foreach (var i in ingresos)
        {
            ws.Cell(fila, 1).Value = i.Contenedor;
            ws.Cell(fila, 2).Value = i.Marchamos;
            ws.Cell(fila, 3).Value = i.FechaHoraIngreso.ToString("yyyy-MM-dd HH:mm");
            ws.Cell(fila, 4).Value = i.Transportista;
            ws.Cell(fila, 5).Value = i.Cliente;
            ws.Cell(fila, 6).Value = i.Tamaño;
            ws.Cell(fila, 7).Value = i.Chofer;
            ws.Cell(fila, 8).Value = i.PlacaCabezal;
            ws.Cell(fila, 9).Value = i.Chasis;
            ws.Cell(fila, 10).Value = i.ViajeDua;
            fila++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Ingresos{fecha:yyyyMMdd}.xlsx"
        );
    }

    public async Task<IActionResult> ExportarCsv(DateTime fecha)
    {
        var ingresos = await _context.BitacoraIngresos
            .Where(i => i.FechaHoraIngreso.Date == fecha.Date)
            .OrderBy(i => i.FechaHoraIngreso)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine(
            "Contenedor,Marchamos,FechaHora,Transportista,Cliente,Tamaño,Chofer,PlacaCabezal,Chasis,ViajeDUA"
        );

        foreach (var i in ingresos)
        {
            sb.AppendLine(
                $"{i.Contenedor}," +
                $"{i.Marchamos}," +
                $"{i.FechaHoraIngreso:yyyy-MM-dd HH:mm}," +
                $"{i.Transportista}," +
                $"{i.Cliente}," +
                $"{i.Tamaño}," +
                $"{i.Chofer}," +
                $"{i.PlacaCabezal}," +
                $"{i.Chasis}," +
                $"{i.ViajeDua}"
            );
        }

        return File(
            Encoding.UTF8.GetBytes(sb.ToString()),
            "text/csv",
            $"Ingresos{fecha:yyyyMMdd}.csv"
        );
    }

    public async Task<IActionResult> ExportarPdf(DateTime? fecha)
    {
        DateTime fechaSeleccionada = fecha?.Date ?? DateTime.Today;
        var ingresos = await _context.BitacoraIngresos
            .Where(i => i.FechaHoraIngreso.Date == fechaSeleccionada)
            .OrderBy(i => i.FechaHoraIngreso)
            .ToListAsync();

        var document = new BitacoraIngresosPdf(fechaSeleccionada, ingresos);

        byte[] pdf = document.GeneratePdf();

        return File(
            pdf,
            "application/pdf",
            $"Ingresos_{fechaSeleccionada:yyyy/MM/dd}.pdf"
        );

    }

    [HttpGet]
    public IActionResult ObtenerDatosPorContenedor(string contenedor)
    {
        if (string.IsNullOrWhiteSpace(contenedor))
            return Json(null);

        var ingreso = _context.DatosIngresosViajes
            .FirstOrDefault(x => x.Contenedor == contenedor);

        if (ingreso == null)
            return Json(null);

        // Buscar transportista autorizado por cédula jurídica
        var transportistaAutorizado = _context.TransportistasAutorizados
            .FirstOrDefault(t => t.CedulaJuridica == ingreso.Transportista);

        var resultado = new
        {
            cliente = ingreso.Declarante,
            transportista = transportistaAutorizado?.Codigo ?? ingreso.Transportista,
            viaje = ingreso.Viaje
        };

        return Json(resultado);
    }


}
