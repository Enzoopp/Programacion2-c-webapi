using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models
{
    public class Autor
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; }
        
        [Required(ErrorMessage = "La nacionalidad es requerida")]
        public string Nacionalidad { get; set; }
        
        public string Descripcion { get; set; }
        public List<Libro> Libros { get; set; }
    }
}
