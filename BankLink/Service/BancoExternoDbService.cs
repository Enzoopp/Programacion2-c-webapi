using BankLink.Context;
using BankLink.interfaces;
using BankLink.Models;

namespace BankLink.Service
{
    public class BancoExternoDbService : IBancoExternoService
    {
        private readonly BankLinkDbContext _context;

        public BancoExternoDbService(BankLinkDbContext context)
        {
            _context = context;
        }

        public BancoExterno Create(BancoExterno banco)
        {
            _context.BancosExternos.Add(banco);
            _context.SaveChanges();
            return banco;
        }

        public void Delete(int id)
        {
            var banco = _context.BancosExternos.Find(id);
            if (banco != null)
            {
                _context.BancosExternos.Remove(banco);
                _context.SaveChanges();
            }
        }

        public List<BancoExterno> GetAll()
        {
            return _context.BancosExternos.ToList();
        }

        public BancoExterno GetById(int id)
        {
            return _context.BancosExternos.Find(id);
        }

        public BancoExterno GetByCodigoIdentificacion(string codigo)
        {
            return _context.BancosExternos.FirstOrDefault(b => b.CodigoIdentificacion == codigo);
        }

        public void Update(int id, BancoExterno banco)
        {
            var bancoExistente = _context.BancosExternos.Find(id);
            if (bancoExistente != null)
            {
                bancoExistente.NombreBanco = banco.NombreBanco;
                bancoExistente.CodigoIdentificacion = banco.CodigoIdentificacion;
                bancoExistente.UrlApiBase = banco.UrlApiBase;
                bancoExistente.Descripcion = banco.Descripcion;
                bancoExistente.Activo = banco.Activo;
                _context.SaveChanges();
            }
        }
    }
}
