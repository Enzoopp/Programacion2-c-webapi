using Microsoft.AspNetCore.Mvc;
using BankLink.Models;
using BankLink.Context;
using Microsoft.EntityFrameworkCore;

namespace BankLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovimientosController : ControllerBase
    {
        private readonly BankLinkDbContext _context;

        public MovimientosController(BankLinkDbContext context)
        {
            _context = context;
        }

        // GET: api/movimientos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Movimiento>>> GetMovimientos()
        {
            return await _context.Movimientos
                .Include(m => m.Cuenta)
                    .ThenInclude(c => c.Cliente)
                .Include(m => m.Transferencia)
                .OrderByDescending(m => m.FechaHora)
                .ToListAsync();
        }

        // GET: api/movimientos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Movimiento>> GetMovimiento(int id)
        {
            var movimiento = await _context.Movimientos
                .Include(m => m.Cuenta)
                    .ThenInclude(c => c.Cliente)
                .Include(m => m.Transferencia)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movimiento == null)
            {
                return NotFound();
            }

            return movimiento;
        }

        // GET: api/movimientos/cuenta/5
        [HttpGet("cuenta/{cuentaId}")]
        public async Task<ActionResult<IEnumerable<Movimiento>>> GetMovimientosByCuenta(int cuentaId)
        {
            return await _context.Movimientos
                .Include(m => m.Cuenta)
                    .ThenInclude(c => c.Cliente)
                .Include(m => m.Transferencia)
                .Where(m => m.CuentaId == cuentaId)
                .OrderByDescending(m => m.FechaHora)
                .ToListAsync();
        }

        // GET: api/movimientos/cliente/5
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<Movimiento>>> GetMovimientosByCliente(int clienteId)
        {
            return await _context.Movimientos
                .Include(m => m.Cuenta)
                    .ThenInclude(c => c.Cliente)
                .Include(m => m.Transferencia)
                .Where(m => m.Cuenta.ClienteId == clienteId)
                .OrderByDescending(m => m.FechaHora)
                .ToListAsync();
        }

        // GET: api/movimientos/cuenta/5/resumen
        [HttpGet("cuenta/{cuentaId}/resumen")]
        public async Task<ActionResult<object>> GetResumenMovimientos(int cuentaId)
        {
            var movimientos = await _context.Movimientos
                .Where(m => m.CuentaId == cuentaId)
                .ToListAsync();

            var totalDepositos = movimientos
                .Where(m => m.Tipo == TipoMovimiento.Deposito || m.Tipo == TipoMovimiento.TransferenciaRecibida)
                .Sum(m => m.Monto);

            var totalRetiros = movimientos
                .Where(m => m.Tipo == TipoMovimiento.Retiro || m.Tipo == TipoMovimiento.TransferenciaEnviada)
                .Sum(m => m.Monto);

            return new
            {
                CuentaId = cuentaId,
                TotalMovimientos = movimientos.Count,
                TotalDepositos = totalDepositos,
                TotalRetiros = totalRetiros,
                SaldoCalculado = totalDepositos - totalRetiros,
                FechaConsulta = DateTime.UtcNow
            };
        }

        // POST: api/movimientos
        [HttpPost]
        public async Task<ActionResult<Movimiento>> PostMovimiento(Movimiento movimiento)
        {
            movimiento.FechaHora = DateTime.UtcNow;
            
            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMovimiento", new { id = movimiento.Id }, movimiento);
        }

        // PUT: api/movimientos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMovimiento(int id, Movimiento movimiento)
        {
            if (id != movimiento.Id)
            {
                return BadRequest();
            }

            // Solo permitir actualizar la descripción
            var existingMovimiento = await _context.Movimientos.FindAsync(id);
            if (existingMovimiento == null)
            {
                return NotFound();
            }

            existingMovimiento.Descripcion = movimiento.Descripcion;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovimientoExists(id))
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

        private bool MovimientoExists(int id)
        {
            return _context.Movimientos.Any(e => e.Id == id);
        }
    }
}