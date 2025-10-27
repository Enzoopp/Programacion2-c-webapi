using System.ComponentModel.DataAnnotations;

namespace BankLink.Models
{
    public class BancoExterno
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del banco es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El código de identificación es requerido")]
        [StringLength(10, ErrorMessage = "El código no puede exceder 10 caracteres")]
        public string CodigoIdentificacion { get; set; }

        [Required(ErrorMessage = "La URL base es requerida")]
        [Url(ErrorMessage = "Formato de URL inválido")]
        [StringLength(500, ErrorMessage = "La URL no puede exceder 500 caracteres")]
        public string UrlBase { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Navegación: Un banco externo puede recibir múltiples transferencias
        public virtual ICollection<Transferencia> TransferenciasRecibidas { get; set; } = new List<Transferencia>();
    }
}