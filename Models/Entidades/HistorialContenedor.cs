namespace BitacoraAlfipac.Models.Entidades;

public class HistorialContenedor
{
    public int Id { get; set; }
    public string Contenedor { get; set; } = null!;
    public DateTime? FechaHoraIngreso { get; set; }
    public DateTime? FechaHoraSalida { get; set; }
}
