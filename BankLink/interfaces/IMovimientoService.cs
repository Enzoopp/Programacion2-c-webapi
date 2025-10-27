using BankLink.Models;

namespace BankLink.interfaces
{
    public interface IMovimientoService
    {
        Task<List<Movimiento>> GetAllAsync();
        Task<Movimiento?> GetByIdAsync(int id);
        Task<List<Movimiento>> GetByCuentaIdAsync(int cuentaId);
        Task<List<Movimiento>> GetByTransferenciaIdAsync(int transferenciaId);
        Task<Movimiento> CreateAsync(Movimiento movimiento);
        Task<List<Movimiento>> GetHistorialCuentaAsync(int cuentaId, DateTime? fechaDesde = null, DateTime? fechaHasta = null);
        Task<decimal> CalcularSaldoCuentaAsync(int cuentaId);
        
        // Métodos específicos para operaciones bancarias
        Task<Movimiento> RegistrarDepositoAsync(int cuentaId, decimal monto, string descripcion);
        Task<Movimiento> RegistrarRetiroAsync(int cuentaId, decimal monto, string descripcion);
        Task<Movimiento> RegistrarTransferenciaEnviadaAsync(int cuentaId, decimal monto, string descripcion, int? transferenciaId = null);
        Task<Movimiento> RegistrarTransferenciaRecibidaAsync(int cuentaId, decimal monto, string descripcion, int? transferenciaId = null);
    }
}