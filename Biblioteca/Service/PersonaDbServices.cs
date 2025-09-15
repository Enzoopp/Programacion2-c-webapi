using Biblioteca.Context;
using Biblioteca.interfaces;
using Biblioteca.Models;
using System.Collections.Generic;
using System.Linq;

namespace Biblioteca.Services
{
    public class PersonaDbServices : IPersonaService
    {
        private readonly BibliotecaDbContext _context;

        public PersonaDbServices(BibliotecaDbContext context)
        {
            _context = context;
        }

        public Persona Create(Persona persona)
        {
            _context.Personas.Add(persona);
            _context.SaveChanges();
            return persona;
        }

        public void Delete(int id)
        {
            var persona = _context.Personas.Find(id);
            if (persona != null)
            {
                _context.Personas.Remove(persona);
                _context.SaveChanges();
            }
        }

        public List<Persona> GetAll()
        {
            return _context.Personas.ToList();
        }

        public Persona GetById(int id)
        {
            return _context.Personas.Find(id);
        }

        public void Update(int id, Persona persona)
        {
            var personaExistente = _context.Personas.Find(id);
            if (personaExistente != null)
            {
                personaExistente.Nombre = persona.Nombre;
                personaExistente.Apellido = persona.Apellido;
                personaExistente.FechaNacimiento = persona.FechaNacimiento;
                personaExistente.Dni = persona.Dni;
                _context.SaveChanges();
            }
        }
    }
}
