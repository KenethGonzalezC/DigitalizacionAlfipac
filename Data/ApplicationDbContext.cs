using BitacoraAlfipac.Models.Entidades;
using Microsoft.EntityFrameworkCore;

namespace BitacoraAlfipac.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<BitacoraIngreso> BitacoraIngresos => Set<BitacoraIngreso>();
        public DbSet<BitacoraDespacho> BitacoraDespachos { get; set; }
        public DbSet<HistorialContenedor> HistorialContenedores => Set<HistorialContenedor>();

        public DbSet<ContenedorSinAsignarPatio> ContenedoresSinAsignarPatio { get; set; }
        public DbSet<Patio1> Patio1 { get; set; }
        public DbSet<Patio2> Patio2 { get; set; }
        public DbSet<Anden2000> Anden2000 { get; set; }
        public DbSet<PatioQuimicos> PatioQuimicos { get; set; }

        public DbSet<ContenedorRefrigerado> ContenedoresRefrigerados { get; set; }
        public DbSet<RegistroTemperatura> RegistrosTemperatura { get; set; }
        public DbSet<ContenedorBackupDespacho> ContenedoresBackupDespacho { get; set; }

        public DbSet<ActasPermanencias> ActasPermanencias { get; set; }

        public DbSet<DatosIngresoViaje> DatosIngresosViajes { get; set; }

        public DbSet<PabMercanciaSusceptible> PabMercanciasSusceptibles { get; set; }
        public DbSet<TransportistaAutorizado> TransportistasAutorizados { get; set; }
        public DbSet<DatosDespachoViaje> DatosDespachosViajes { get; set; }

        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<Vacio> Vacios { get; set; }
    }
}
