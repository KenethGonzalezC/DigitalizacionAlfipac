using System.ComponentModel.DataAnnotations;

namespace BitacoraAlfipac.Models.Entidades
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string NombreUsuario { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string Rol { get; set; } = null!;
        // Valores esperados: "Administrador", "Usuario"

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
