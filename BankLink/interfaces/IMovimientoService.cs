using BankLink.Models;

namespace BankLink.interfaces
{
    public interface IMovimientoService
    {
        List<Movimiento> GetAll();
        Movimiento GetById(int id);
        List<Movimiento> GetByCuentaId(int cuentaId);
        Movimiento Create(Movimiento movimiento);
        void Delete(int id);
    }
}
