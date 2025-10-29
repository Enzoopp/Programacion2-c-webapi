using BankLink.Models;

namespace BankLink.interfaces
{
    public interface IClienteService
    {
        List<Cliente> GetAll();
        Cliente GetById(int id);
        Cliente GetByNombreUsuario(string nombreUsuario);
        Cliente GetByDni(string dni);
        Cliente Create(Cliente cliente);
        void Update(int id, Cliente cliente);
        void Delete(int id);
    }
}
