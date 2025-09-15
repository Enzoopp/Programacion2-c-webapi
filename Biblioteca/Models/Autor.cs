using System.Collections.Generic;

namespace Biblioteca.Models
{
    public class Autor
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Nacionalidad { get; set; }
        public string Descripcion { get; set; }
        public List<Libro> Libros { get; set; }
    }
}
