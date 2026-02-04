using BitacoraAlfipac.Data;
using BitacoraAlfipac.Models.Entidades;
using BitacoraAlfipac.Services.Interfaces;

namespace BitacoraAlfipac.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public (Usuario? user, string? error) ValidateUser(string username, string password)
        {
            var user = _context.Usuarios
                .FirstOrDefault(u => u.NombreUsuario == username);

            if (user == null)
                return (null, "El usuario no existe.");

            if (!user.Activo)
                return (null, "El usuario se encuentra inactivo. Contacte al administrador.");

            bool passwordOk = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            if (!passwordOk)
                return (null, "La contraseña es incorrecta.");

            return (user, null);
        }
    }
}
