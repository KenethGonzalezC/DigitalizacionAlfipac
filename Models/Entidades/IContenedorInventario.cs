namespace BitacoraAlfipac.Models.Entidades
{
    public interface IContenedorInventario
    {
        int Id { get; set; }
        string? Contenedor { get; set; }
        string? Marchamos { get; set; }
        string? Tamano { get; set; }
        string? Chasis { get; set; }
        string? Transportista { get; set; }
        string? EstadoCarga { get; set; }
    }
}
