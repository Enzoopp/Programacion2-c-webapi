using BankLink.Models;
using BankLink.interfaces;
using System.Text.Json;

namespace BankLink.Service
{
    public class MovimientoFileService : IMovimientoService
    {
        private readonly string _filePath = Path.Combine("data", "movimientos.json");
        private readonly IFileStorageService _fileStorageService;
        private List<Movimiento> _movimientos;

        public MovimientoFileService(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
            LoadMovimientosFromFile();
        }

        private void LoadMovimientosFromFile()
        {
            var fileContent = _fileStorageService.Read(_filePath);
            _movimientos = JsonSerializer.Deserialize<List<Movimiento>>(fileContent) ?? new List<Movimiento>();
        }

        private void SaveMovimientosToFile()
        {
            var fileContent = JsonSerializer.Serialize(_movimientos, new JsonSerializerOptions { WriteIndented = true });
            _fileStorageService.Write(_filePath, fileContent);
        }

        public List<Movimiento> GetAll()
        {
            return _movimientos.OrderByDescending(m => m.FechaHora).ToList();
        }

        public Movimiento GetById(int id)
        {
            return _movimientos.FirstOrDefault(m => m.Id == id);
        }

        public List<Movimiento> GetByCuentaId(int cuentaId)
        {
            return _movimientos
                .Where(m => m.IdCuenta == cuentaId)
                .OrderByDescending(m => m.FechaHora)
                .ToList();
        }

        public Movimiento Create(Movimiento movimiento)
        {
            int newId = _movimientos.Any() ? _movimientos.Max(m => m.Id) + 1 : 1;
            movimiento.Id = newId;
            movimiento.FechaHora = DateTime.Now;
            _movimientos.Add(movimiento);
            SaveMovimientosToFile();
            return movimiento;
        }

        public void Delete(int id)
        {
            _movimientos.RemoveAll(m => m.Id == id);
            SaveMovimientosToFile();
        }
    }
}
