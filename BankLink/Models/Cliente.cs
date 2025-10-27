using System.ComponentModel.DataAnnotations;

namespace BankLink.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El DNI/Identificación es requerido")]
        [StringLength(20, ErrorMessage = "El DNI no puede exceder 20 caracteres")]
        public string Identificacion { get; set; }

        [Required(ErrorMessage = "La dirección es requerida")]
        [StringLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres")]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(255, ErrorMessage = "El email no puede exceder 255 caracteres")]
        public string Email { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Navegación: Un cliente puede tener múltiples cuentas
        public virtual ICollection<Cuenta> Cuentas { get; set; } = new List<Cuenta>();
    }
}