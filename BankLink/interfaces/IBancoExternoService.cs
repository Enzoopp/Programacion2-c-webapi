using BankLink.Models;

namespace BankLink.interfaces
{
    public interface IBancoExternoService
    {
        Task<List<BancoExterno>> GetAllAsync();
        Task<BancoExterno?> GetByIdAsync(int id);
        Task<BancoExterno?> GetByCodigoIdentificacionAsync(string codigo);
        Task<BancoExterno> CreateAsync(BancoExterno banco);
        Task UpdateAsync(int id, BancoExterno banco);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByCodigoAsync(string codigo);
        Task CambiarEstadoAsync(int id, bool activo);
        
        // Métodos para integración con APIs externas
        Task<bool> ValidarConexionAsync(int bancoId);
        Task<bool> EnviarTransferenciaAsync(int bancoId, string numeroCuentaDestino, decimal monto, string descripcion, string referenciaInterna);
    }
}