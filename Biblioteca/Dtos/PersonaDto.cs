namespace Bibliote.Dto;

public record RegistrarPersonaDto(string Nombre, string NombreUsuario, string Email, string Contrasenia, string Apellido, DateTime FechaNacimiento, string DNI, string Rol);
public record ActualizarPersonaDto(string Nombre, string NombreUsuario, string Apellido, DateTime FechaNacimiento, string DNI, string Rol);
public record PersonaDto(string Nombre, string NombreUsuario, string Apellido, DateTime FechaNacimiento, string DNI);