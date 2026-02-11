namespace BitacoraAlfipac.Models.ViewModels
{
    public class BitacoraDiaVM
    {
        public string Contenedor { get; set; }
        public string Marchamos { get; set; }

        public DateTime? HoraEntrada { get; set; }
        public DateTime? HoraSalida { get; set; }

        // 🔑 Esta columna es la que permite el orden consecutivo real
        public DateTime HoraOrden { get; set; }

        public string Transportista { get; set; }
        public string Informacion { get; set; } // Tamaño o Cliente
        public string Chofer { get; set; }
        public string Placa { get; set; }
        public string Chasis { get; set; }
        public string ViajeODua { get; set; }
    }
}
