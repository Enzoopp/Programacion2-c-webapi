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
    }
}

