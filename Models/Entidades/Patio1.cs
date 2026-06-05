namespace BitacoraAlfipac.Models.Entidades;

public class Patio1 : IContenedorInventario
{
    public int Id { get; set; }

    private string _contenedor = string.Empty;
    public string Contenedor
    {
        get => _contenedor;
        set => _contenedor = value?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    private string? _marchamos;
    public string? Marchamos
    {
        get => _marchamos;
        set => _marchamos = value?.Trim().ToUpperInvariant();
    }

    private string? _tamano;
    public string? Tamano
    {
        get => _tamano;
        set => _tamano = value?.Trim().ToUpperInvariant();
    }

    private string? _transportista;
    public string? Transportista
    {
        get => _transportista;
        set => _transportista = value?.Trim().ToUpperInvariant();
    }

    private string? _cliente;
    public string? Cliente
    {
        get => _cliente;
        set => _cliente = value?.Trim().ToUpperInvariant();
    }

    private string? _chasis;
    public string? Chasis
    {
        get => _chasis;
        set => _chasis = value?.Trim().ToUpperInvariant();
    }

    public string? EstadoCarga { get; set; }

    public string? Ubicacion { get; set; }

    public int Orden { get; set; }
}