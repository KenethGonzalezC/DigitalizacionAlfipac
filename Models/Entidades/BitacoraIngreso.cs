namespace BitacoraAlfipac.Models.Entidades;

public class BitacoraIngreso
{
    public int Id { get; set; }
    public string Contenedor { get; set; } = null!;
    public string Marchamos { get; set; } = null!;
    public DateTime FechaHoraIngreso { get; set; }
    public string Transportista { get; set; } = null!;
    public string Cliente { get; set; } = null!;   
    public string Tamaño { get; set; } = null!;
    public string Chofer { get; set; } = null!;
    public string PlacaCabezal { get; set; } = null!;
    public string Chasis { get; set; } = null!;
    public string ViajeDua { get; set; } = null!;
}
