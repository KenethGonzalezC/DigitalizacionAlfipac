namespace BitacoraAlfipac.Models.Entidades;

public class BitacoraDespacho
{
    public int Id { get; set; }

    // Contenedor que sale físicamente
    private string _contenedor = string.Empty;
    public string Contenedor
    {
        get => _contenedor;
        set => _contenedor = value?.Trim().ToUpperInvariant() ?? string.Empty;
    }
        
    private string? _contenedorReferencia;
    public string? ContenedorReferencia
    {
        get => _contenedorReferencia;
        set => _contenedorReferencia = value?.Trim().ToUpperInvariant();
    }

    //SI EL CONTENEDOR NO SE ENCUENTRA FISCAMENTE EN EL ALMACEN, SE REGISTRARÁ COMO SALIDA EN FURGON
    public bool EsSalidaEnFurgon { get; set; } = false;

    private string _marchamos = string.Empty;
    public string Marchamos
    {
        get => _marchamos;
        set => _marchamos = value?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    public DateTime FechaHoraDespacho { get; set; }

    private string _transportista = string.Empty;
    public string Transportista
    {
        get => _transportista;
        set => _transportista = value?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    // Cliente destino
    private string _informacion = string.Empty;
    public string Informacion
    {
        get => _informacion;
        set => _informacion = value?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    private string _chofer = string.Empty;
    public string Chofer
    {
        get => _chofer;
        set => _chofer = value?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    private string _placaCabezal = string.Empty;
    public string PlacaCabezal
    {
        get => _placaCabezal;
        set => _placaCabezal = value?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    private string _chasis = string.Empty;
    public string Chasis
    {
        get => _chasis;
        set => _chasis = value?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    private string _viajeDua = string.Empty;
    public string ViajeDua
    {
        get => _viajeDua;
        set => _viajeDua = value?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    // Contenedor de salida
    public bool GuardarContenedorSalida { get; set; }

    // Relación opcional con RegistroTransportista
    public int? RegistroTransportistaId { get; set; }
}