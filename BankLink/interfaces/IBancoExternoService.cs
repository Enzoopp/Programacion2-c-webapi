using BankLink.Models;

namespace BankLink.interfaces
{
    public interface IBancoExternoService
    {
        List<BancoExterno> GetAll();
        BancoExterno GetById(int id);
        BancoExterno GetByCodigoIdentificacion(string codigo);
        BancoExterno Create(BancoExterno banco);
        void Update(int id, BancoExterno banco);
        void Delete(int id);
    }
}
