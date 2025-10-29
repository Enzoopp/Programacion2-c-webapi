using System.ComponentModel.DataAnnotations;

namespace BankLink.Models
{
    public class Cuenta
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El número de cuenta es requerido")]
        public string NumeroCuenta { get; set; }

        [Required(ErrorMessage = "El tipo de cuenta es requerido")]
        public string TipoCuenta { get; set; } // Ahorro, Corriente

        [Required(ErrorMessage = "El saldo actual es requerido")]
        public decimal SaldoActual { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        public string Estado { get; set; } // Activa, Inactiva

        public DateTime FechaApertura { get; set; } = DateTime.Now;

        // FK hacia Cliente
        [Required(ErrorMessage = "El ID del cliente propietario es requerido")]
        public int IdClientePropietario { get; set; }

        // Propiedad de navegación
        public Cliente ClientePropietario { get; set; }

        // Relación con Movimientos
        public List<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
    }
}
