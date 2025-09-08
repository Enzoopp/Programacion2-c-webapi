using Biblioteca.Models;
using Biblioteca.interfaces;

namespace Biblioteca.Services
{
    public class PersonaMemService : IPersonaService
    {
        private static List<Persona> _listaPersonas = new List<Persona>
        {
            new Persona { Id = 1, Nombre = "Enzo", Apellido = "Pitana", FechaNacimiento = new DateTime(1998, 5, 20), Dni = "46976644" },
            new Persona { Id = 2, Nombre = "Lucía", Apellido = "Fernández", FechaNacimiento = new DateTime(2000, 3, 15), Dni = "40234567" },
            new Persona { Id = 3, Nombre = "Martín", Apellido = "Gómez", FechaNacimiento = new DateTime(1995, 7, 10), Dni = "38911223" },
            new Persona { Id = 4, Nombre = "Carla", Apellido = "López", FechaNacimiento = new DateTime(1992, 11, 2), Dni = "37655433" },
            new Persona { Id = 5, Nombre = "Santiago", Apellido = "Rodríguez", FechaNacimiento = new DateTime(1989, 1, 30), Dni = "35466778" }
        };
        public Persona Create(Persona persona)
        {
            persona.Id = _listaPersonas.Max(p => p.Id) + 1;
            _listaPersonas.Add(persona);
            return persona;
        }

        public void Delete(int id)
        {
            var persona = _listaPersonas.FirstOrDefault(p => p.Id == id);
            if (persona != null)
            {
                _listaPersonas.Remove(persona);
            }
        }

        public List<Persona> GetAll()
        {
            return _listaPersonas;
        }

        public Persona GetById(int id)
        {
            var persona = _listaPersonas.FirstOrDefault(p => p.Id == id);
            if (persona != null)
            {
                return persona;
            }
            return null;
        }

        public void Update(int id, Persona persona)
        {
            var personaLista = _listaPersonas.FirstOrDefault(p => p.Id == id);
            if (personaLista != null)
            {
                personaLista.Nombre = persona.Nombre;
                personaLista.Apellido = persona.Apellido;
                personaLista.FechaNacimiento = persona.FechaNacimiento;
                personaLista.Dni = persona.Dni;
            }
        }
    }
}