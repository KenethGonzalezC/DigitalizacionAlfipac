using System.ComponentModel.DataAnnotations;

namespace BitacoraAlfipac.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
    }
}
