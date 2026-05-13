namespace BitacoraAlfipac.Helpers
{
    public static class ContenedorHelper
    {
        public static string Normalizar(string? contenedor)
        {
            return (contenedor ?? "")
                .Trim()
                .ToUpper();
        }
    }
}