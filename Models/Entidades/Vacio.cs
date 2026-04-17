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
    }
}
