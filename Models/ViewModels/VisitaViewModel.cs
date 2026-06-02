using System.ComponentModel.DataAnnotations;

namespace BitacoraAlfipac.Models.ViewModels
{
    public class VisitaViewModel
    {
        public string NumeroCedula { get; set; }

        public string NombreCompletoVisitante { get; set; }

        [Required]
        public string PersonaVisita { get; set; }

        [Required]
        public string Departamento { get; set; }
    }
}
