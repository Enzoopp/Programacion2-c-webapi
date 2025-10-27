using Microsoft.AspNetCore.Mvc;
using BankLink.Models;
using BankLink.Context;
using Microsoft.EntityFrameworkCore;

namespace BankLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransferenciasController : ControllerBase
    {
        private readonly BankLinkDbContext _context;

        public TransferenciasController(BankLinkDbContext context)
        {
            _context = context;
        }

        // GET: api/transferencias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transferencia>>> GetTransferencias()
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

        // GET: api/transferencias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Transferencia>> GetTransferencia(int id)
        {
            var transferencia = await _context.Transferencias
                .Include(t => t.CuentaOrigen)
                    .ThenInclude(c => c.Cliente)
                .Include(t => t.CuentaDestino)
                    .ThenInclude(c => c.Cliente)
                .Include(t => t.BancoExterno)
                .Include(t => t.Movimientos)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transferencia == null)
            {
                return NotFound();
            }

            return transferencia;
        }

        // GET: api/transferencias/cuenta-origen/5
        [HttpGet("cuenta-origen/{cuentaId}")]
        public async Task<ActionResult<IEnumerable<Transferencia>>> GetTransferenciasByCuentaOrigen(int cuentaId)
        {
            return await _context.Transferencias
                .Include(t => t.CuentaOrigen)
                    .ThenInclude(c => c.Cliente)
                .Include(t => t.CuentaDestino)
                    .ThenInclude(c => c.Cliente)
                .Include(t => t.BancoExterno)
                .Where(t => t.CuentaOrigenId == cuentaId)
                .OrderByDescending(t => t.FechaHora)
                .ToListAsync();
        }

        // GET: api/transferencias/cuenta-destino/5
        [HttpGet("cuenta-destino/{cuentaId}")]
        public async Task<ActionResult<IEnumerable<Transferencia>>> GetTransferenciasByCuentaDestino(int cuentaId)
        {
            return await _context.Transferencias
                .Include(t => t.CuentaOrigen)
                    .ThenInclude(c => c.Cliente)
                .Include(t => t.CuentaDestino)
                    .ThenInclude(c => c.Cliente)
                .Include(t => t.BancoExterno)
                .Where(t => t.CuentaDestinoId == cuentaId)
                .OrderByDescending(t => t.FechaHora)
                .ToListAsync();
        }

        // POST: api/transferencias/interna
        [HttpPost("interna")]
        public async Task<ActionResult<object>> CreateTransferenciaInterna([FromBody] object transferencia)
        {
            // Esta será implementada cuando tengamos los servicios
            return BadRequest("Funcionalidad no implementada aún. Requiere servicios activos.");
        }

        // POST: api/transferencias/externa
        [HttpPost("externa")]
        public async Task<ActionResult<object>> CreateTransferenciaExterna([FromBody] object transferencia)
        {
            // Esta será implementada cuando tengamos los servicios
            return BadRequest("Funcionalidad no implementada aún. Requiere servicios activos.");
        }

        // PUT: api/transferencias/5/cancelar
        [HttpPut("{id}/cancelar")]
        public async Task<IActionResult> CancelarTransferencia(int id)
        {
            var transferencia = await _context.Transferencias.FindAsync(id);
            if (transferencia == null)
            {
                return NotFound();
            }

            if (transferencia.Estado != EstadoTransferencia.Pendiente)
            {
                return BadRequest("Solo se pueden cancelar transferencias pendientes");
            }

            transferencia.Estado = EstadoTransferencia.Cancelada;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/transferencias/estado/{estado}
        [HttpGet("estado/{estado}")]
        public async Task<ActionResult<IEnumerable<Transferencia>>> GetTransferenciasByEstado(EstadoTransferencia estado)
        {
            return await _context.Transferencias
                .Include(t => t.CuentaOrigen)
                    .ThenInclude(c => c.Cliente)
                .Include(t => t.CuentaDestino)
                    .ThenInclude(c => c.Cliente)
                .Include(t => t.BancoExterno)
                .Where(t => t.Estado == estado)
                .OrderByDescending(t => t.FechaHora)
                .ToListAsync();
        }

        private bool TransferenciaExists(int id)
        {
            return _context.Transferencias.Any(e => e.Id == id);
        }
    }
}