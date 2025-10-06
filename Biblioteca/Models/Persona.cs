using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models
{
    public class Persona
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        public string Apellido { get; set; }

        public DateTime FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El DNI es requerido")]
        [StringLength(8, MinimumLength = 7, ErrorMessage = "El DNI debe tener entre 7 y 8 caracteres")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "El DNI solo puede contener números")]
        public string Dni { get; set; }

        // Propiedades para autenticación
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string NombreUsuario { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string PassHash { get; set; }

        [Required(ErrorMessage = "El rol es requerido")]
        public string Rol { get; set; } = "Usuario"; // Valor por defecto
    }
}

