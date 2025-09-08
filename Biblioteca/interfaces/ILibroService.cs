using Biblioteca.Models;
using System.Collections.Generic;

namespace Biblioteca.interfaces
{
    public interface ILibroService
    {
        List<Libro> GetAll();
        Libro GetById(int id);
        void Update(int id, Libro libro);
        void Delete(int id);
        Libro Create(Libro libro);
    }
}
