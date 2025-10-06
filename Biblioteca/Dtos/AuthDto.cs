namespace Biblioteca.Dtos;

using System.ComponentModel.DataAnnotations;

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
    
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    string NombreUsuario,
    
    [Required(ErrorMessage = "La contraseña es requerida")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    string Contraseña,
    
    [Required(ErrorMessage = "El DNI es requerido")]
    string Dni,
    
    DateTime FechaNacimiento,
    string Rol = "Usuario"
);

public record LoginResponseDto(string Token, string Rol, string NombreUsuario);

public record CreateTokenDto(string NombreUsuario, int Id, string Nombre, string Rol);