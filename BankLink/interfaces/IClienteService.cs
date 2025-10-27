using BankLink.Models;

namespace BankLink.interfaces
{
    public interface IClienteService
    {
        Task<List<Cliente>> GetAllAsync();
        Task<Cliente?> GetByIdAsync(int id);
        Task<Cliente?> GetByIdentificacionAsync(string identificacion);
        Task<Cliente> CreateAsync(Cliente cliente);
        Task UpdateAsync(int id, Cliente cliente);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByIdentificacionAsync(string identificacion);
        Task<bool> ExistsByEmailAsync(string email);
    }
}