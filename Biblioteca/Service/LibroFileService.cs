using Biblioteca.Models;
using Biblioteca.interfaces;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Biblioteca.Services
{
    public class LibroFileService : ILibroService
    {
        private readonly string _filePath = Path.Combine("data", "libros.json");
        private readonly IFileStorageService _fileStorageService;
        private List<Libro> _libros;

        public LibroFileService(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
            LoadLibrosFromFile();
        }

        private void LoadLibrosFromFile()
        {
            var fileContent = _fileStorageService.Read(_filePath);
            _libros = JsonSerializer.Deserialize<List<Libro>>(fileContent) ?? new List<Libro>();
        }

        private void SaveLibrosToFile()
        {
            var fileContent = JsonSerializer.Serialize(_libros);
            _fileStorageService.Write(_filePath, fileContent);
        }

        public List<Libro> GetAll()
        {
            return _libros;
        }

        public Libro GetById(int id)
        {
            return _libros.FirstOrDefault(l => l.Id == id);
        }

        public Libro Create(Libro libro)
        {
            int newId = _libros.Any() ? _libros.Max(l => l.Id) + 1 : 1;
            libro.Id = newId;
            _libros.Add(libro);
            SaveLibrosToFile();
            return libro;
        }

        public void Update(int id, Libro libro)
        {
            var existingLibro = GetById(id);
            if (existingLibro != null)
            {
                existingLibro.IdAutor = libro.IdAutor;
                existingLibro.Titulo = libro.Titulo;
                existingLibro.Descripcion = libro.Descripcion;
                existingLibro.Genero = libro.Genero;
                existingLibro.ISBN = libro.ISBN;
                existingLibro.Fecha_Publicacion = libro.Fecha_Publicacion;
                existingLibro.Fecha_Edicion = libro.Fecha_Edicion;
                existingLibro.Edicion = libro.Edicion;
                SaveLibrosToFile();
            }
        }

        public void Delete(int id)
        {
            _libros.RemoveAll(l => l.Id == id);
            SaveLibrosToFile();
        }
    }
}
