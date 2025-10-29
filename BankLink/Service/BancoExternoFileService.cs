using BankLink.Models;
using BankLink.interfaces;
using System.Text.Json;

namespace BankLink.Service
{
    public class BancoExternoFileService : IBancoExternoService
    {
        private readonly string _filePath = Path.Combine("data", "bancos_externos.json");
        private readonly IFileStorageService _fileStorageService;
        private List<BancoExterno> _bancos;

        public BancoExternoFileService(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
            LoadBancosFromFile();
        }

        private void LoadBancosFromFile()
        {
            var fileContent = _fileStorageService.Read(_filePath);
            _bancos = JsonSerializer.Deserialize<List<BancoExterno>>(fileContent) ?? new List<BancoExterno>();
        }

        private void SaveBancosToFile()
        {
            var fileContent = JsonSerializer.Serialize(_bancos, new JsonSerializerOptions { WriteIndented = true });
            _fileStorageService.Write(_filePath, fileContent);
        }

        public List<BancoExterno> GetAll()
        {
            return _bancos;
        }

        public BancoExterno GetById(int id)
        {
            return _bancos.FirstOrDefault(b => b.Id == id);
        }

        public BancoExterno GetByCodigoIdentificacion(string codigo)
        {
            return _bancos.FirstOrDefault(b => b.CodigoIdentificacion == codigo);
        }

        public BancoExterno Create(BancoExterno banco)
        {
            int newId = _bancos.Any() ? _bancos.Max(b => b.Id) + 1 : 1;
            banco.Id = newId;
            _bancos.Add(banco);
            SaveBancosToFile();
            return banco;
        }

        public void Update(int id, BancoExterno banco)
        {
            var existingBanco = GetById(id);
            if (existingBanco != null)
            {
                existingBanco.NombreBanco = banco.NombreBanco;
                existingBanco.CodigoIdentificacion = banco.CodigoIdentificacion;
                existingBanco.UrlApiBase = banco.UrlApiBase;
                existingBanco.Descripcion = banco.Descripcion;
                existingBanco.Activo = banco.Activo;
                SaveBancosToFile();
            }
        }

        public void Delete(int id)
        {
            _bancos.RemoveAll(b => b.Id == id);
            SaveBancosToFile();
        }
    }
}
