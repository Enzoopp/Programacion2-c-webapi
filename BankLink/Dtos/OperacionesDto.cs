using System.ComponentModel.DataAnnotations;

namespace BankLink.Dtos
{
    // DTO para realizar un depósito
    public class DepositoDto
    {
        [Required(ErrorMessage = "El ID de la cuenta es requerido")]
        public int IdCuenta { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        public string Descripcion { get; set; }
    }

    // DTO para realizar un retiro
    public class RetiroDto
    {
        [Required(ErrorMessage = "El ID de la cuenta es requerido")]
        public int IdCuenta { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        public string Descripcion { get; set; }
    }

    // DTO para realizar una transferencia
    public class TransferenciaDto
    {
        [Required(ErrorMessage = "El ID de cuenta origen es requerido")]
        public int IdCuentaOrigen { get; set; }

        [Required(ErrorMessage = "El número de cuenta destino es requerido")]
        public string NumeroCuentaDestino { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        public string Descripcion { get; set; }

        // Opcional: para transferencias a bancos externos
        public int? IdBancoDestino { get; set; }
    }

    // DTO para recibir una transferencia desde un banco externo
    public class TransferenciaRecibidaDto
    {
        [Required(ErrorMessage = "El número de cuenta destino es requerido")]
        public string NumeroCuentaDestino { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El banco origen es requerido")]
        public string BancoOrigen { get; set; }

        public string NumeroCuentaOrigen { get; set; }

        public string Descripcion { get; set; }
    }

    // DTO para crear una cuenta
    public class CrearCuentaDto
    {
        [Required(ErrorMessage = "El tipo de cuenta es requerido")]
        public string TipoCuenta { get; set; } // Ahorro, Corriente

        [Required(ErrorMessage = "El ID del cliente propietario es requerido")]
        public int IdClientePropietario { get; set; }

        public decimal SaldoInicial { get; set; } = 0;
    }
}
