using BitacoraAlfipac.Models.Entidades;

namespace BitacoraAlfipac.Models.ViewModels
{
    public class BitacoraDespachosViewModel
    {
        //necesario para editar
        public int Id { get; set; } 
        // Formulario
        public string Contenedor { get; set; } = "";
        public string? ContenedorReferencia { get; set; }
        public bool EsSalidaEnFurgon { get; set; }

        public string Marchamos { get; set; } = "";
        public DateTime FechaHoraDespacho { get; set; } = DateTime.Now;
        public string Transportista { get; set; } = "";
        public string Informacion { get; set; } = "";
        public string Chofer { get; set; } = "";
        public string PlacaCabezal { get; set; } = "";
        public string Chasis { get; set; } = "";
        public string ViajeDua { get; set; } = "";

        // Filtro fecha
        public DateTime FechaSeleccionada { get; set; } = DateTime.Today;

        // Tabla
        public List<BitacoraDespacho> Despachos { get; set; } = new();
    }
}
