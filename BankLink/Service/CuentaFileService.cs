using BankLink.Models;
using BankLink.interfaces;
using System.Text.Json;

namespace BankLink.Service
{
    public class CuentaFileService : ICuentaService
    {
        private readonly string _filePath = Path.Combine("data", "cuentas.json");
        private readonly IFileStorageService _fileStorageService;
        private List<Cuenta> _cuentas;

        public CuentaFileService(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
            LoadCuentasFromFile();
        }

        private void LoadCuentasFromFile()
        {
            var fileContent = _fileStorageService.Read(_filePath);
            _cuentas = JsonSerializer.Deserialize<List<Cuenta>>(fileContent) ?? new List<Cuenta>();
        }

        private void SaveCuentasToFile()
        {
            var fileContent = JsonSerializer.Serialize(_cuentas, new JsonSerializerOptions { WriteIndented = true });
            _fileStorageService.Write(_filePath, fileContent);
        }

        public List<Cuenta> GetAll()
        {
            return _cuentas;
        }

        public Cuenta GetById(int id)
        {
            return _cuentas.FirstOrDefault(c => c.Id == id);
        }

        public Cuenta GetByNumeroCuenta(string numeroCuenta)
        {
            return _cuentas.FirstOrDefault(c => c.NumeroCuenta == numeroCuenta);
        }

        public List<Cuenta> GetByClienteId(int clienteId)
        {
            return _cuentas.Where(c => c.IdClientePropietario == clienteId).ToList();
        }

        public Cuenta Create(Cuenta cuenta)
        {
            int newId = _cuentas.Any() ? _cuentas.Max(c => c.Id) + 1 : 1;
            cuenta.Id = newId;
            _cuentas.Add(cuenta);
            SaveCuentasToFile();
            return cuenta;
        }

        public void Update(int id, Cuenta cuenta)
        {
            var existingCuenta = GetById(id);
            if (existingCuenta != null)
            {
                existingCuenta.NumeroCuenta = cuenta.NumeroCuenta;
                existingCuenta.TipoCuenta = cuenta.TipoCuenta;
                existingCuenta.SaldoActual = cuenta.SaldoActual;
                existingCuenta.Estado = cuenta.Estado;
                existingCuenta.IdClientePropietario = cuenta.IdClientePropietario;
                SaveCuentasToFile();
            }
        }

        public void Delete(int id)
        {
            _cuentas.RemoveAll(c => c.Id == id);
            SaveCuentasToFile();
        }

        public void ActualizarSaldo(int id, decimal nuevoSaldo)
        {
            var cuenta = GetById(id);
            if (cuenta != null)
            {
                cuenta.SaldoActual = nuevoSaldo;
                SaveCuentasToFile();
            }
        }
    }
}
