namespace BankLink.Dtos;

using System.ComponentModel.DataAnnotations;

// DTOs para autenticación
public record LoginDto(
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    string NombreUsuario, 
    
    [Required(ErrorMessage = "La contraseña es requerida")]
    string Contraseña
);

public record RegisterDto(
    [Required(ErrorMessage = "El nombre es requerido")]
    string Nombre,
    
    [Required(ErrorMessage = "El apellido es requerido")]
    string Apellido,
    
    [Required(ErrorMessage = "El DNI es requerido")]
    string Dni,
    
    [Required(ErrorMessage = "La dirección es requerida")]
    string Direccion,
    
    [Required(ErrorMessage = "El teléfono es requerido")]
    string Telefono,
    
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    string Email,
    
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    string NombreUsuario,
    
    [Required(ErrorMessage = "La contraseña es requerida")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    string Contraseña,
    
    string Rol = "Cliente"
);

public record LoginResponseDto(string Token, string Rol, string NombreUsuario);

public record CreateTokenDto(string NombreUsuario, int Id, string Nombre, string Rol);
