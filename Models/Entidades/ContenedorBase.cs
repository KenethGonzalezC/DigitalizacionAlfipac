namespace BitacoraAlfipac.Models.Entidades;

public abstract class ContenedorBase
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

    private string _tamano = string.Empty;
    public string Tamano
    {
        get => _tamano;
        set => _tamano = value?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    private string _chasis = string.Empty;
    public string Chasis
    {
        get => _chasis;
        set => _chasis = value?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    private string _transportista = string.Empty;
    public string Transportista
    {
        get => _transportista;
        set => _transportista = value?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    private string? _cliente;
    public string? Cliente
    {
        get => _cliente;
        set => _cliente = value?.Trim().ToUpperInvariant();
    }

    // Vacio / Cargado
    public string EstadoCarga { get; set; } = null!;

    public string Ubicacion { get; set; } = null!;
}