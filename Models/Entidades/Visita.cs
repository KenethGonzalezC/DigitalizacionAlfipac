using System.ComponentModel.DataAnnotations;

namespace BitacoraAlfipac.Models.Entidades
{
    public class Visita
    {
        public int Id { get; set; }

        [Required]
        public DateTime FechaIngreso { get; set; }

        [Required]
        [StringLength(200)]
        public string NombreCompletoVisitante { get; set; }

        [Required]
        [StringLength(20)]
        public string NumeroCedula { get; set; }

        [Required]
        [StringLength(150)]
        public string PersonaVisita { get; set; }

        [Required]
        [StringLength(100)]
        public string Departamento { get; set; }

        [Required]
        [StringLength(100)]
        public string Compañía { get; set; }

        [StringLength(100)]
        public string UsuarioRegistro { get; set; }

        public DateTime FechaRegistroSistema { get; set; }

        public string? RutaFirma { get; set; }
    }
}
