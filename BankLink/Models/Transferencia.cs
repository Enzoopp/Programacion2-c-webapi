using System.ComponentModel.DataAnnotations;

namespace BankLink.Models
{
    public class Transferencia
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero")]
        public decimal Monto { get; set; }

        public DateTime FechaHora { get; set; } = DateTime.Now;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El estado es requerido")]
        public EstadoTransferencia Estado { get; set; } = EstadoTransferencia.Pendiente;

        [Required(ErrorMessage = "El tipo de transferencia es requerido")]
        public TipoTransferencia Tipo { get; set; }

        // Cuenta de origen (siempre es una cuenta local)
        [Required(ErrorMessage = "La cuenta de origen es requerida")]
        public int CuentaOrigenId { get; set; }
        public virtual Cuenta CuentaOrigen { get; set; }

        // Para transferencias internas: cuenta de destino local
        public int? CuentaDestinoId { get; set; }
        public virtual Cuenta? CuentaDestino { get; set; }

        // Para transferencias externas: banco destino
        public int? BancoExternoId { get; set; }
        public virtual BancoExterno? BancoExterno { get; set; }

        // Para transferencias externas: número de cuenta destino
        [StringLength(50, ErrorMessage = "El número de cuenta destino no puede exceder 50 caracteres")]
        public string? NumeroCuentaDestino { get; set; }

        // ID de referencia externa (para tracking con otros bancos)
        [StringLength(100, ErrorMessage = "La referencia externa no puede exceder 100 caracteres")]
        public string? ReferenciaExterna { get; set; }

        // Navegación: Una transferencia puede generar múltiples movimientos
        public virtual ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
    }

    public enum TipoTransferencia
    {
        Interna = 1,        // Entre cuentas del mismo banco
        Externa = 2         // Hacia otro banco
    }

    public enum EstadoTransferencia
    {
        Pendiente = 1,
        Completada = 2,
        Fallida = 3,
        Cancelada = 4
    }
}