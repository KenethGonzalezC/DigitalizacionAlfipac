using System.ComponentModel.DataAnnotations;

namespace BitacoraAlfipac.Models.Entidades
{
    public class PabMercanciaSusceptible
    {
        public int Id { get; set; }

        public int Codigo { get; set; }

        [Required]
        [MaxLength(300)]
        public string Descripcion { get; set; } = string.Empty;
    }
}
