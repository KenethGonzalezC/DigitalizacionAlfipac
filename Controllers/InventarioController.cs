using Aspose.Pdf.Forms;
using BitacoraAlfipac.Data;
using BitacoraAlfipac.Models.Entidades;
using BitacoraAlfipac.Models.ViewModels;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Presentation;
using iText.Kernel.Pdf.Canvas.Wmf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;

public class InventarioController : Controller
{
    private readonly ApplicationDbContext _context;

    public InventarioController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ===============================
    // PÁGINA PRINCIPAL
    // ===============================
    public IActionResult Index()
    {
        var vm = new InventarioIndexVM
        {
            ContenedoresSinAsignar = _context.ContenedoresSinAsignarPatio
                .OrderByDescending(c => c.Id)
                .ToList()
        };

        return View(vm);
    }

    // ===============================
    // MOVER CONTENEDOR
    // ===============================
    [HttpPost]
public async Task<IActionResult> Mover(
    int id,
    string marchamos,
    string estadoCarga,
    string ubicacion)
{
    var c = await _context.ContenedoresSinAsignarPatio.FindAsync(id);
    if (c == null)
        return NotFound();

        if (!string.IsNullOrWhiteSpace(estadoCarga))
            c.EstadoCarga = estadoCarga;

        if (!string.IsNullOrWhiteSpace(marchamos))
            c.Marchamos = marchamos;

        // Datos editables
        c.Marchamos = marchamos;
    c.EstadoCarga = estadoCarga;

        if (string.IsNullOrWhiteSpace(ubicacion))
        {
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        switch (ubicacion)
    {
        case "Patio1":
            _context.Patio1.Add(new Patio1
            {
                Contenedor = c.Contenedor,
                Marchamos = c.Marchamos,
                Tamano = c.Tamano,
                Chasis = c.Chasis,
                Transportista = c.Transportista,
                Cliente = c.Cliente,
                EstadoCarga = c.EstadoCarga
            });
            break;

        case "Patio2":
            _context.Patio2.Add(new Patio2
            {
                Contenedor = c.Contenedor,
                Marchamos = c.Marchamos,
                Tamano = c.Tamano,
                Chasis = c.Chasis,
                Transportista = c.Transportista,
                Cliente = c.Cliente,
                EstadoCarga = c.EstadoCarga
            });
            break;

        case "Anden2000":
            _context.Anden2000.Add(new Anden2000
            {
                Contenedor = c.Contenedor,
                Marchamos = c.Marchamos,
                Tamano = c.Tamano,
                Chasis = c.Chasis,
                Transportista = c.Transportista,
                Cliente = c.Cliente,
                EstadoCarga = c.EstadoCarga
            });
            break;

        case "PatioQuimicos":
            _context.PatioQuimicos.Add(new PatioQuimicos
            {
                Contenedor = c.Contenedor,
                Marchamos = c.Marchamos,
                Tamano = c.Tamano,
                Chasis = c.Chasis,
                Transportista = c.Transportista,
                Cliente = c.Cliente,
                EstadoCarga = c.EstadoCarga
            });
            break;

            case "SinAsignar":
                break;

            default:
            return BadRequest("Ubicación inválida");
    }

    // Regla clave: NO puede estar en dos lugares
    _context.ContenedoresSinAsignarPatio.Remove(c);

    await _context.SaveChangesAsync();

    return RedirectToAction("Index");
}

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarEdicionSinAsignar(
    int Id,
    string Marchamos,
    string EstadoCarga,
    string? Ubicacion,
    string UbicacionActual,
    string Chasis,
    string Transportista,
    string Cliente)
    {
        IContenedorInventario? contenedor = null;

        // 1️⃣ BUSCAR DONDE ESTÁ
        switch (UbicacionActual)
        {
            case "SinAsignar":
                contenedor = await _context.ContenedoresSinAsignarPatio.FindAsync(Id);
                break;
            case "Patio1":
                contenedor = await _context.Patio1.FindAsync(Id);
                break;
            case "Patio2":
                contenedor = await _context.Patio2.FindAsync(Id);
                break;
            case "Anden2000":
                contenedor = await _context.Anden2000.FindAsync(Id);
                break;
            case "PatioQuimicos":
                contenedor = await _context.PatioQuimicos.FindAsync(Id);
                break;
        }

        if (contenedor == null)
            return Json(new { success = false });

        // 2️⃣ ACTUALIZAR TODOS LOS DATOS
        contenedor.Marchamos = Marchamos;
        contenedor.EstadoCarga = EstadoCarga;
        contenedor.Chasis = Chasis;
        contenedor.Transportista = Transportista;
        contenedor.Cliente = Cliente;

        await _context.SaveChangesAsync();

        // 3️⃣ SI HAY MOVIMIENTO
        if (!string.IsNullOrWhiteSpace(Ubicacion) && Ubicacion != UbicacionActual)
        {
            switch (Ubicacion)
            {
                case "Patio1":
                    _context.Patio1.Add(new Patio1
                    {
                        Contenedor = contenedor.Contenedor,
                        Marchamos = contenedor.Marchamos,
                        Tamano = contenedor.Tamano,
                        Chasis = contenedor.Chasis,
                        Transportista = contenedor.Transportista,
                        Cliente = contenedor.Cliente,
                        EstadoCarga = contenedor.EstadoCarga
                    });
                    break;

                case "Patio2":
                    _context.Patio2.Add(new Patio2
                    {
                        Contenedor = contenedor.Contenedor,
                        Marchamos = contenedor.Marchamos,
                        Tamano = contenedor.Tamano,
                        Chasis = contenedor.Chasis,
                        Transportista = contenedor.Transportista,
                        Cliente = contenedor.Cliente,
                        EstadoCarga = contenedor.EstadoCarga
                    });
                    break;

                case "Anden2000":
                    _context.Anden2000.Add(new Anden2000
                    {
                        Contenedor = contenedor.Contenedor,
                        Marchamos = contenedor.Marchamos,
                        Tamano = contenedor.Tamano,
                        Chasis = contenedor.Chasis,
                        Transportista = contenedor.Transportista,
                        Cliente = contenedor.Cliente,
                        EstadoCarga = contenedor.EstadoCarga
                    });
                    break;

                case "PatioQuimicos":
                    _context.PatioQuimicos.Add(new PatioQuimicos
                    {
                        Contenedor = contenedor.Contenedor,
                        Marchamos = contenedor.Marchamos,
                        Tamano = contenedor.Tamano,
                        Chasis = contenedor.Chasis,
                        Transportista = contenedor.Transportista,
                        Cliente = contenedor.Cliente,
                        EstadoCarga = contenedor.EstadoCarga
                    });
                    break;

                case "SinAsignar":
                    _context.ContenedoresSinAsignarPatio.Add(new ContenedorSinAsignarPatio
                    {
                        Contenedor = contenedor.Contenedor,
                        Marchamos = contenedor.Marchamos,
                        Tamano = contenedor.Tamano,
                        Chasis = contenedor.Chasis,
                        Transportista = contenedor.Transportista,
                        Cliente = contenedor.Cliente,
                        EstadoCarga = contenedor.EstadoCarga
                    });
                    break;
            }

            _context.Remove(contenedor);
            await _context.SaveChangesAsync();
        }


        return Json(new { success = true });
    }

    [HttpPost]
    public IActionResult ConfirmarEdicion(int id, string marchamos, string estadoCarga, string chasis, string transportista, string cliente)
    {
        var contenedor = _context.ContenedoresSinAsignarPatio.FirstOrDefault(c => c.Id == id);

        if (contenedor == null)
            return NotFound();

        contenedor.Marchamos = marchamos;
        contenedor.EstadoCarga = estadoCarga;
        contenedor.Chasis = chasis;
        contenedor.Transportista = transportista;
        contenedor.Cliente = cliente;

        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> BuscarGlobal(string contenedor)
    {
        if (string.IsNullOrWhiteSpace(contenedor))
            return PartialView("_ResultadoBusquedaGlobal", null);

        // SIN ASIGNAR
        var sinAsignar = await _context.ContenedoresSinAsignarPatio
            .FirstOrDefaultAsync(c => c.Contenedor == contenedor);

        if (sinAsignar != null)
            return PartialView("_ResultadoBusquedaGlobal", Mapear(sinAsignar, "SinAsignar"));

        // PATIO 1
        var p1 = await _context.Patio1
            .FirstOrDefaultAsync(c => c.Contenedor == contenedor);

        if (p1 != null)
            return PartialView("_ResultadoBusquedaGlobal", Mapear(p1, "Patio1"));

        // PATIO 2
        var p2 = await _context.Patio2
            .FirstOrDefaultAsync(c => c.Contenedor == contenedor);

        if (p2 != null)
            return PartialView("_ResultadoBusquedaGlobal", Mapear(p2, "Patio2"));

        // ANDEN 2000
        var anden = await _context.Anden2000
            .FirstOrDefaultAsync(c => c.Contenedor == contenedor);

        if (anden != null)
            return PartialView("_ResultadoBusquedaGlobal", Mapear(anden, "Anden2000"));

        // QUIMICOS
        var quimicos = await _context.PatioQuimicos
            .FirstOrDefaultAsync(c => c.Contenedor == contenedor);

        if (quimicos != null)
            return PartialView("_ResultadoBusquedaGlobal", Mapear(quimicos, "PatioQuimicos"));

        return PartialView("_ResultadoBusquedaGlobal", null);
    }

    private BusquedaGlobalContenedorVM Mapear(
    IContenedorInventario c,
    string ubicacion)
    {
        return new BusquedaGlobalContenedorVM
        {
            Id = c.Id,
            Contenedor = c.Contenedor,
            Marchamos = c.Marchamos,
            Tamano = c.Tamano,
            Chasis = c.Chasis,
            Transportista = c.Transportista,
            Cliente = c.Cliente,
            EstadoCarga = c.EstadoCarga,
            UbicacionActual = ubicacion
        };
    }

    private void CopiarDatosBase(IContenedorInventario origen, dynamic destino)
    {
        destino.Contenedor = origen.Contenedor;
        destino.Marchamos = origen.Marchamos;
        destino.Tamano = origen.Tamano;
        destino.Chasis = origen.Chasis;
        destino.Transportista = origen.Transportista;
        destino.Cliente = origen.Cliente;
        destino.EstadoCarga = origen.EstadoCarga;
    }

    // Metodo para empezar a exportar
    private List<ContenedorGeneralVM> ObtenerInventarioGeneral()
    {
        var lista = new List<ContenedorGeneralVM>();

        lista.AddRange(_context.ContenedoresSinAsignarPatio.Select(c => new ContenedorGeneralVM
        {
            Contenedor = c.Contenedor,
            Marchamos = c.Marchamos,
            Tamano = c.Tamano,
            Chasis = c.Chasis,
            Transportista = c.Transportista,
            Cliente = c.Cliente,
            Estado = c.EstadoCarga,
            Ubicacion = "Sin asignar"
        }));

        lista.AddRange(_context.Patio1.Select(c => new ContenedorGeneralVM
        {
            Contenedor = c.Contenedor,
            Marchamos = c.Marchamos,
            Tamano = c.Tamano,
            Chasis = c.Chasis,
            Transportista = c.Transportista,
            Cliente = c.Cliente,
            Estado = c.EstadoCarga,
            Ubicacion = "Patio 1"
        }));

        lista.AddRange(_context.Patio2.Select(c => new ContenedorGeneralVM
        {
            Contenedor = c.Contenedor,
            Marchamos = c.Marchamos,
            Tamano = c.Tamano,
            Chasis = c.Chasis,
            Transportista = c.Transportista,
            Cliente = c.Cliente,
            Estado = c.EstadoCarga,
            Ubicacion = "Patio 2"
        }));

        lista.AddRange(_context.Anden2000.Select(c => new ContenedorGeneralVM
        {
            Contenedor = c.Contenedor,
            Marchamos = c.Marchamos,
            Tamano = c.Tamano,
            Chasis = c.Chasis,
            Transportista = c.Transportista,
            Cliente = c.Cliente,
            Estado = c.EstadoCarga,
            Ubicacion = "Andén 2000"
        }));

        lista.AddRange(_context.PatioQuimicos.Select(c => new ContenedorGeneralVM
        {
            Contenedor = c.Contenedor,
            Marchamos = c.Marchamos,
            Tamano = c.Tamano,
            Chasis = c.Chasis,
            Transportista = c.Transportista,
            Cliente = c.Cliente,
            Estado = c.EstadoCarga,
            Ubicacion = "Patio Químicos"
        }));

        return lista.OrderBy(c => c.Contenedor).ToList();
    }

    [HttpPost]
    public IActionResult ExportarGeneralPDF(string nombre, DateTime fecha, string turno)
    {
        var datos = ObtenerInventarioGeneral();

        int total = datos.Count;
        int cargados = datos.Count(c => c.Estado == "Cargado");
        int vacios = datos.Count(c => c.Estado == "Vacio");

        QuestPDF.Settings.License = LicenseType.Community;

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);

                // ===== ESTILOS LOCALES =====
                static IContainer BoxStyle(IContainer c) =>
                    c.Background(Colors.Grey.Lighten4)
                     .Border(1)
                     .BorderColor(Colors.Grey.Lighten2)
                     .Padding(6)
                     .AlignCenter();

                static IContainer Cell(IContainer c) =>
                    c.BorderBottom(0.5f)
                     .BorderColor(Colors.Grey.Lighten2)
                     .PaddingVertical(4)
                     .PaddingHorizontal(2);

                static IContainer HeaderCell(IContainer c) =>
    c.Background(Colors.Grey.Lighten3)
     .BorderBottom(1)
     .BorderColor(Colors.Grey.Medium)
     .Padding(5)
     .AlignCenter();


                // ===== HEADER =====
                page.Header().Column(col =>
                {
                    col.Item().Text("ALFIPAC – INVENTARIO GENERAL DE CONTENEDORES")
                        .Bold().FontSize(18).FontColor(Colors.Blue.Darken2);

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                    col.Item().Text($"Responsable: {nombre}");
                    col.Item().Text($"Fecha operativa: {fecha:dd/MM/yyyy}");
                    col.Item().Text($"Turno: {turno}");
                    col.Item().Text($"Impreso: {DateTime.Now:dd/MM/yyyy HH:mm}");

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Element(BoxStyle).Column(x =>
                        {
                            x.Item().Text("TOTAL").Bold().FontSize(10);
                            x.Item().Text(total.ToString()).FontSize(16).Bold();
                        });

                        row.RelativeItem().Element(BoxStyle).Column(x =>
                        {
                            x.Item().Text("CARGADOS").Bold().FontSize(10);
                            x.Item().Text(cargados.ToString()).FontSize(16).Bold();
                        });

                        row.RelativeItem().Element(BoxStyle).Column(x =>
                        {
                            x.Item().Text("VACÍOS").Bold().FontSize(10);
                            x.Item().Text(vacios.ToString()).FontSize(16).Bold();
                        });
                    });
                });

