namespace Biblioteca.Models
{
    public class Libro
    {
        public int Id { get; set; }
        public int IdAutor { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Genero { get; set; }
        public string ISBN { get; set; }
        public int Fecha_Publicacion { get; set; }
        public int Fecha_Edicion { get; set; }
        public string Edicion { get; set; }

        public Autor Autor { get; set; }
    }
}
