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

        [NotMapped]
        public string? ChoferDespacho { get; set; }

        [NotMapped]
        public string? PlacaDespacho { get; set; }

        [NotMapped]
        public string? ChasisDespacho { get; set; }

        [NotMapped]
        public string? ViajeDuaDespacho { get; set; }

        [NotMapped]
        public DateTime? FechaDespachoReal { get; set; }

        [NotMapped]
        public string? FechasDespachoTexto { get; set; }

        [NotMapped]
        public string? ChoferesDespachoTexto { get; set; }

        [NotMapped]
        public string? PlacasDespachoTexto { get; set; }

        [NotMapped]
        public string? ContenedoresPendientes { get; set; }
    }
}
