using BankLink.Models;
using BankLink.Dtos;

namespace BankLink.interfaces
{
    public interface ITransferenciaService
    {
        List<Transferencia> GetAll();
        Transferencia GetById(int id);
        List<Transferencia> GetByCuentaOrigenId(int cuentaId);
        Transferencia Create(Transferencia transferencia);
        void Update(int id, Transferencia transferencia);
        
        // Métodos específicos para operaciones de transferencia
        Task<Transferencia> RealizarTransferenciaInternaAsync(TransferenciaDto dto);
        Task<Transferencia> RealizarTransferenciaExternaAsync(TransferenciaDto dto);
        Task<Transferencia> RecibirTransferenciaExternaAsync(TransferenciaRecibidaDto dto);
    }
}
