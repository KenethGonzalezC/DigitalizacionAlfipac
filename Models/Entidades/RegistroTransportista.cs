using System.ComponentModel.DataAnnotations;

namespace BitacoraAlfipac.Models.Entidades
{
    public class RegistroTransportista
    {
        public int Id { get; set; }

        [Display(Name = "Fecha Registro")]
        public DateTime FechaRegistro { get; set; }

        private string _placa = string.Empty;

        [Required]
        [StringLength(20)]
        public string Placa
        {
            get => _placa;
            set => _placa = value?.Trim().ToUpperInvariant() ?? string.Empty;
        }

        private string _nombreChofer = string.Empty;

        [Required]
        [StringLength(150)]
        public string NombreChofer
        {
            get => _nombreChofer;
            set => _nombreChofer = value?.Trim().ToUpperInvariant() ?? string.Empty;
        }

        private string _cliente = string.Empty;

        [Required]
        [StringLength(200)]
        public string Cliente
        {
            get => _cliente;
            set => _cliente = value?.Trim().ToUpperInvariant() ?? string.Empty;
        }

        private string? _dua;

        [StringLength(100)]
        public string? DUA
        {
            get => _dua;
            set => _dua = value?.Trim().ToUpperInvariant();
        }

        private string _tipo = string.Empty;

        [Required]
        [StringLength(50)]
        public string Tipo
        {
            get => _tipo;
            set => _tipo = value?.Trim().ToUpperInvariant() ?? string.Empty;
        }

        [Required]
        [StringLength(50)]
        public string Ubicacion { get; set; } = string.Empty;

        public string? RutaFirma { get; set; }

        [StringLength(100)]
        public string? UsuarioRegistro { get; set; }

        public DateTime? FechaHoraIngreso { get; set; }

        public DateTime? FechaHoraSalida { get; set; }

        [StringLength(100)]
        public string? UsuarioSalida { get; set; }
    }
}