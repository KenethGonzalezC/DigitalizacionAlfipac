using System;

namespace BitacoraAlfipac.Models.Entidades
{
    public class Vehiculo
    {
        public int Id { get; set; }

        // 📦 MISMA LÓGICA QUE CONTENEDOR
        public string Contenedor { get; set; } = null!; // Modelo vehículo

        public string? Marchamos { get; set; } // Marchamo o VIN completo o S/M

        public DateTime FechaHoraIngreso { get; set; }

        public string? Transportista { get; set; }

        public string? Cliente { get; set; }

        public string Tamano { get; set; } = "VEHICULO"; // 🔑 IDENTIFICADOR

        public string? Chofer { get; set; }

        public string? PlacaCabezal { get; set; } // Placa o S/P

        public string? Chasis { get; set; } // 🔑 VIN (VIN + últimos 6)

        public string? ViajeDua { get; set; }

        // 🔥 CONTROL (MUY IMPORTANTE)
        public bool Activo { get; set; } = true; // para saber si ya fue despachado
    }
}