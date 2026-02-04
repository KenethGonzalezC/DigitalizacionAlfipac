namespace BitacoraAlfipac.Models.Entidades;

public class BitacoraDespacho
{
    public int Id { get; set; }

    // Contenedor que sale físicamente
    public string Contenedor { get; set; } = null!;

    // Si la mercadería salió en furgón
    public string? ContenedorReferencia { get; set; }

    public bool EsSalidaEnFurgon { get; set; } = false;

    public string Marchamos { get; set; } = null!;
    public DateTime FechaHoraDespacho { get; set; }

    public string Transportista { get; set; } = null!;
    public string Informacion { get; set; } = null!; // Cliente destino
    public string Chofer { get; set; } = null!;
    public string PlacaCabezal { get; set; } = null!;
    public string Chasis { get; set; } = null!;
    public string ViajeDua { get; set; } = null!;
}
    