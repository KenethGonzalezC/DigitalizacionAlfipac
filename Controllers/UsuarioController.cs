using BitacoraAlfipac.Data;
using BitacoraAlfipac.Documents;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using QuestPDF.Fluent;

namespace BitacoraAlfipac.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class UsuarioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // ============================
        // LISTADO SOLO LECTURA
        // ============================
        public async Task<IActionResult> Refrigerados(string contenedor)
        {
            var query = _context.ContenedoresRefrigerados.AsQueryable();

            if (!string.IsNullOrWhiteSpace(contenedor))
            {
                contenedor = contenedor.Trim().ToUpper();
                query = query.Where(x => x.Contenedor.Contains(contenedor));
            }

            var lista = await query
                .OrderByDescending(x => x.FechaHoraIngreso)
                .ToListAsync();

            ViewBag.Activos = lista
                .Where(x => x.FechaHoraDespacho == null)
                .ToList();

            ViewBag.Despachados = lista
                .Where(x => x.FechaHoraDespacho != null)
                .ToList();

            ViewBag.Busqueda = contenedor;

            return View();
        }

        // ============================
        // DETALLE SOLO LECTURA
        // ============================
        public async Task<IActionResult> DetalleRefrigerado(int id)
        {
            var contenedor = await _context.ContenedoresRefrigerados
                .Include(x => x.RegistrosTemperatura)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (contenedor == null)
                return NotFound();

            return View(contenedor);
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
    }
}