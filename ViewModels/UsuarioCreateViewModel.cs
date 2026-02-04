using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BitacoraAlfipac.ViewModels
{
    public class UsuarioCreateViewModel
    {
        [Required]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Rol { get; set; } = string.Empty;

        public List<SelectListItem> RolesDisponibles { get; set; } = new();
    }
}
