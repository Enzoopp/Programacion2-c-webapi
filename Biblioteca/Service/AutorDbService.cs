using Biblioteca.Context;
using Biblioteca.interfaces;
using Biblioteca.Models;
using System.Collections.Generic;
using System.Linq;

namespace Biblioteca.Services
{
    public class AutorDbService : IAutorService
    {
        private readonly BibliotecaDbContext _context;

        public AutorDbService(BibliotecaDbContext context)
        {
            _context = context;
        }

        public Autor Create(Autor autor)
        {
            _context.Autores.Add(autor);
            _context.SaveChanges();
            return autor;
        }

        public void Delete(int id)
        {
            var autor = _context.Autores.Find(id);
            if (autor != null)
            {
                _context.Autores.Remove(autor);
                _context.SaveChanges();
            }
        }

        public List<Autor> GetAll()
        {
            return _context.Autores.ToList();
        }

        public Autor GetById(int id)
        {
            return _context.Autores.Find(id);
        }

        public void Update(int id, Autor autor)
        {
            var autorExistente = _context.Autores.Find(id);
            if (autorExistente != null)
            {
                autorExistente.Nombre = autor.Nombre;
                autorExistente.Nacionalidad = autor.Nacionalidad;
                autorExistente.Descripcion = autor.Descripcion;
                _context.SaveChanges();
            }
        }
    }
}
