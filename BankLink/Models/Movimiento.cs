using System.ComponentModel.DataAnnotations;

namespace BankLink.Models
{
    public class Movimiento
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El ID de cuenta es requerido")]
        public int IdCuenta { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es requerido")]
        public string TipoMovimiento { get; set; } // Depósito, Retiro, Transferencia Enviada, Transferencia Recibida

        [Required(ErrorMessage = "El monto es requerido")]
        public decimal Monto { get; set; }

        public DateTime FechaHora { get; set; } = DateTime.Now;

        public string Descripcion { get; set; }

        // Propiedad de navegación
        public Cuenta Cuenta { get; set; }
    }
}
