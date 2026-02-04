namespace BitacoraAlfipac.Models.Entidades;

public abstract class ContenedorBase
{
    public int Id { get; set; }

    public string Contenedor { get; set; } = null!;
    public string Marchamos { get; set; } = null!;
    public string Tamano { get; set; } = null!;
    public string Chasis { get; set; } = null!;
    public string Transportista { get; set; } = null!;

    public string EstadoCarga { get; set; } = null!; // Vacio / Cargado
    public string Ubicacion { get; set; } = null!;
}
