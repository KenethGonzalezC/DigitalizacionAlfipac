using BitacoraAlfipac.Models.Entidades;

namespace BitacoraAlfipac.Models.ViewModels
{
    public class InventarioIndexVM
    {
        public List<ContenedorSinAsignarPatio> ContenedoresSinAsignar { get; set; }
            = new();

        public BusquedaGlobalContenedorVM? ResultadoBusqueda { get; set; }
    }
}