                // ===== TABLA =====
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                        c.RelativeColumn(1);
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                        c.RelativeColumn(1);
                        c.RelativeColumn(2);
                    });

                    void Header(string t) =>
                        table.Cell().Element(HeaderCell).Text(t);

                    Header("Contenedor");
                    Header("Marchamos");
                    Header("Tamaño");
                    Header("Chasis");
                    Header("Transportista");
                    Header("Cliente");
                    Header("Estado");
                    Header("Ubicación");

                    foreach (var c in datos)
                    {
                        table.Cell().Element(Cell).Text(c.Contenedor);
                        table.Cell().Element(Cell).Text(c.Marchamos);
                        table.Cell().Element(Cell).Text(c.Tamano);
                        table.Cell().Element(Cell).Text(c.Chasis);
                        table.Cell().Element(Cell).Text(c.Transportista);
                        table.Cell().Element(Cell).Text(c.Cliente);
                        table.Cell().Element(Cell).Text(c.Estado);
                        table.Cell().Element(Cell).Text(c.Ubicacion);
                    }
                });
            });
        }).GeneratePdf();

        return File(pdf, "application/pdf",
            $"Inventario_General_{DateTime.Now:ddMMyyyy_HHmm}.pdf");
    }

    //EXPORTAR A EXCEL
    [HttpPost]
    public IActionResult ExportarGeneralExcel(string nombre, DateTime fecha, string turno)
    {
        var datos = ObtenerInventarioGeneral();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Inventario General");

        ws.Cell("A1").Value = "ALFIPAC – INVENTARIO GENERAL DE CONTENEDORES";
        ws.Range("A1:G1").Merge().Style.Font.SetBold().Font.SetFontSize(16)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Cell("A3").Value = "Responsable:";
        ws.Cell("B3").Value = nombre;
        ws.Cell("D3").Value = "Fecha:";
        ws.Cell("E3").Value = fecha.ToString("dd/MM/yyyy");

        ws.Cell("A4").Value = "Turno:";
        ws.Cell("B4").Value = turno;

        int fila = 6;

        ws.Cell(fila, 1).Value = "Contenedor";
        ws.Cell(fila, 2).Value = "Marchamos";
        ws.Cell(fila, 3).Value = "Tamaño";
        ws.Cell(fila, 4).Value = "Chasis";
        ws.Cell(fila, 5).Value = "Transportista";
        ws.Cell(fila, 6).Value = "Cliente";
        ws.Cell(fila, 7).Value = "Estado";
        ws.Cell(fila, 8).Value = "Ubicación";

        ws.Range(fila, 1, fila, 8).Style.Font.SetBold()
            .Fill.SetBackgroundColor(XLColor.LightGray);

        fila++;

        foreach (var c in datos)
        {
            ws.Cell(fila, 1).Value = c.Contenedor;
            ws.Cell(fila, 2).Value = c.Marchamos;
            ws.Cell(fila, 3).Value = c.Tamano;
            ws.Cell(fila, 4).Value = c.Chasis;
            ws.Cell(fila, 5).Value = c.Transportista;
            ws.Cell(fila, 6).Value = c.Cliente;
            ws.Cell(fila, 7).Value = c.Estado;
            ws.Cell(fila, 8).Value = c.Ubicacion;
            fila++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Inventario_General_{DateTime.Now:ddMMyyyy_HHmm}.xlsx");
    }

    //EXPORTAR A CSV
    [HttpPost]
    public IActionResult ExportarGeneralCSV(string nombre, DateTime fecha, string turno)
    {
        var datos = ObtenerInventarioGeneral();

        int total = datos.Count;
        int cargados = datos.Count(c => c.Estado == "Cargado");
        int vacios = datos.Count(c => c.Estado == "Vacio");

        var sb = new StringBuilder();

        sb.AppendLine("ALFIPAC – INVENTARIO GENERAL");
        sb.AppendLine($"Responsable,{nombre}");
        sb.AppendLine($"Fecha operativa,{fecha:dd/MM/yyyy}");
        sb.AppendLine($"Turno,{turno}");
        sb.AppendLine($"Impreso,{DateTime.Now:dd/MM/yyyy HH:mm}");
        sb.AppendLine("");
        sb.AppendLine($"TOTAL,{total}");
        sb.AppendLine($"CARGADOS,{cargados}");
        sb.AppendLine($"VACÍOS,{vacios}");
        sb.AppendLine("");
        sb.AppendLine("Contenedor,Marchamos,Tamaño,Chasis,Transportista, Cliente,Estado,Ubicación");

        foreach (var c in datos)
        {
            sb.AppendLine($"{c.Contenedor},{c.Marchamos},{c.Tamano},{c.Chasis},{c.Transportista}, {c.Cliente},{c.Estado},{c.Ubicacion}");
        }

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv",
            $"Inventario_General_{DateTime.Now:ddMMyyyy_HHmm}.csv");
    }

    public async Task<IActionResult> InventarioGeneral(
    string? contenedor,
    string? cliente,
    string? transportista,
    string? estado,
    string? tamano,
    string? patio)
    {
        var sinAsignar = await _context.ContenedoresSinAsignarPatio
            .Select(x => new InventarioItemVM
            {
                Id = x.Id,
                Contenedor = x.Contenedor,
                Marchamos = x.Marchamos,
                Tamano = x.Tamano,
                Chasis = x.Chasis,
                Transportista = x.Transportista,
                Cliente = x.Cliente,
                EstadoCarga = x.EstadoCarga,
                Patio = "S/Pat"
            }).ToListAsync();

        var patio1 = await _context.Patio1
            .Select(x => new InventarioItemVM
            {
                Id = x.Id,
                Contenedor = x.Contenedor,
                Marchamos = x.Marchamos,
                Tamano = x.Tamano,
                Chasis = x.Chasis,
                Transportista = x.Transportista,
                Cliente = x.Cliente,
                EstadoCarga = x.EstadoCarga,
                Patio = "P1"
            }).ToListAsync();

        var patio2 = await _context.Patio2
            .Select(x => new InventarioItemVM
            {
                Id = x.Id,
                Contenedor = x.Contenedor,
                Marchamos = x.Marchamos,
                Tamano = x.Tamano,
                Chasis = x.Chasis,
                Transportista = x.Transportista,
                Cliente = x.Cliente,
                EstadoCarga = x.EstadoCarga,
                Patio = "P2"
            }).ToListAsync();

        var anden = await _context.Anden2000
            .Select(x => new InventarioItemVM
            {
                Id = x.Id,
                Contenedor = x.Contenedor,
                Marchamos = x.Marchamos,
                Tamano = x.Tamano,
                Chasis = x.Chasis,
                Transportista = x.Transportista,
                Cliente = x.Cliente,
                EstadoCarga = x.EstadoCarga,
                Patio = "2000"
            }).ToListAsync();

        var quimicos = await _context.PatioQuimicos
            .Select(x => new InventarioItemVM
            {
                Id = x.Id,
                Contenedor = x.Contenedor,
                Marchamos = x.Marchamos,
                Tamano = x.Tamano,
                Chasis = x.Chasis,
                Transportista = x.Transportista,
                Cliente = x.Cliente,
                EstadoCarga = x.EstadoCarga,
                Patio = "AgroQui"
            }).ToListAsync();

        var todos = sinAsignar
            .Concat(patio1)
            .Concat(patio2)
            .Concat(anden)
            .Concat(quimicos)
            .AsQueryable();

        // 🔎 FILTROS SEGUROS
        if (!string.IsNullOrWhiteSpace(contenedor))
            todos = todos.Where(x => x.Contenedor != null &&
                                     x.Contenedor.Contains(contenedor));

        if (!string.IsNullOrWhiteSpace(transportista))
            todos = todos.Where(x => x.Transportista != null &&
                                     x.Transportista.Contains(transportista));

        if (!string.IsNullOrWhiteSpace(cliente))
            todos = todos.Where(x => x.Cliente != null &&
                                     x.Cliente.Contains(cliente));

        if (!string.IsNullOrWhiteSpace(estado))
            todos = todos.Where(x => x.EstadoCarga == estado);

        if (!string.IsNullOrWhiteSpace(tamano))
            todos = todos.Where(x => x.Tamano == tamano);

        if (!string.IsNullOrWhiteSpace(patio))
            todos = todos.Where(x => x.Patio == patio);

        var lista = todos.ToList();

        var vm = new InventarioGeneralVM
        {
            Items = lista,

            Total = lista.Count,
            Cargados = lista.Count(x => x.EstadoCarga == "Cargado"),
            Vacios = lista.Count(x => x.EstadoCarga == "Vacio"),

            SinAsignar = lista.Count(x => x.Patio == "S/Pat"),
            Patio1 = lista.Count(x => x.Patio == "P1"),
            Patio2 = lista.Count(x => x.Patio == "P2"),
            Anden2000 = lista.Count(x => x.Patio == "2000"),
            Quimicos = lista.Count(x => x.Patio == "AgroQui")
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> AgregarManual(AgregarManualVM model)
    {
        if (!ModelState.IsValid)
            return RedirectToAction("InventarioGeneral");

        var ahora = DateTime.Now;

        // =====================
        // INSERTAR EN INVENTARIO
        // =====================
        var inventario = new ContenedorSinAsignarPatio
        {
            Contenedor = model.Contenedor.ToUpper().Trim(),
            Marchamos = model.Marchamos,
            Tamano = model.Tamano,
            Chasis = model.Chasis,
            Transportista = model.Transportista,
            Cliente = model.Cliente,
            EstadoCarga = model.EstadoCarga
        };

        var existe = await _context.ContenedoresSinAsignarPatio
    .AnyAsync(x => x.Contenedor == model.Contenedor);

        if (existe)
        {
            TempData["error"] = "El contenedor ya existe en inventario";
            return RedirectToAction("InventarioGeneral");
        }


        _context.ContenedoresSinAsignarPatio.Add(inventario);

        // =====================
        // INSERTAR EN BITACORA (OPCIONAL)
        // =====================
        if (model.InsertarBitacora)
        {
            var bitacora = new BitacoraIngreso
            {
                Contenedor = inventario.Contenedor,
                FechaHoraIngreso = ahora,
                Cliente = model.Cliente,
                Transportista = model.Transportista
            };

            _context.BitacoraIngresos.Add(bitacora);
        }

        await _context.SaveChangesAsync();

        TempData["ok"] = "Contenedor agregado manualmente";

        return RedirectToAction("InventarioGeneral");
    }

    [HttpPost]
    public async Task<IActionResult> EliminarContenedor(string contenedor)
    {
        if (string.IsNullOrWhiteSpace(contenedor))
            return RedirectToAction("InventarioGeneral");

        contenedor = contenedor.ToUpper().Trim();

        var item = await _context.ContenedoresSinAsignarPatio
            .FirstOrDefaultAsync(x => x.Contenedor == contenedor);

        if (item != null)
        {
            _context.ContenedoresSinAsignarPatio.Remove(item);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("InventarioGeneral");
    }

}
