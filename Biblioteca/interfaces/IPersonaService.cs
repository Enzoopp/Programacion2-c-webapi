using Biblioteca.Models;
using System.Collections.Generic;

namespace Biblioteca.interfaces
{
    public interface IPersonaService
    {
        List<Persona> GetAll();
        Persona GetById(int id);
        void Update(int id, Persona persona);
        void Delete(int id);
        Persona Create(Persona persona);
        Persona GetByNombreUsuario(string nombreUsuario);
    }
} 