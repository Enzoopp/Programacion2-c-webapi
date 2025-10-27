using System.ComponentModel.DataAnnotations;

namespace BankLink.Dtos
{
    // DTOs para Autenticación
    public record LoginDto(
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        string NombreUsuario,
        
        [Required(ErrorMessage = "La contraseña es requerida")]
        string Contraseña
    )
    {
        public string Username => NombreUsuario;
        public string Password => Contraseña;
    }

    public record CreateTokenDto(string NombreUsuario, int Id, string Nombre, string Rol);

    // DTOs para Operaciones Bancarias
    public record DepositoDto(
        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero")]
        decimal Monto,
        
        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        string Descripcion = ""
    )
    {
        public int CuentaId { get; set; }
    }

    public record RetiroDto(
        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero")]
        decimal Monto,
        
        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        string Descripcion = ""
    )
    {
        public int CuentaId { get; set; }
    }

    // DTOs para Transferencias
    public record TransferenciaInternaDto(
        [Required(ErrorMessage = "La cuenta de destino es requerida")]
        int CuentaDestinoId,
        
        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero")]
        decimal Monto,
        
        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        string Descripcion = ""
    )
    {
        public int CuentaOrigenId { get; set; }
    }

    public record TransferenciaExternaDto(
        [Required(ErrorMessage = "El banco externo es requerido")]
        int BancoExternoId,
        
        [Required(ErrorMessage = "El número de cuenta destino es requerido")]
        [StringLength(50, ErrorMessage = "El número de cuenta no puede exceder 50 caracteres")]
        string NumeroCuentaDestino,
        
        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero")]
        decimal Monto,
        
        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        string Descripcion = ""
    )
    {
        public int CuentaOrigenId { get; set; }
    }

    public record TransferenciaRecibidaDto(
        [Required(ErrorMessage = "El número de cuenta destino es requerido")]
        string NumeroCuentaDestino,
        
        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero")]
        decimal Monto,
        
        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        string Descripcion = "",
        
        [Required(ErrorMessage = "La referencia externa es requerida")]
        string ReferenciaExterna = ""
    );

    // DTOs para respuestas
    public record OperacionBancariaResponseDto(
        bool Exito,
        string Mensaje,
        decimal? NuevoSaldo = null,
        int? MovimientoId = null,
        int? TransferenciaId = null
    );

    public record SaldoCuentaDto(
        int CuentaId,
        string NumeroCuenta,
        decimal Saldo,
        DateTime FechaConsulta
    );

    // DTOs adicionales para autenticación
    public record LoginResponseDto(
        string Token,
        string Username, 
        string Role,
        DateTime ExpiresAt
    );

    public record UsuarioDto(
        string Username,
        string Password,
        string Role
    );
}