namespace BitacoraAlfipac.Models.ViewModels
{
    public class InventarioItemVM
    {
        public int Id { get; set; }
        public string? Contenedor { get; set; }
        public string? Marchamos { get; set; }
        public string? Tamano { get; set; }
        public string? Chasis { get; set; }
        public string? Transportista { get; set; }
        public string? Cliente { get; set; }
        public string? EstadoCarga { get; set; }
        public string? Patio { get; set; }
    }

    public class InventarioGeneralVM
    {
        public List<InventarioItemVM> Items { get; set; } = new();

        public int Total { get; set; }
        public int Cargados { get; set; }
        public int Vacios { get; set; }

        public int SinAsignar { get; set; }
        public int Patio1 { get; set; }
        public int Patio2 { get; set; }
        public int Anden2000 { get; set; }
        public int Quimicos { get; set; }
    }
}
