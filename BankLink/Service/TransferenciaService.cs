using BankLink.Models;
using BankLink.Context;
using BankLink.interfaces;
using BankLink.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BankLink.Service
{
    public class TransferenciaService : ITransferenciaService
    {
        private readonly BankLinkDbContext _context;
        private readonly IMovimientoService _movimientoService;
        private readonly IBancoExternoService _bancoExternoService;

        public TransferenciaService(BankLinkDbContext context, IMovimientoService movimientoService, IBancoExternoService bancoExternoService)
        {
            _context = context;
            _movimientoService = movimientoService;
            _bancoExternoService = bancoExternoService;
        }

        public async Task<IEnumerable<Transferencia>> GetAllAsync()
        {
            return await _context.Transferencias
                .Include(t => t.CuentaOrigen)
                    .ThenInclude(c => c.Cliente)
                .Include(t => t.CuentaDestino)
                    .ThenInclude(c => c.Cliente)
                .Include(t => t.BancoExterno)
                .Include(t => t.Movimientos)
                .OrderByDescending(t => t.FechaHora)
                .ToListAsync();
        }

        public async Task<Transferencia?> GetByIdAsync(int id)
        {
            return await _context.Transferencias
                .Include(t => t.CuentaOrigen)
                    .ThenInclude(c => c.Cliente)
                .Include(t => t.CuentaDestino)
                    .ThenInclude(c => c.Cliente)
                .Include(t => t.BancoExterno)
                .Include(t => t.Movimientos)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Transferencia>> GetByCuentaOrigenIdAsync(int cuentaOrigenId)
        {
            return await _context.Transferencias
                .Include(t => t.CuentaOrigen)
                    .ThenInclude(c => c.Cliente)
                .Include(t => t.CuentaDestino)
                    .ThenInclude(c => c.Cliente)
                .Include(t => t.BancoExterno)
                .Include(t => t.Movimientos)
                .Where(t => t.CuentaOrigenId == cuentaOrigenId)
                .OrderByDescending(t => t.FechaHora)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transferencia>> GetByCuentaDestinoIdAsync(int cuentaDestinoId)
        {
            return await _context.Transferencias
                .Include(t => t.CuentaOrigen)
                    .ThenInclude(c => c.Cliente)
                .Include(t => t.CuentaDestino)
                    .ThenInclude(c => c.Cliente)
                .Include(t => t.BancoExterno)
                .Include(t => t.Movimientos)
                .Where(t => t.CuentaDestinoId == cuentaDestinoId)
                .OrderByDescending(t => t.FechaHora)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transferencia>> GetByEstadoAsync(EstadoTransferencia estado)
        {
            return await _context.Transferencias
                .Include(t => t.CuentaOrigen)
                    .ThenInclude(c => c.Cliente)
                .Include(t => t.CuentaDestino)
                    .ThenInclude(c => c.Cliente)
                .Include(t => t.BancoExterno)
                .Include(t => t.Movimientos)
                .Where(t => t.Estado == estado)
                .OrderByDescending(t => t.FechaHora)
                .ToListAsync();
        }

        public async Task<Transferencia> CreateTransferenciaInternaAsync(TransferenciaInternaDto transferenciaDto)
        {
            // Validar cuenta origen
            var cuentaOrigen = await _context.Cuentas.FindAsync(transferenciaDto.CuentaOrigenId);
            if (cuentaOrigen == null)
            {
                throw new InvalidOperationException("Cuenta origen no encontrada");
            }

            if (cuentaOrigen.Estado != EstadoCuenta.Activa)
            {
                throw new InvalidOperationException("La cuenta origen no está activa");
            }

            // Validar cuenta destino
            var cuentaDestino = await _context.Cuentas.FindAsync(transferenciaDto.CuentaDestinoId);
            if (cuentaDestino == null)
            {
                throw new InvalidOperationException("Cuenta destino no encontrada");
            }

            if (cuentaDestino.Estado != EstadoCuenta.Activa)
            {
                throw new InvalidOperationException("La cuenta destino no está activa");
            }

            // Validar que no sea la misma cuenta
            if (transferenciaDto.CuentaOrigenId == transferenciaDto.CuentaDestinoId)
            {
                throw new InvalidOperationException("No se puede transferir a la misma cuenta");
            }

            // Validar monto
            if (transferenciaDto.Monto <= 0)
            {
                throw new InvalidOperationException("El monto debe ser mayor a cero");
            }

            // Validar saldo suficiente
            if (cuentaOrigen.Saldo < transferenciaDto.Monto)
            {
                throw new InvalidOperationException("Saldo insuficiente en la cuenta origen");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Crear transferencia
                var transferencia = new Transferencia
                {
                    Monto = transferenciaDto.Monto,
                    FechaHora = DateTime.UtcNow,
                    Descripcion = transferenciaDto.Descripcion,
                    Estado = EstadoTransferencia.Completada,
                    Tipo = TipoTransferencia.Interna,
                    CuentaOrigenId = transferenciaDto.CuentaOrigenId,
                    CuentaDestinoId = transferenciaDto.CuentaDestinoId
                };

                _context.Transferencias.Add(transferencia);
                await _context.SaveChangesAsync();

                // Actualizar saldos
                cuentaOrigen.Saldo -= transferenciaDto.Monto;
                cuentaDestino.Saldo += transferenciaDto.Monto;

                // Crear movimientos
                var movimientoOrigen = new Movimiento
                {
                    CuentaId = transferenciaDto.CuentaOrigenId,
                    Tipo = TipoMovimiento.TransferenciaEnviada,
                    Monto = transferenciaDto.Monto,
                    FechaHora = DateTime.UtcNow,
                    Descripcion = $"Transferencia a cuenta {cuentaDestino.NumeroCuenta}",
                    TransferenciaId = transferencia.Id
                };

                var movimientoDestino = new Movimiento
                {
                    CuentaId = transferenciaDto.CuentaDestinoId,
                    Tipo = TipoMovimiento.TransferenciaRecibida,
                    Monto = transferenciaDto.Monto,
                    FechaHora = DateTime.UtcNow,
                    Descripcion = $"Transferencia desde cuenta {cuentaOrigen.NumeroCuenta}",
                    TransferenciaId = transferencia.Id
                };

                await _movimientoService.CreateAsync(movimientoOrigen);
                await _movimientoService.CreateAsync(movimientoDestino);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return transferencia;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Transferencia> CreateTransferenciaExternaAsync(TransferenciaExternaDto transferenciaDto)
        {
            // Validar cuenta origen
            var cuentaOrigen = await _context.Cuentas.FindAsync(transferenciaDto.CuentaOrigenId);
            if (cuentaOrigen == null)
            {
                throw new InvalidOperationException("Cuenta origen no encontrada");
            }

            if (cuentaOrigen.Estado != EstadoCuenta.Activa)
            {
                throw new InvalidOperationException("La cuenta origen no está activa");
            }

            // Validar banco externo
            var bancoExterno = await _context.BancosExternos.FindAsync(transferenciaDto.BancoExternoId);
            if (bancoExterno == null)
            {
                throw new InvalidOperationException("Banco externo no encontrado");
            }

            if (!bancoExterno.Activo)
            {
                throw new InvalidOperationException("El banco externo no está activo");
            }

            // Validar monto
            if (transferenciaDto.Monto <= 0)
            {
                throw new InvalidOperationException("El monto debe ser mayor a cero");
            }

            // Validar saldo suficiente
            if (cuentaOrigen.Saldo < transferenciaDto.Monto)
            {
                throw new InvalidOperationException("Saldo insuficiente en la cuenta origen");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Crear transferencia
                var transferencia = new Transferencia
                {
                    Monto = transferenciaDto.Monto,
                    FechaHora = DateTime.UtcNow,
                    Descripcion = transferenciaDto.Descripcion,
                    Estado = EstadoTransferencia.Pendiente,
                    Tipo = TipoTransferencia.Externa,
                    CuentaOrigenId = transferenciaDto.CuentaOrigenId,
                    BancoExternoId = transferenciaDto.BancoExternoId,
                    NumeroCuentaDestino = transferenciaDto.NumeroCuentaDestino
                };

                _context.Transferencias.Add(transferencia);
                await _context.SaveChangesAsync();

                // Actualizar saldo origen (bloquear fondos)
                cuentaOrigen.Saldo -= transferenciaDto.Monto;

                // Crear movimiento de origen
                var movimientoOrigen = new Movimiento
                {
                    CuentaId = transferenciaDto.CuentaOrigenId,
                    Tipo = TipoMovimiento.TransferenciaEnviada,
                    Monto = transferenciaDto.Monto,
                    FechaHora = DateTime.UtcNow,
                    Descripcion = $"Transferencia externa a {bancoExterno.Nombre} - Cuenta {transferenciaDto.NumeroCuentaDestino}",
                    TransferenciaId = transferencia.Id
                };

                await _movimientoService.CreateAsync(movimientoOrigen);

                // Intentar enviar a banco externo
                var resultadoExterno = await _bancoExternoService.EnviarTransferenciaAsync(
                    transferenciaDto.BancoExternoId, 
                    transferenciaDto.NumeroCuentaDestino, 
                    transferenciaDto.Monto, 
                    transferenciaDto.Descripcion
                );

                if (resultadoExterno)
                {
                    transferencia.Estado = EstadoTransferencia.Completada;
                    transferencia.ReferenciaExterna = Guid.NewGuid().ToString();
                }
                else
                {
                    transferencia.Estado = EstadoTransferencia.Fallida;
                    // Revertir saldo
                    cuentaOrigen.Saldo += transferenciaDto.Monto;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return transferencia;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> CancelarTransferenciaAsync(int transferenciaId)
        {
            var transferencia = await _context.Transferencias
                .Include(t => t.CuentaOrigen)
                .FirstOrDefaultAsync(t => t.Id == transferenciaId);

            if (transferencia == null)
            {
                return false;
            }

            if (transferencia.Estado != EstadoTransferencia.Pendiente)
            {
                throw new InvalidOperationException("Solo se pueden cancelar transferencias pendientes");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                transferencia.Estado = EstadoTransferencia.Cancelada;
                
                // Si es externa, revertir saldo
                if (transferencia.Tipo == TipoTransferencia.Externa)
                {
                    transferencia.CuentaOrigen.Saldo += transferencia.Monto;
                }

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

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Transferencias.AnyAsync(t => t.Id == id);
        }
    }
}