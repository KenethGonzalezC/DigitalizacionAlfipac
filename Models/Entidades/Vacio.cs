using System.ComponentModel.DataAnnotations.Schema;

namespace BitacoraAlfipac.Models.Entidades
{
    public class Vacio
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public string Contenedor { get; set; } = "";

        public string Cliente { get; set; } = "";

        public string Transportista { get; set; } = "";

        public string Consecutivo { get; set; } = "";

        public string Usuario { get; set; } = "";

        [NotMapped]
        public string EstadoDespacho { get; set; } = "Pendiente";

        [NotMapped]
        public DateTime? FechaDespacho { get; set; }

        [NotMapped]
        public string DetalleDespacho { get; set; } = "";
    }
}
