using System.ComponentModel.DataAnnotations;

namespace BankLink.Models
{
    public class BancoExterno
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del banco es requerido")]
        public string NombreBanco { get; set; }

        [Required(ErrorMessage = "El código de identificación es requerido")]
        public string CodigoIdentificacion { get; set; }

        [Required(ErrorMessage = "La URL base de la API es requerida")]
        [Url(ErrorMessage = "La URL no tiene un formato válido")]
        public string UrlApiBase { get; set; }

        public string Descripcion { get; set; }

        public bool Activo { get; set; } = true;
    }
}
