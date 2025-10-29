using BankLink.Context;
using BankLink.interfaces;
using BankLink.Models;
using Microsoft.EntityFrameworkCore;

namespace BankLink.Service
{
    public class CuentaDbService : ICuentaService
    {
        private readonly BankLinkDbContext _context;

        public CuentaDbService(BankLinkDbContext context)
        {
            _context = context;
        }

        public Cuenta Create(Cuenta cuenta)
        {
            _context.Cuentas.Add(cuenta);
            _context.SaveChanges();
            return cuenta;
        }

        public void Delete(int id)
        {
            var cuenta = _context.Cuentas.Find(id);
            if (cuenta != null)
            {
                _context.Cuentas.Remove(cuenta);
                _context.SaveChanges();
            }
        }

        public List<Cuenta> GetAll()
        {
            return _context.Cuentas
                .Include(c => c.ClientePropietario)
                .ToList();
        }

        public Cuenta GetById(int id)
        {
            return _context.Cuentas
                .Include(c => c.ClientePropietario)
                .FirstOrDefault(c => c.Id == id);
        }

        public Cuenta GetByNumeroCuenta(string numeroCuenta)
        {
            return _context.Cuentas
                .Include(c => c.ClientePropietario)
                .FirstOrDefault(c => c.NumeroCuenta == numeroCuenta);
        }

        public List<Cuenta> GetByClienteId(int clienteId)
        {
            return _context.Cuentas
                .Include(c => c.ClientePropietario)
                .Where(c => c.IdClientePropietario == clienteId)
                .ToList();
        }

        public void Update(int id, Cuenta cuenta)
        {
            var cuentaExistente = _context.Cuentas.Find(id);
            if (cuentaExistente != null)
            {
                cuentaExistente.NumeroCuenta = cuenta.NumeroCuenta;
                cuentaExistente.TipoCuenta = cuenta.TipoCuenta;
                cuentaExistente.SaldoActual = cuenta.SaldoActual;
                cuentaExistente.Estado = cuenta.Estado;
                cuentaExistente.IdClientePropietario = cuenta.IdClientePropietario;
                _context.SaveChanges();
            }
        }

        public void ActualizarSaldo(int id, decimal nuevoSaldo)
        {
            var cuenta = _context.Cuentas.Find(id);
            if (cuenta != null)
            {
                cuenta.SaldoActual = nuevoSaldo;
                _context.SaveChanges();
            }
        }
    }
}
