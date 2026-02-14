namespace BitacoraAlfipac.Models.Entidades
{
    public class ContenedorBackupDespacho
    {
        public int Id { get; set; }

        public string Contenedor { get; set; } = "";
        public string PatioOrigen { get; set; } = "";
        public string Marchamos { get; set; } = "";
        public string Estado { get; set; } = "";   // vacío/cargado
        public string Tamaño { get; set; } = "";
        public string Transportista { get; set; } = "";
        public string? Cliente { get; set; } = "";
        public string Chasis { get; set; } = "";

        public DateTime FechaRespaldo { get; set; } = DateTime.Now;
    }
}
