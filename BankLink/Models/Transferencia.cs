using System.ComponentModel.DataAnnotations;

namespace BankLink.Models
{
    public class Transferencia
    {
        public int Id { get; set; }

        // Cuenta origen (en BankLink)
        [Required(ErrorMessage = "El ID de cuenta origen es requerido")]
        public int IdCuentaOrigen { get; set; }

        // Puede ser null si es una transferencia recibida desde otro banco
        public Cuenta CuentaOrigen { get; set; }

        // Para transferencias hacia otros bancos
        public int? IdBancoDestino { get; set; }
        public BancoExterno BancoDestino { get; set; }

        // Número de cuenta destino (puede ser interno o externo)
        [Required(ErrorMessage = "El número de cuenta destino es requerido")]
        public string NumeroCuentaDestino { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        public decimal Monto { get; set; }

        public DateTime FechaHora { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El estado es requerido")]
        public string Estado { get; set; } // Pendiente, Completada, Fallida

        public string Descripcion { get; set; }

        // Para identificar si es enviada o recibida
        [Required(ErrorMessage = "El tipo de transferencia es requerido")]
        public string TipoTransferencia { get; set; } // Enviada, Recibida
    }
}
