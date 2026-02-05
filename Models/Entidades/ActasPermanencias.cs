using System.ComponentModel.DataAnnotations;

namespace BitacoraAlfipac.Models.Entidades
{
    public class ActasPermanencias
    {
        public int Id { get; set; }

        [Required]
        public string Tipo { get; set; } = null!; // ACTA o PAB

        [Required]
        public string Contenedor { get; set; } = null!;

        [Required]
        public string Numero { get; set; } = null!;

        [Required]
        public string Detalle { get; set; } = null!;

        public string? Viaje { get; set; }
        public string? Cliente { get; set; }

        // 🔹 OPERATIVO
        public DateTime? FechaHoraIngresoContenedor { get; set; }

        // 🔹 Resultado operativo (solo aplica si ingresó)
        public bool AplicadoCorrectamente { get; set; } = false;
    }
}
