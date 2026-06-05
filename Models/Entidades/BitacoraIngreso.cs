namespace BitacoraAlfipac.Models.Entidades;

public class BitacoraIngreso
{
    public int Id { get; set; }

    private string _contenedor = string.Empty;
    public string Contenedor
    {
        get => _contenedor;
        set => _contenedor = value?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    private string _marchamos = string.Empty;
    public string Marchamos
    {
        get => _marchamos;
        set => _marchamos = value?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    public DateTime FechaHoraIngreso { get; set; }

    private string _transportista = string.Empty;
    public string Transportista
    {
        get => _transportista;
        set => _transportista = value?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    private string _cliente = string.Empty;
    public string Cliente
    {
        get => _cliente;
        set => _cliente = value?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    private string _tamano = string.Empty;
    public string Tamaño
    {
        get => _tamano;
        set => _tamano = value?.Trim().ToUpperInvariant() ?? string.Empty;
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
}