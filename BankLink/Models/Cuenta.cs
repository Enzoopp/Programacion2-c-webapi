using System.ComponentModel.DataAnnotations;

namespace BankLink.Models
{
    public class Cuenta
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El número de cuenta es requerido")]
        [StringLength(20, ErrorMessage = "El número de cuenta no puede exceder 20 caracteres")]
        public string NumeroCuenta { get; set; }

        [Required(ErrorMessage = "El tipo de cuenta es requerido")]
        public TipoCuenta Tipo { get; set; }

        [Required(ErrorMessage = "El saldo es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El saldo no puede ser negativo")]
        public decimal Saldo { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        public EstadoCuenta Estado { get; set; } = EstadoCuenta.Activa;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Clave foránea hacia Cliente
        [Required(ErrorMessage = "El cliente propietario es requerido")]
        public int ClienteId { get; set; }

        // Navegación hacia Cliente
        public virtual Cliente Cliente { get; set; }

        // Navegación: Una cuenta puede tener múltiples movimientos
        public virtual ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
    }

    public enum TipoCuenta
    {
        Ahorro = 1,
        Corriente = 2
    }

    public enum EstadoCuenta
    {
        Activa = 1,
        Inactiva = 2,
        Suspendida = 3
    }
}