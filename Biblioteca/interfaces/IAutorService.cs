using Biblioteca.Models;
using System.Collections.Generic;

namespace Biblioteca.interfaces
{
    public interface IAutorService
    {
        List<Autor> GetAll();
        Autor GetById(int id);
        void Update(int id, Autor autor);
        void Delete(int id);
        Autor Create(Autor autor);
    }
}
