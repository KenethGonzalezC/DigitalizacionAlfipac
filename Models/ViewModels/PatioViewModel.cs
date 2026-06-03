using BitacoraAlfipac.Models.Entidades;

namespace BitacoraAlfipac.Models.ViewModels
{
    public class PatioViewModel
    {
        public List<RegistroTransportista> Pendientes { get; set; }
            = new();

        public List<RegistroTransportista> Ingresados { get; set; }
            = new();

        public List<RegistroTransportista> Salidos { get; set; }
            = new();

        public DateTime FechaFiltro { get; set; }
    }
}