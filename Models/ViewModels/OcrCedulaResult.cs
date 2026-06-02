namespace BitacoraAlfipac.Models.ViewModels
{
    public class OcrCedulaResult
    {
        public bool Success { get; set; }

        public string? Cedula { get; set; }

        public string? NombreCompleto { get; set; }

        public string? TextoDetectado { get; set; }

        public string? Mensaje { get; set; }
    }
}
