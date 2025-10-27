using BankLink.Models;
using BankLink.Context;
using BankLink.interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankLink.Service
{
    public class MovimientoService : IMovimientoService
    {
        private readonly BankLinkDbContext _context;

        public MovimientoService(BankLinkDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Movimiento>> GetAllAsync()
        {
            return await _context.Movimientos
                .Include(m => m.Cuenta)
                    .ThenInclude(c => c.Cliente)
                .Include(m => m.Transferencia)
                .OrderByDescending(m => m.FechaHora)
                .ToListAsync();
        }

        public async Task<Movimiento?> GetByIdAsync(int id)
        {
            return await _context.Movimientos
                .Include(m => m.Cuenta)
                    .ThenInclude(c => c.Cliente)
                .Include(m => m.Transferencia)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Movimiento>> GetByCuentaIdAsync(int cuentaId)
        {
            return await _context.Movimientos
                .Include(m => m.Cuenta)
                    .ThenInclude(c => c.Cliente)
                .Include(m => m.Transferencia)
                .Where(m => m.CuentaId == cuentaId)
                .OrderByDescending(m => m.FechaHora)
                .ToListAsync();
        }

        public async Task<IEnumerable<Movimiento>> GetByClienteIdAsync(int clienteId)
        {
            return await _context.Movimientos
                .Include(m => m.Cuenta)
                    .ThenInclude(c => c.Cliente)
                .Include(m => m.Transferencia)
                .Where(m => m.Cuenta.ClienteId == clienteId)
                .OrderByDescending(m => m.FechaHora)
                .ToListAsync();
        }

        public async Task<IEnumerable<Movimiento>> GetByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.Movimientos
                .Include(m => m.Cuenta)
                    .ThenInclude(c => c.Cliente)
                .Include(m => m.Transferencia)
                .Where(m => m.FechaHora >= fechaInicio && m.FechaHora <= fechaFin)
                .OrderByDescending(m => m.FechaHora)
                .ToListAsync();
        }

        public async Task<IEnumerable<Movimiento>> GetByTipoAsync(TipoMovimiento tipo)
        {
            return await _context.Movimientos
                .Include(m => m.Cuenta)
                    .ThenInclude(c => c.Cliente)
                .Include(m => m.Transferencia)
                .Where(m => m.Tipo == tipo)
                .OrderByDescending(m => m.FechaHora)
                .ToListAsync();
        }

        public async Task<IEnumerable<Movimiento>> GetByCuentaAndFechaRangoAsync(int cuentaId, DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.Movimientos
                .Include(m => m.Cuenta)
                    .ThenInclude(c => c.Cliente)
                .Include(m => m.Transferencia)
                .Where(m => m.CuentaId == cuentaId && 
                           m.FechaHora >= fechaInicio && 
                           m.FechaHora <= fechaFin)
                .OrderByDescending(m => m.FechaHora)
                .ToListAsync();
        }

        public async Task<IEnumerable<Movimiento>> GetByTransferenciaIdAsync(int transferenciaId)
        {
            return await _context.Movimientos
                .Include(m => m.Cuenta)
                    .ThenInclude(c => c.Cliente)
                .Include(m => m.Transferencia)
                .Where(m => m.TransferenciaId == transferenciaId)
                .OrderByDescending(m => m.FechaHora)
                .ToListAsync();
        }

        public async Task<Movimiento> CreateAsync(Movimiento movimiento)
        {
            // Validar que la cuenta existe
            var cuenta = await _context.Cuentas.FindAsync(movimiento.CuentaId);
            if (cuenta == null)
            {
                throw new InvalidOperationException("Cuenta no encontrada");
            }

            // Validar transferencia si existe
            if (movimiento.TransferenciaId.HasValue)
            {
                var transferencia = await _context.Transferencias.FindAsync(movimiento.TransferenciaId.Value);
                if (transferencia == null)
                {
                    throw new InvalidOperationException("Transferencia no encontrada");
                }
            }

            movimiento.FechaHora = DateTime.UtcNow;
            
            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync();
            
            return movimiento;
        }

        public async Task<Movimiento> UpdateAsync(Movimiento movimiento)
        {
            var existeMovimiento = await _context.Movimientos.FindAsync(movimiento.Id);
            if (existeMovimiento == null)
            {
                throw new InvalidOperationException("Movimiento no encontrado");
            }

            // Solo permitir actualizar la descripción
            existeMovimiento.Descripcion = movimiento.Descripcion;

            await _context.SaveChangesAsync();
            return existeMovimiento;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var movimiento = await _context.Movimientos.FindAsync(id);
            if (movimiento == null)
            {
                return false;
            }

            // No se pueden eliminar movimientos que afecten el saldo
            // Solo movimientos de tipo informativo o correcciones
            throw new InvalidOperationException("No se pueden eliminar movimientos que afecten el saldo de las cuentas");
        }

        public async Task<decimal> GetSaldoCalculadoAsync(int cuentaId)
        {
            var movimientos = await _context.Movimientos
                .Where(m => m.CuentaId == cuentaId)
                .ToListAsync();

            decimal saldo = 0;
            foreach (var movimiento in movimientos)
            {
                switch (movimiento.Tipo)
                {
                    case TipoMovimiento.Deposito:
                    case TipoMovimiento.TransferenciaRecibida:
                        saldo += movimiento.Monto;
                        break;
                    case TipoMovimiento.Retiro:
                    case TipoMovimiento.TransferenciaEnviada:
                        saldo -= movimiento.Monto;
                        break;
                }
            }

            return saldo;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Movimientos.AnyAsync(m => m.Id == id);
        }

        public async Task<int> GetCountByCuentaIdAsync(int cuentaId)
        {
            return await _context.Movimientos.CountAsync(m => m.CuentaId == cuentaId);
        }

        public async Task<decimal> GetTotalDepositosByCuentaIdAsync(int cuentaId)
        {
            return await _context.Movimientos
                .Where(m => m.CuentaId == cuentaId && 
                           (m.Tipo == TipoMovimiento.Deposito || m.Tipo == TipoMovimiento.TransferenciaRecibida))
                .SumAsync(m => m.Monto);
        }

        public async Task<decimal> GetTotalRetirosByCuentaIdAsync(int cuentaId)
        {
            return await _context.Movimientos
                .Where(m => m.CuentaId == cuentaId && 
                           (m.Tipo == TipoMovimiento.Retiro || m.Tipo == TipoMovimiento.TransferenciaEnviada))
                .SumAsync(m => m.Monto);
        }
    }
}