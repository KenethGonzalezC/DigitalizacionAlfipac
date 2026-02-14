using Microsoft.EntityFrameworkCore;
using System;

namespace BitacoraAlfipac.Models.Entidades
{
    [Index(nameof(Contenedor), IsUnique = true)]
    public class DatosIngresoViaje
    {
        public int Id { get; set; }

        // Información del viaje (la que el usuario escribe)
        public string Viaje { get; set; }
        public string Contenedor { get; set; }
        public string RecintoOrigen { get; set; }
        public DateTime FechaCreacionViaje { get; set; } // NO es la del sistema
        public string Declarante { get; set; }
        public string Transportista { get; set; }
        public string Mercancia { get; set; }

        // Fecha interna automática (NO se muestra en pantalla)
        public DateTime FechaRegistroSistema { get; set; } = DateTime.Now;
    }
}
