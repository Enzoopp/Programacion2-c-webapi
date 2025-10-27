using BankLink.Models;

namespace BankLink.interfaces
{
    public interface ITransferenciaService
    {
        Task<List<Transferencia>> GetAllAsync();
        Task<Transferencia?> GetByIdAsync(int id);
        Task<List<Transferencia>> GetByCuentaOrigenAsync(int cuentaOrigenId);
        Task<List<Transferencia>> GetByCuentaDestinoAsync(int cuentaDestinoId);
        Task<Transferencia> CreateAsync(Transferencia transferencia);
        Task UpdateAsync(int id, Transferencia transferencia);
        Task CambiarEstadoAsync(int id, EstadoTransferencia nuevoEstado);
        
        // Métodos específicos para transferencias
        Task<Transferencia> CrearTransferenciaInternaAsync(int cuentaOrigenId, int cuentaDestinoId, decimal monto, string descripcion);
        Task<Transferencia> CrearTransferenciaExternaAsync(int cuentaOrigenId, int bancoExternoId, string numeroCuentaDestino, decimal monto, string descripcion);
        Task<bool> ProcesarTransferenciaInternaAsync(int transferenciaId);
        Task<bool> ProcesarTransferenciaExternaAsync(int transferenciaId);
        Task<bool> RecibirTransferenciaExternaAsync(string numeroCuentaDestino, decimal monto, string descripcion, string referenciaExterna);
    }
}