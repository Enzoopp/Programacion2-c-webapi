using Biblioteca.Models;
using Biblioteca.interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Biblioteca.Services
{
    public class LibroMemService : ILibroService
    {
        private static List<Libro> _libros = new List<Libro>
        {
            new Libro { Id = 1, IdAutor = 1, Titulo = "Cien Años de Soledad", Descripcion = "La novela narra la historia de la familia Buendía a lo largo de siete generaciones en el pueblo ficticio de Macondo.", Genero = "Realismo Mágico", ISBN = "978-0307474728", Fecha_Publicacion = 1967, Fecha_Edicion = 2003, Edicion = "Edición Conmemorativa" },
            new Libro { Id = 2, IdAutor = 2, Titulo = "Rayuela", Descripcion = "La novela es conocida por su estructura, que permite al lector elegir entre leer la novela de forma lineal o seguir un orden de capítulos no secuencial.", Genero = "Novela", ISBN = "978-8437604034", Fecha_Publicacion = 1963, Fecha_Edicion = 2019, Edicion = "Edición Cátedra" },
            new Libro { Id = 3, IdAutor = 3, Titulo = "La Ciudad y los Perros", Descripcion = "La novela narra las vivencias de un grupo de jóvenes internos en un colegio militar en Lima, Perú.", Genero = "Novela", ISBN = "978-8466333868", Fecha_Publicacion = 1963, Fecha_Edicion = 2016, Edicion = "Edición Alfaguara" }
        };

        public List<Libro> GetAll()
        {
            return _libros;
        }

        public Libro GetById(int id)
        {
            return _libros.FirstOrDefault(l => l.Id == id);
        }

        public Libro Create(Libro libro)
        {
            int newId = _libros.Any() ? _libros.Max(l => l.Id) + 1 : 1;
            libro.Id = newId;
            _libros.Add(libro);
            return libro;
        }

        public void Update(int id, Libro libro)
        {
            var existingLibro = GetById(id);
            if (existingLibro != null)
            {
                existingLibro.IdAutor = libro.IdAutor;
                existingLibro.Titulo = libro.Titulo;
                existingLibro.Descripcion = libro.Descripcion;
                existingLibro.Genero = libro.Genero;
                existingLibro.ISBN = libro.ISBN;
                existingLibro.Fecha_Publicacion = libro.Fecha_Publicacion;
                existingLibro.Fecha_Edicion = libro.Fecha_Edicion;
                existingLibro.Edicion = libro.Edicion;
            }
        }

        public void Delete(int id)
        {
            _libros.RemoveAll(l => l.Id == id);
        }
    }
}
