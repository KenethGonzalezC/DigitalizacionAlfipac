namespace BitacoraAlfipac.Models.Entidades
{
    public class DatosDespachoViaje
    {
        public int Id { get; set; }

        public string Contenedor { get; set; } = null!;

        public string? Marchamos { get; set; }
        public string? Transportista { get; set; }
        public string? Cliente { get; set; }
        public string? Chasis { get; set; }

        // 🔹 Nuevos datos del despacho
        public string? Chofer { get; set; }
        public string? PlacaCabezal { get; set; }
        public string? ViajeDua { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
