namespace BitacoraAlfipac.Models.ViewModels;

public class EstadisticasDespachosVM
{
    //ESTADISTICAS REALES    
    public int AnioSeleccionado { get; set; }

    public List<int> AniosDisponibles { get; set; } = new();

    public int TotalHoy { get; set; }

    public int TotalMesActual { get; set; }

    public int TotalAnioSeleccionado { get; set; }

    public double PromedioDiario { get; set; }

    public List<string> Meses { get; set; } = new();

    public List<int> CantidadesPorMes { get; set; } = new();

    public List<ResumenMesVM> ResumenMensual { get; set; } = new();

    //ESTADISTICAS SEMANALES
    public string DiaMasActivo { get; set; } = string.Empty;

    public double PromedioDiaMasActivo { get; set; }

    public string DiaMenosActivo { get; set; } = string.Empty;

    public double PromedioDiaMenosActivo { get; set; }

    public DateTime? FechaRecordOperativo { get; set; }

    public int CantidadRecordOperativo { get; set; }

    public double PromedioDiarioOperativo { get; set; }

    public List<string> DiasSemana { get; set; } = new();

    public List<double> PromediosSemana { get; set; } = new();

    //ESTADISTICAS POR CLIENTE
    public string ClientePrincipal { get; set; } = string.Empty;

    public int CantidadClientePrincipal { get; set; }

    public int TotalClientesActivos { get; set; }

    public double PorcentajeClientePrincipal { get; set; }

    public List<string> ClientesTop { get; set; } = new();

    public List<int> CantidadesClientesTop { get; set; } = new();
}

public class ResumenMesVM
{
    public string Mes { get; set; } = string.Empty;

    public int Cantidad { get; set; }
}