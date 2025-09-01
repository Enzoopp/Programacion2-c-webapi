using Biblioteca.Models;

namespace Biblioteca.interfaces
{
    public interface IPersonaService
    {
        List<Persona> GetAll();
        Persona GetById(int id);
        Persona Update(int id, Persona persona);
        void Delete(int id);
        Persona Create(Persona persona);
    
    }
} 