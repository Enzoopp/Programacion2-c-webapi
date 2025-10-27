using System.ComponentModel.DataAnnotations;

namespace BankLink.Models
{
    public class Movimiento
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es requerido")]
        public TipoMovimiento Tipo { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero")]
        public decimal Monto { get; set; }

        public DateTime FechaHora { get; set; } = DateTime.Now;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        // Clave foránea hacia Cuenta
        [Required(ErrorMessage = "La cuenta es requerida")]
        public int CuentaId { get; set; }

        // Navegación hacia Cuenta
        public virtual Cuenta Cuenta { get; set; }

        // Para transferencias: ID de la transferencia relacionada (opcional)
        public int? TransferenciaId { get; set; }
        public virtual Transferencia? Transferencia { get; set; }
    }

    public enum TipoMovimiento
    {
        Deposito = 1,
        Retiro = 2,
        TransferenciaEnviada = 3,
        TransferenciaRecibida = 4
    }
}