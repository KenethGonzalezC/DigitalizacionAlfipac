using BitacoraAlfipac.Models.Entidades;

namespace BitacoraAlfipac.Data.Seed
{
    public static class UserSeed
    {
        public static void SeedAdminUser(ApplicationDbContext context)
        {
            if (context.Usuarios.Any(u => u.NombreUsuario == "admin"))
                return;

            var admin = new Usuario
            {
                NombreUsuario = "admin",
                Rol = "Administrador",
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Alfipac@123")
            };

            context.Usuarios.Add(admin);
            context.SaveChanges();
        }
    }
}
