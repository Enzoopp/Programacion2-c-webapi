using BankLink.Models;
using BankLink.Context;
using BankLink.interfaces;
using BankLink.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BankLink.Service
{
    public class CuentaService : ICuentaService
    {
        private readonly BankLinkDbContext _context;
        private readonly IMovimientoService _movimientoService;

        public CuentaService(BankLinkDbContext context, IMovimientoService movimientoService)
        {
            _context = context;
            _movimientoService = movimientoService;
        }

        public async Task<IEnumerable<Cuenta>> GetAllAsync()
        {
            return await _context.Cuentas
                .Include(c => c.Cliente)
                .Include(c => c.Movimientos)
                .ToListAsync();
        }

        public async Task<Cuenta?> GetByIdAsync(int id)
        {
            return await _context.Cuentas
                .Include(c => c.Cliente)
                .Include(c => c.Movimientos)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Cuenta?> GetByNumeroCuentaAsync(string numeroCuenta)
        {
            return await _context.Cuentas
                .Include(c => c.Cliente)
                .Include(c => c.Movimientos)
                .FirstOrDefaultAsync(c => c.NumeroCuenta == numeroCuenta);
        }

        public async Task<IEnumerable<Cuenta>> GetByClienteIdAsync(int clienteId)
        {
            return await _context.Cuentas
                .Include(c => c.Cliente)
                .Include(c => c.Movimientos)
                .Where(c => c.ClienteId == clienteId)
                .ToListAsync();
        }

        public async Task<Cuenta> CreateAsync(Cuenta cuenta)
        {
            // Validar que el cliente existe
            var cliente = await _context.Clientes.FindAsync(cuenta.ClienteId);
            if (cliente == null)
            {
                throw new InvalidOperationException("Cliente no encontrado");
            }

            // Validar que no exista el número de cuenta
            var existeNumeroCuenta = await _context.Cuentas
                .AnyAsync(c => c.NumeroCuenta == cuenta.NumeroCuenta);
            
            if (existeNumeroCuenta)
            {
                throw new InvalidOperationException("Ya existe una cuenta con este número");
            }

            cuenta.FechaCreacion = DateTime.UtcNow;
            cuenta.Estado = EstadoCuenta.Activa;
            
            _context.Cuentas.Add(cuenta);
            await _context.SaveChangesAsync();
            
            return cuenta;
        }

        public async Task<Cuenta> UpdateAsync(Cuenta cuenta)
        {
            var existeCuenta = await _context.Cuentas.FindAsync(cuenta.Id);
            if (existeCuenta == null)
            {
                throw new InvalidOperationException("Cuenta no encontrada");
            }

            // Validar que no exista el número de cuenta en otra cuenta
            var existeNumeroCuenta = await _context.Cuentas
                .AnyAsync(c => c.NumeroCuenta == cuenta.NumeroCuenta && c.Id != cuenta.Id);
            
            if (existeNumeroCuenta)
            {
                throw new InvalidOperationException("Ya existe otra cuenta con este número");
            }

            existeCuenta.NumeroCuenta = cuenta.NumeroCuenta;
            existeCuenta.Tipo = cuenta.Tipo;
            existeCuenta.Estado = cuenta.Estado;

            await _context.SaveChangesAsync();
            return existeCuenta;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var cuenta = await _context.Cuentas
                .Include(c => c.Movimientos)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (cuenta == null)
            {
                return false;
            }

            // Verificar que el saldo sea cero
            if (cuenta.Saldo != 0)
            {
                throw new InvalidOperationException("No se puede eliminar una cuenta con saldo diferente a cero");
            }

            _context.Cuentas.Remove(cuenta);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetSaldoAsync(int cuentaId)
        {
            var cuenta = await _context.Cuentas.FindAsync(cuentaId);
            if (cuenta == null)
            {
                throw new InvalidOperationException("Cuenta no encontrada");
            }
            
            return cuenta.Saldo;
        }

        public async Task<bool> DepositarAsync(DepositoDto deposito)
        {
            var cuenta = await _context.Cuentas.FindAsync(deposito.CuentaId);
            if (cuenta == null)
            {
                throw new InvalidOperationException("Cuenta no encontrada");
            }

            if (cuenta.Estado != EstadoCuenta.Activa)
            {
                throw new InvalidOperationException("La cuenta no está activa");
            }

            if (deposito.Monto <= 0)
            {
                throw new InvalidOperationException("El monto debe ser mayor a cero");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Actualizar saldo
                cuenta.Saldo += deposito.Monto;
                
                // Crear movimiento
                var movimiento = new Movimiento
                {
                    CuentaId = deposito.CuentaId,
                    Tipo = TipoMovimiento.Deposito,
                    Monto = deposito.Monto,
                    FechaHora = DateTime.UtcNow,
                    Descripcion = deposito.Descripcion ?? "Depósito"
                };

                await _movimientoService.CreateAsync(movimiento);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> RetirarAsync(RetiroDto retiro)
        {
            var cuenta = await _context.Cuentas.FindAsync(retiro.CuentaId);
            if (cuenta == null)
            {
                throw new InvalidOperationException("Cuenta no encontrada");
            }

            if (cuenta.Estado != EstadoCuenta.Activa)
            {
                throw new InvalidOperationException("La cuenta no está activa");
            }

            if (retiro.Monto <= 0)
            {
                throw new InvalidOperationException("El monto debe ser mayor a cero");
            }

            if (cuenta.Saldo < retiro.Monto)
            {
                throw new InvalidOperationException("Saldo insuficiente");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Actualizar saldo
                cuenta.Saldo -= retiro.Monto;
                
                // Crear movimiento
                var movimiento = new Movimiento
                {
                    CuentaId = retiro.CuentaId,
                    Tipo = TipoMovimiento.Retiro,
                    Monto = retiro.Monto,
                    FechaHora = DateTime.UtcNow,
                    Descripcion = retiro.Descripcion ?? "Retiro"
                };

                await _movimientoService.CreateAsync(movimiento);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> BloquearCuentaAsync(int cuentaId)
        {
            var cuenta = await _context.Cuentas.FindAsync(cuentaId);
            if (cuenta == null)
            {
                return false;
            }

            cuenta.Estado = EstadoCuenta.Bloqueada;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivarCuentaAsync(int cuentaId)
        {
            var cuenta = await _context.Cuentas.FindAsync(cuentaId);
            if (cuenta == null)
            {
                return false;
            }

            cuenta.Estado = EstadoCuenta.Activa;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Cuentas.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> ExistsByNumeroCuentaAsync(string numeroCuenta)
        {
            return await _context.Cuentas.AnyAsync(c => c.NumeroCuenta == numeroCuenta);
        }
    }
}