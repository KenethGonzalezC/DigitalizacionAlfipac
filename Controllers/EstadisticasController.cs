using BitacoraAlfipac.Data;
using BitacoraAlfipac.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BitacoraAlfipac.Controllers
{
    public class EstadisticasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EstadisticasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Despachos(int? anio, string tipo = "todos")
        {
            //ESTADISTICA GENERAL REAL

            var hoy = DateTime.Today;

            var anioSeleccionado =
                anio ?? hoy.Year;

            var inicioMes =
                new DateTime(hoy.Year, hoy.Month, 1);

            var despachos =
                await _context.BitacoraDespachos
                    .AsNoTracking()
                    .ToListAsync();

            var despachosAnio =
                despachos
                    .Where(x =>
                        x.FechaHoraDespacho.Year == anioSeleccionado)
                    .ToList();

            switch (tipo?.ToLower())
            {
                case "comercial":

                    despachosAnio =
                        despachosAnio
                            .Where(x =>
                                !string.IsNullOrWhiteSpace(x.ViajeDua) &&
                                !x.ViajeDua
                                    .Trim()
                                    .ToUpper()
                                    .StartsWith("CCV"))
                            .ToList();

                    break;

                case "vacios":

                    despachosAnio =
                        despachosAnio
                            .Where(x =>
                                !string.IsNullOrWhiteSpace(x.ViajeDua) &&
                                x.ViajeDua
                                    .Trim()
                                    .ToUpper()
                                    .StartsWith("CCV"))
                            .ToList();

                    break;

                default:
                    break;
            }

            string[] nombresMeses =
            {
        "",
        "Enero",
        "Febrero",
        "Marzo",
        "Abril",
        "Mayo",
        "Junio",
        "Julio",
        "Agosto",
        "Septiembre",
        "Octubre",
        "Noviembre",
        "Diciembre"
    };

            var resumenMensual =
                Enumerable.Range(1, 12)
                .Select(mes => new ResumenMesVM
                {
                    Mes = nombresMeses[mes],

                    Cantidad = despachosAnio.Count(x =>
                        x.FechaHoraDespacho.Month == mes)
                })
                .ToList();

            var vm =
                new EstadisticasDespachosVM
                {
                    TipoSeleccionado = tipo,

                    AnioSeleccionado = anioSeleccionado,

                    AniosDisponibles =
                        despachos
                            .Select(x => x.FechaHoraDespacho.Year)
                            .Distinct()
                            .OrderByDescending(x => x)
                            .ToList(),

                    TotalHoy =
                        despachos.Count(x =>
                            x.FechaHoraDespacho.Date == hoy),

                    TotalMesActual =
                        despachos.Count(x =>
                            x.FechaHoraDespacho.Year == hoy.Year &&
                            x.FechaHoraDespacho.Month == hoy.Month),

                    TotalAnioSeleccionado =
                        despachosAnio.Count,

                    PromedioDiario =
                        despachosAnio.Any()
                            ? Math.Round(
                                despachosAnio.Count /
                                365.0,
                                1)
                            : 0,

                    ResumenMensual = resumenMensual,

                    Meses =
                        resumenMensual
                            .Select(x => x.Mes)
                            .ToList(),

                    CantidadesPorMes =
                        resumenMensual
                            .Select(x => x.Cantidad)
                            .ToList()
                };

            // ============================================
            // ESTADÍSTICAS OPERATIVAS
            // (LUNES A VIERNES)
            // ============================================

            var despachosOperativos =
                despachosAnio
                    .Where(x =>
                        x.FechaHoraDespacho.DayOfWeek != DayOfWeek.Saturday &&
                        x.FechaHoraDespacho.DayOfWeek != DayOfWeek.Sunday)
                    .ToList();

            var movimientosPorDia =
                despachosOperativos
                    .GroupBy(x => x.FechaHoraDespacho.Date)
                    .Select(g => new
                    {
                        Fecha = g.Key,
                        Cantidad = g.Count()
                    })
                    .ToList();

            if (movimientosPorDia.Any())
            {
                string ObtenerNombreDia(DayOfWeek dia)
                {
                    return dia switch
                    {
                        DayOfWeek.Monday => "Lunes",
                        DayOfWeek.Tuesday => "Martes",
                        DayOfWeek.Wednesday => "Miércoles",
                        DayOfWeek.Thursday => "Jueves",
                        DayOfWeek.Friday => "Viernes",
                        _ => dia.ToString()
                    };
                }

                var estadisticasSemana =
                    movimientosPorDia
                        .GroupBy(x => x.Fecha.DayOfWeek)
                        .Select(g => new
                        {
                            Dia = g.Key,

                            Promedio =
                                Math.Round(
                                    g.Average(x => x.Cantidad),
                                    1)
                        })
                        .ToList();

                var diaMasActivo =
                    estadisticasSemana
                        .OrderByDescending(x => x.Promedio)
                        .First();

                var diaMenosActivo =
                    estadisticasSemana
                        .OrderBy(x => x.Promedio)
                        .First();

                var recordOperativo =
                    movimientosPorDia
                        .OrderByDescending(x => x.Cantidad)
                        .First();

                vm.DiaMasActivo =
                    ObtenerNombreDia(diaMasActivo.Dia);

                vm.PromedioDiaMasActivo =
                    diaMasActivo.Promedio;

                vm.DiaMenosActivo =
                    ObtenerNombreDia(diaMenosActivo.Dia);

                vm.PromedioDiaMenosActivo =
                    diaMenosActivo.Promedio;

                vm.FechaRecordOperativo =
                    recordOperativo.Fecha;

                vm.CantidadRecordOperativo =
                    recordOperativo.Cantidad;

                vm.PromedioDiarioOperativo =
                    Math.Round(
                        movimientosPorDia.Average(x => x.Cantidad),
                        1);

                DayOfWeek[] ordenDias =
                {
                    DayOfWeek.Monday,
                    DayOfWeek.Tuesday,
                    DayOfWeek.Wednesday,
                    DayOfWeek.Thursday,
                    DayOfWeek.Friday
                };

                vm.DiasSemana =
                    ordenDias
                        .Select(ObtenerNombreDia)
                        .ToList();

                vm.PromediosSemana =
                    ordenDias
                        .Select(dia =>
                            estadisticasSemana
                                .FirstOrDefault(x => x.Dia == dia)
                                ?.Promedio ?? 0)
                        .ToList();
            }

            // ============================================
            // ESTADÍSTICAS POR CLIENTE (SIN CCV)
            // ============================================

            var despachosClientes =
                despachosAnio
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.Informacion) &&
                        !string.IsNullOrWhiteSpace(x.ViajeDua) &&
                        !x.ViajeDua
                            .Trim()
                            .ToUpper()
                            .StartsWith("CCV"))
                    .ToList();

            var clientes =
                despachosClientes
                    .GroupBy(x =>
                        x.Informacion
                            .Trim()
                            .ToUpper())
                    .Select(g => new
                    {
                        Cliente = g.Key,
                        Cantidad = g.Count()
                    })
                    .OrderByDescending(x => x.Cantidad)
                    .ToList();

            vm.TotalClientesActivos =
                clientes.Count;

            var clientePrincipal =
                clientes.FirstOrDefault();

            if (clientePrincipal != null)
            {
                vm.ClientePrincipal =
                    clientePrincipal.Cliente;

                vm.CantidadClientePrincipal =
                    clientePrincipal.Cantidad;

                vm.PorcentajeClientePrincipal =
                    despachosClientes.Any()
                        ? Math.Round(
                            (clientePrincipal.Cantidad * 100.0)
                            / despachosClientes.Count,
                            1)
                        : 0;
            }

            var top10Clientes =
                clientes
                    .Take(10)
                    .ToList();

            vm.ClientesTop =
                top10Clientes
                    .Select(x => x.Cliente)
                    .ToList();

            vm.CantidadesClientesTop =
                top10Clientes
                    .Select(x => x.Cantidad)
                    .ToList();

            // ============================================
            // ESTADÍSTICAS SIN VACÍOS
            // ============================================

            var despachosComerciales =
                despachosAnio
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.ViajeDua) &&
                        !x.ViajeDua
                            .Trim()
                            .ToUpper()
                            .StartsWith("CCV"))
                    .ToList();

            var despachosVacios =
                despachosAnio
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.ViajeDua) &&
                        x.ViajeDua
                            .Trim()
                            .ToUpper()
                            .StartsWith("CCV"))
                    .ToList();

            vm.TotalDespachosComerciales =
                despachosComerciales.Count;

            vm.TotalDespachosVacios =
                despachosVacios.Count;

            vm.PorcentajeVacios =
                vm.TotalAnioSeleccionado > 0
                    ? Math.Round(
                        (vm.TotalDespachosVacios * 100.0)
                        / vm.TotalAnioSeleccionado,
                        1)
                    : 0;

            //comercial sin vacios de lunes a viernes
            var comercialesOperativos =
            despachosComerciales
                .Where(x =>
                    x.FechaHoraDespacho.DayOfWeek != DayOfWeek.Saturday &&
                    x.FechaHoraDespacho.DayOfWeek != DayOfWeek.Sunday)
                .ToList();

            var diasComerciales =
                comercialesOperativos
                    .GroupBy(x => x.FechaHoraDespacho.Date)
                    .Count();

            vm.PromedioDiarioComercial =
                diasComerciales > 0
                    ? Math.Round(
                        comercialesOperativos.Count / (double)diasComerciales,
                        1)
                    : 0;

            var resumenMensualComercial =
            Enumerable.Range(1, 12)
                .Select(mes => new ResumenMesVM
                {
                    Mes = nombresMeses[mes],

                    Cantidad =
                        despachosComerciales.Count(x =>
                            x.FechaHoraDespacho.Month == mes)
                })
                .ToList();

                    vm.ResumenMensualComercial =
                        resumenMensualComercial;

                    vm.MesesComerciales =
                        resumenMensualComercial
                            .Select(x => x.Mes)
                            .ToList();

                    vm.CantidadesComerciales =
                        resumenMensualComercial
                            .Select(x => x.Cantidad)
                            .ToList();


            return View(vm);            
        }

        public IActionResult Ingresos()
        {
            return View();
        }

        public IActionResult Vacios()
        {
            return View();
        }

    }
}
