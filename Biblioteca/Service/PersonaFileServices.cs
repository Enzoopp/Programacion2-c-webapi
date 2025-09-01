using Biblioteca.Models;
using Biblioteca.interfaces;
using System.Text.Json;
using System.IO;

namespace Biblioteca.Services
{
    public class PersonaFileServices : IPersonaService
    {
        private readonly string _filePath = Path.Combine("data", "personas.json");
        private readonly IFileStorageService _fileStorageService;
        private List<Persona> _personas;

        public PersonaFileServices(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
            LoadPersonasFromFile();
        }

        private void LoadPersonasFromFile()
        {
            var fileContent = _fileStorageService.Read(_filePath);
            _personas = JsonSerializer.Deserialize<List<Persona>>(fileContent) ?? new List<Persona>();
        }

        private void SavePersonasToFile()
        {
            var fileContent = JsonSerializer.Serialize(_personas);
            _fileStorageService.Write(_filePath, fileContent);
        }

        public List<Persona> GetAll()
        {
            return _personas;
        }

        public Persona GetById(int id)
        {
            return _personas.FirstOrDefault(p => p.Id == id);
        }

        public Persona Create(Persona persona)
        {
            int newId = _personas.Any() ? _personas.Max(p => p.Id) + 1 : 1;
            persona.Id = newId;
            _personas.Add(persona);
            SavePersonasToFile();
            return persona;
        }

        public Persona Update(int id, Persona persona)
        {
            var existingPersona = GetById(id);
            if (existingPersona != null)
            {
                existingPersona.Nombre = persona.Nombre;
                existingPersona.Apellido = persona.Apellido;
                existingPersona.FechaNacimiento = persona.FechaNacimiento;
                existingPersona.Dni = persona.Dni;
                SavePersonasToFile();
                return existingPersona;
            }
            return null;
        }

        public void Delete(int id)
        {
            var persona = GetById(id);
            if (persona != null)
            {
                _personas.Remove(persona);
                SavePersonasToFile();
            }
        }
    }
}