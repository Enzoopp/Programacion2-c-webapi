using BankLink.Context;
using BankLink.interfaces;
using BankLink.Models;
using Microsoft.EntityFrameworkCore;

namespace BankLink.Service
{
    public class MovimientoDbService : IMovimientoService
    {
        private readonly BankLinkDbContext _context;

        public MovimientoDbService(BankLinkDbContext context)
        {
            _context = context;
        }

        public Movimiento Create(Movimiento movimiento)
        {
            _context.Movimientos.Add(movimiento);
            _context.SaveChanges();
            return movimiento;
        }

        public void Delete(int id)
        {
            var movimiento = _context.Movimientos.Find(id);
            if (movimiento != null)
            {
                _context.Movimientos.Remove(movimiento);
                _context.SaveChanges();
            }
        }

        public List<Movimiento> GetAll()
        {
            return _context.Movimientos
                .Include(m => m.Cuenta)
                .OrderByDescending(m => m.FechaHora)
                .ToList();
        }

        public Movimiento GetById(int id)
        {
            return _context.Movimientos
                .Include(m => m.Cuenta)
                .FirstOrDefault(m => m.Id == id);
        }

        public List<Movimiento> GetByCuentaId(int cuentaId)
        {
            return _context.Movimientos
                .Include(m => m.Cuenta)
                .Where(m => m.IdCuenta == cuentaId)
                .OrderByDescending(m => m.FechaHora)
                .ToList();
        }
    }
}
