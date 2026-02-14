using System.ComponentModel.DataAnnotations;

namespace BitacoraAlfipac.Models.Entidades
{
    public class TransportistaAutorizado
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(300)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(50)]
        public string CedulaJuridica { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Codigo { get; set; } = string.Empty;
    }
}
