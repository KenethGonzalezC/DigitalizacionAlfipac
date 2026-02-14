namespace BitacoraAlfipac.Models.Entidades;

public class Patio1 : IContenedorInventario
{
    public int Id { get; set; }
    public string Contenedor { get; set; } = null!;
    public string? Marchamos { get; set; }

    public string? Tamano { get; set; }
    public string? Transportista { get; set; }
    public string? Cliente { get; set; }
    public string? Chasis { get; set; }
    public string? EstadoCarga { get; set; }
    public string? Ubicacion { get; set; }
    
}