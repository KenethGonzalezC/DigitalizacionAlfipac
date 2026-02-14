using BitacoraAlfipac.Models.Entidades;
using System.ComponentModel.DataAnnotations;

namespace BitacoraAlfipac.Models.ViewModels;

public class BitacoraIngresosViewModel
{
    // 🔑 NECESARIO PARA EDITAR
    public int Id { get; set; }
    // ====== FILTRO ======
    public DateTime FechaSeleccionada { get; set; }

    // ====== FORMULARIO INGRESO ======
    [Required]
    public string Contenedor { get; set; } = string.Empty;

    public string? Marchamos { get; set; }

    [Required]
    public DateTime FechaHoraIngreso { get; set; }

    public string? Transportista { get; set; }
    public string? Cliente { get; set; }

    // Texto libre (20, 40, furgón, lowboy, etc.)
    public string? Tamano { get; set; }

    public string? Chofer { get; set; }

    public string? PlacaCabezal { get; set; }

    public string? Chasis { get; set; }

    public string? ViajeDua { get; set; }

    // ====== TABLA ======
    public List<BitacoraIngreso> Ingresos { get; set; } = new();

    //CREACION DE REFRIGERADO DESDE EL INGRESO
    public bool EsRefrigerado { get; set; }

}
