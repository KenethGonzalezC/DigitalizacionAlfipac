using System.ComponentModel.DataAnnotations;

namespace BitacoraAlfipac.Models.Entidades;

public class ContenedorRefrigerado
{
    public int Id { get; set; }

    [Required]
    public string Contenedor { get; set; } = null!;

    public DateTime? FechaHoraIngreso { get; set; }

    public DateTime? FechaHoraConexion { get; set; }

    public decimal? SetPoint { get; set; }

    public DateTime? FechaHoraDespacho { get; set; }

    public DateTime? FechaHoraDesconexion { get; set; }

    // 🔑 CLAVE: relación con temperaturas
    public ICollection<RegistroTemperatura> RegistrosTemperatura { get; set; }
        = new List<RegistroTemperatura>();
}
