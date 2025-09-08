using Biblioteca.Models;
using Biblioteca.interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Biblioteca.Services
{
    public class AutorMemService : IAutorService
    {
        private static List<Autor> _autores = new List<Autor>
        {
            new Autor { Id = 1, Nombre = "Gabriel Garcia Marquez", Nacionalidad = "Colombiano", Descripcion = "Escritor de Cien Años de Soledad" },
            new Autor { Id = 2, Nombre = "Julio Cortazar", Nacionalidad = "Argentino", Descripcion = "Escritor de Rayuela" },
            new Autor { Id = 3, Nombre = "Mario Vargas Llosa", Nacionalidad = "Peruano", Descripcion = "Escritor de La Ciudad y los Perros" }
        };

        public List<Autor> GetAll()
        {
            return _autores;
        }

        public Autor GetById(int id)
        {
            return _autores.FirstOrDefault(a => a.Id == id);
        }

        public Autor Create(Autor autor)
        {
            int newId = _autores.Any() ? _autores.Max(a => a.Id) + 1 : 1;
            autor.Id = newId;
            _autores.Add(autor);
            return autor;
        }

        public void Update(int id, Autor autor)
        {
            var existingAutor = GetById(id);
            if (existingAutor != null)
            {
                existingAutor.Nombre = autor.Nombre;
                existingAutor.Nacionalidad = autor.Nacionalidad;
                existingAutor.Descripcion = autor.Descripcion;
            }
        }

        public void Delete(int id)
        {
            _autores.RemoveAll(a => a.Id == id);
        }
    }
}
