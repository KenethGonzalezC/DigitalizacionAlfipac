using BitacoraAlfipac.Models.Entidades;

namespace BitacoraAlfipac.Data.Seed
{
    public static class UserSeed
    {
        public static void SeedAdminUser(ApplicationDbContext context)
        {
            // Si ya existe algún usuario, NO hacer nada
            if (context.Usuarios.Any())
                return;

            var admin = new Usuario
            {
                NombreUsuario = "admin",
                Rol = "Administrador",
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123")
            };

            context.Usuarios.Add(admin);
            context.SaveChanges();
        }
    }
}
