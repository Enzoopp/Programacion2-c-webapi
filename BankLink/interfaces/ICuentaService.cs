using BankLink.Models;

namespace BankLink.interfaces
{
    public interface ICuentaService
    {
        Task<List<Cuenta>> GetAllAsync();
        Task<Cuenta?> GetByIdAsync(int id);
        Task<Cuenta?> GetByNumeroCuentaAsync(string numeroCuenta);
        Task<List<Cuenta>> GetByClienteIdAsync(int clienteId);
        Task<Cuenta> CreateAsync(Cuenta cuenta);
        Task UpdateAsync(int id, Cuenta cuenta);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByNumeroCuentaAsync(string numeroCuenta);
        Task<decimal> GetSaldoAsync(int cuentaId);
        Task ActualizarSaldoAsync(int cuentaId, decimal nuevoSaldo);
        Task<bool> TieneSaldoSuficienteAsync(int cuentaId, decimal monto);
        Task CambiarEstadoAsync(int cuentaId, EstadoCuenta nuevoEstado);
    }
}