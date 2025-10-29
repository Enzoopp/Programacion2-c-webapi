using BankLink.Models;

namespace BankLink.interfaces
{
    public interface ICuentaService
    {
        List<Cuenta> GetAll();
        Cuenta GetById(int id);
        Cuenta GetByNumeroCuenta(string numeroCuenta);
        List<Cuenta> GetByClienteId(int clienteId);
        Cuenta Create(Cuenta cuenta);
        void Update(int id, Cuenta cuenta);
        void Delete(int id);
        void ActualizarSaldo(int id, decimal nuevoSaldo);
    }
}
