using BankLink.Models;
using BankLink.interfaces;
using System.Text.Json;

namespace BankLink.Service
{
    public class ClienteFileService : IClienteService
    {
        private readonly string _filePath = Path.Combine("data", "clientes.json");
        private readonly IFileStorageService _fileStorageService;
        private List<Cliente> _clientes;

        public ClienteFileService(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
            LoadClientesFromFile();
        }

        private void LoadClientesFromFile()
        {
            var fileContent = _fileStorageService.Read(_filePath);
            _clientes = JsonSerializer.Deserialize<List<Cliente>>(fileContent) ?? new List<Cliente>();
        }

        private void SaveClientesToFile()
        {
            var fileContent = JsonSerializer.Serialize(_clientes, new JsonSerializerOptions { WriteIndented = true });
            _fileStorageService.Write(_filePath, fileContent);
        }

        public List<Cliente> GetAll()
        {
            return _clientes;
        }

        public Cliente GetById(int id)
        {
            return _clientes.FirstOrDefault(c => c.Id == id);
        }

        public Cliente GetByNombreUsuario(string nombreUsuario)
        {
            return _clientes.FirstOrDefault(c => c.NombreUsuario == nombreUsuario);
        }

        public Cliente GetByDni(string dni)
        {
            return _clientes.FirstOrDefault(c => c.Dni == dni);
        }

        public Cliente Create(Cliente cliente)
        {
            int newId = _clientes.Any() ? _clientes.Max(c => c.Id) + 1 : 1;
            cliente.Id = newId;
            _clientes.Add(cliente);
            SaveClientesToFile();
            return cliente;
        }

        public void Update(int id, Cliente cliente)
        {
            var existingCliente = GetById(id);
            if (existingCliente != null)
            {
                existingCliente.Nombre = cliente.Nombre;
                existingCliente.Apellido = cliente.Apellido;
                existingCliente.Dni = cliente.Dni;
                existingCliente.Direccion = cliente.Direccion;
                existingCliente.Telefono = cliente.Telefono;
                existingCliente.Email = cliente.Email;
                existingCliente.NombreUsuario = cliente.NombreUsuario;
                existingCliente.PassHash = cliente.PassHash;
                existingCliente.Rol = cliente.Rol;
                SaveClientesToFile();
            }
        }

        public void Delete(int id)
        {
            _clientes.RemoveAll(c => c.Id == id);
            SaveClientesToFile();
        }
    }
}
