using System;

namespace BitacoraAlfipac.Models.ViewModels
{
    public class VehiculoVM
    {
        public int Id { get; set; }

        public string Contenedor { get; set; } = null!;

        public string? Marchamos { get; set; }

        public DateTime FechaHoraIngreso { get; set; }

        public string? Transportista { get; set; }

        public string? Cliente { get; set; }

        public string Tamano { get; set; } = "VEHICULO";

        public string? Chofer { get; set; }

        public string? PlacaCabezal { get; set; }

        public string? Chasis { get; set; }

        public string? ViajeDua { get; set; }

        public bool Activo { get; set; }
    }
}