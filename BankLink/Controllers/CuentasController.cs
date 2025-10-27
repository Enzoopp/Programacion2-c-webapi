using Microsoft.AspNetCore.Mvc;
using BankLink.Models;
using BankLink.Context;
using Microsoft.EntityFrameworkCore;

namespace BankLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CuentasController : ControllerBase
    {
        private readonly BankLinkDbContext _context;

        public CuentasController(BankLinkDbContext context)
        {
            _context = context;
        }

        // GET: api/cuentas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cuenta>>> GetCuentas()
        {
            return await _context.Cuentas
                .Include(c => c.Cliente)
                .Include(c => c.Movimientos)
                .ToListAsync();
        }

        // GET: api/cuentas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cuenta>> GetCuenta(int id)
        {
            var cuenta = await _context.Cuentas
                .Include(c => c.Cliente)
                .Include(c => c.Movimientos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cuenta == null)
            {
                return NotFound();
            }

            return cuenta;
        }

        // GET: api/cuentas/cliente/5
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<Cuenta>>> GetCuentasByCliente(int clienteId)
        {
            return await _context.Cuentas
                .Include(c => c.Cliente)
                .Include(c => c.Movimientos)
                .Where(c => c.ClienteId == clienteId)
                .ToListAsync();
        }

        // GET: api/cuentas/5/saldo
        [HttpGet("{id}/saldo")]
        public async Task<ActionResult<object>> GetSaldo(int id)
        {
            var cuenta = await _context.Cuentas.FindAsync(id);
            if (cuenta == null)
            {
                return NotFound();
            }

            return new { CuentaId = id, Saldo = cuenta.Saldo, Fecha = DateTime.UtcNow };
        }

        // POST: api/cuentas
        [HttpPost]
        public async Task<ActionResult<Cuenta>> PostCuenta(Cuenta cuenta)
        {
            cuenta.FechaCreacion = DateTime.UtcNow;
            cuenta.Estado = EstadoCuenta.Activa;
            
            _context.Cuentas.Add(cuenta);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCuenta", new { id = cuenta.Id }, cuenta);
        }

        // PUT: api/cuentas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCuenta(int id, Cuenta cuenta)
        {
            if (id != cuenta.Id)
            {
                return BadRequest();
            }

            _context.Entry(cuenta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CuentaExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/cuentas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCuenta(int id)
        {
            var cuenta = await _context.Cuentas.FindAsync(id);
            if (cuenta == null)
            {
                return NotFound();
            }

            if (cuenta.Saldo != 0)
            {
                return BadRequest("No se puede eliminar una cuenta con saldo diferente a cero");
            }

            _context.Cuentas.Remove(cuenta);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/cuentas/5/deposito
        [HttpPost("{id}/deposito")]
        public async Task<ActionResult<object>> Depositar(int id, [FromBody] DepositoRequest request)
        {
            var cuenta = await _context.Cuentas
                .Include(c => c.Cliente)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (cuenta == null)
            {
                return NotFound(new { Error = "Cuenta no encontrada" });
            }

            if (cuenta.Estado != EstadoCuenta.Activa)
            {
                return BadRequest(new { Error = "La cuenta no está activa" });
            }

            if (request.Monto <= 0)
            {
                return BadRequest(new { Error = "El monto debe ser mayor a cero" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Actualizar saldo
                cuenta.Saldo += request.Monto;
                
                // Crear movimiento
                var movimiento = new Movimiento
                {
                    CuentaId = id,
                    Tipo = TipoMovimiento.Deposito,
                    Monto = request.Monto,
                    FechaHora = DateTime.UtcNow,
                    Descripcion = request.Descripcion ?? "Depósito"
                };

                _context.Movimientos.Add(movimiento);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return Ok(new 
                { 
                    Exito = true,
                    Mensaje = "Depósito realizado exitosamente",
                    NuevoSaldo = cuenta.Saldo,
                    MovimientoId = movimiento.Id,
                    FechaOperacion = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Error = "Error interno del servidor", Detalle = ex.Message });
            }
        }

        // POST: api/cuentas/5/retiro
        [HttpPost("{id}/retiro")]
        public async Task<ActionResult<object>> Retirar(int id, [FromBody] RetiroRequest request)
        {
            var cuenta = await _context.Cuentas
                .Include(c => c.Cliente)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (cuenta == null)
            {
                return NotFound(new { Error = "Cuenta no encontrada" });
            }

            if (cuenta.Estado != EstadoCuenta.Activa)
            {
                return BadRequest(new { Error = "La cuenta no está activa" });
            }

            if (request.Monto <= 0)
            {
                return BadRequest(new { Error = "El monto debe ser mayor a cero" });
            }

            if (cuenta.Saldo < request.Monto)
            {
                return BadRequest(new { 
                    Error = "Saldo insuficiente", 
                    SaldoDisponible = cuenta.Saldo,
                    MontoSolicitado = request.Monto
                });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Actualizar saldo
                cuenta.Saldo -= request.Monto;
                
                // Crear movimiento
                var movimiento = new Movimiento
                {
                    CuentaId = id,
                    Tipo = TipoMovimiento.Retiro,
                    Monto = request.Monto,
                    FechaHora = DateTime.UtcNow,
                    Descripcion = request.Descripcion ?? "Retiro"
                };

                _context.Movimientos.Add(movimiento);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return Ok(new 
                { 
                    Exito = true,
                    Mensaje = "Retiro realizado exitosamente",
                    NuevoSaldo = cuenta.Saldo,
                    MovimientoId = movimiento.Id,
                    FechaOperacion = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Error = "Error interno del servidor", Detalle = ex.Message });
            }
        }

        private bool CuentaExists(int id)
        {
            return _context.Cuentas.Any(e => e.Id == id);
        }
    }
}