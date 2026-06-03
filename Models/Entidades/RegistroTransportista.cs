using System.ComponentModel.DataAnnotations;

namespace BitacoraAlfipac.Models.Entidades
{
    public class RegistroTransportista
    {
        public int Id { get; set; }

        [Display(Name = "Fecha Registro")]
        public DateTime FechaRegistro { get; set; }

        [Required]
        [StringLength(20)]
        public string Placa { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string NombreChofer { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Cliente { get; set; } = string.Empty;

        [StringLength(100)]
        public string? DUA { get; set; }

        [Required]
        [StringLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Ubicacion { get; set; } = string.Empty;

        public string? RutaFirma { get; set; }

        [StringLength(100)]
        public string? UsuarioRegistro { get; set; }

        public DateTime? FechaHoraIngreso { get; set; }

        public DateTime? FechaHoraSalida { get; set; }
        
        [StringLength(100)]
        public string? UsuarioSalida { get; set; }
    }
}
