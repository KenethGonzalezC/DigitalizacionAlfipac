using System.ComponentModel.DataAnnotations;

namespace BitacoraAlfipac.Models.Entidades
{
    public class ActaPermanencia
    {
        public int Id { get; set; }

        public string Tipo { get; set; } = null!; // ACTA o PAB

        public string Contenedor { get; set; } = null!;
        public string Numero { get; set; } = null!;
        public string? Viaje { get; set; }
        public string? Cliente { get; set; }
        public string Detalles { get; set; } = null!;

        public DateTime FechaCreacion { get; set; }

        // 🔥 PARTE OPERATIVA
        public bool Aplicada { get; set; } = false;
        public bool? AplicadaCorrectamente { get; set; }
        public DateTime? FechaHoraIngresoContenedor { get; set; }
    }

}
