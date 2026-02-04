using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitacoraAlfipac.Models.Entidades;

public class RegistroTemperatura
{
    public int Id { get; set; }

    [Required]
    public DateTime FechaHora { get; set; }

    [Required]
    public decimal Temperatura { get; set; }

    public string? Observacion { get; set; }

    // FK
    public int ContenedorRefrigeradoId { get; set; }

    [ForeignKey(nameof(ContenedorRefrigeradoId))]
    public ContenedorRefrigerado ContenedorRefrigerado { get; set; } = null!;
}
