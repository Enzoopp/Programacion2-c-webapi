using Biblioteca.Models;
using Biblioteca.interfaces;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Biblioteca.Services
{
    public class AutorFileService : IAutorService
    {
        private readonly string _filePath = Path.Combine("data", "autores.json");
        private readonly IFileStorageService _fileStorageService;
        private List<Autor> _autores;

        public AutorFileService(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
            LoadAutoresFromFile();
        }

        private void LoadAutoresFromFile()
        {
            var fileContent = _fileStorageService.Read(_filePath);
            _autores = JsonSerializer.Deserialize<List<Autor>>(fileContent) ?? new List<Autor>();
        }

        private void SaveAutoresToFile()
        {
            var fileContent = JsonSerializer.Serialize(_autores);
            _fileStorageService.Write(_filePath, fileContent);
        }

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
            SaveAutoresToFile();
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
                SaveAutoresToFile();
            }
        }

        public void Delete(int id)
        {
            _autores.RemoveAll(a => a.Id == id);
            SaveAutoresToFile();
        }
    }
}
