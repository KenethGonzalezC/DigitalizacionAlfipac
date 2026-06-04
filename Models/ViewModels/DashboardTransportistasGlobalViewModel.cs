using BitacoraAlfipac.Models.Entidades;

namespace BitacoraAlfipac.Models.ViewModels
{
    public class DashboardTransportistasGlobalViewModel
    {
        // TODOS los registros del día (sin importar ubicación)
        public List<RegistroTransportista> Registrados { get; set; } = new();

        // Ingresados (FechaHoraIngreso != null y FechaHoraSalida == null)
        public List<RegistroTransportista> Ingresados { get; set; } = new();

        // Salidos (FechaHoraSalida != null)
        public List<RegistroTransportista> Salidos { get; set; } = new();

        public DateTime FechaFiltro { get; set; }
    }
}