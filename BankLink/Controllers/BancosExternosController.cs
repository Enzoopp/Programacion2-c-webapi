using Microsoft.AspNetCore.Mvc;
using BankLink.Models;
using BankLink.Context;
using Microsoft.EntityFrameworkCore;

namespace BankLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BancosExternosController : ControllerBase
    {
        private readonly BankLinkDbContext _context;

        public BancosExternosController(BankLinkDbContext context)
        {
            _context = context;
        }

        // GET: api/bancosexternos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BancoExterno>>> GetBancosExternos()
        {
            return await _context.BancosExternos
                .Include(b => b.TransferenciasRecibidas)
                .ToListAsync();
        }

        // GET: api/bancosexternos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BancoExterno>> GetBancoExterno(int id)
        {
            var bancoExterno = await _context.BancosExternos
                .Include(b => b.TransferenciasRecibidas)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bancoExterno == null)
            {
                return NotFound();
            }

            return bancoExterno;
        }

        // GET: api/bancosexternos/activos
        [HttpGet("activos")]
        public async Task<ActionResult<IEnumerable<BancoExterno>>> GetBancosExternosActivos()
        {
            return await _context.BancosExternos
                .Where(b => b.Activo)
                .ToListAsync();
        }

        // GET: api/bancosexternos/codigo/{codigo}
        [HttpGet("codigo/{codigo}")]
        public async Task<ActionResult<BancoExterno>> GetBancoExternoByCodigo(string codigo)
        {
            var bancoExterno = await _context.BancosExternos
                .Include(b => b.TransferenciasRecibidas)
                .FirstOrDefaultAsync(b => b.CodigoIdentificacion == codigo);

            if (bancoExterno == null)
            {
                return NotFound();
            }

            return bancoExterno;
        }

        // POST: api/bancosexternos
        [HttpPost]
        public async Task<ActionResult<BancoExterno>> PostBancoExterno(BancoExterno bancoExterno)
        {
            bancoExterno.FechaCreacion = DateTime.UtcNow;
            bancoExterno.Activo = true;
            
            _context.BancosExternos.Add(bancoExterno);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBancoExterno", new { id = bancoExterno.Id }, bancoExterno);
        }

        // PUT: api/bancosexternos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBancoExterno(int id, BancoExterno bancoExterno)
        {
            if (id != bancoExterno.Id)
            {
                return BadRequest();
            }

            _context.Entry(bancoExterno).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BancoExternoExists(id))
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

        // PUT: api/bancosexternos/5/activar
        [HttpPut("{id}/activar")]
        public async Task<IActionResult> ActivarBancoExterno(int id)
        {
            var bancoExterno = await _context.BancosExternos.FindAsync(id);
            if (bancoExterno == null)
            {
                return NotFound();
            }

            bancoExterno.Activo = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/bancosexternos/5/desactivar
        [HttpPut("{id}/desactivar")]
        public async Task<IActionResult> DesactivarBancoExterno(int id)
        {
            var bancoExterno = await _context.BancosExternos.FindAsync(id);
            if (bancoExterno == null)
            {
                return NotFound();
            }

            bancoExterno.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/bancosexternos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBancoExterno(int id)
        {
            var bancoExterno = await _context.BancosExternos
                .Include(b => b.TransferenciasRecibidas)
                .FirstOrDefaultAsync(b => b.Id == id);
            
            if (bancoExterno == null)
            {
                return NotFound();
            }

            // Verificar que no tenga transferencias pendientes
            if (bancoExterno.TransferenciasRecibidas.Any(t => t.Estado == EstadoTransferencia.Pendiente))
            {
                return BadRequest("No se puede eliminar un banco con transferencias pendientes");
            }

            _context.BancosExternos.Remove(bancoExterno);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/bancosexternos/5/conexion
        [HttpGet("{id}/conexion")]
        public async Task<ActionResult<object>> ValidarConexion(int id)
        {
            var bancoExterno = await _context.BancosExternos.FindAsync(id);
            if (bancoExterno == null)
            {
                return NotFound();
            }

            // Simulación de validación de conexión
            var esConexionValida = bancoExterno.Activo && !string.IsNullOrEmpty(bancoExterno.UrlBase);

            return new
            {
                BancoId = id,
                Nombre = bancoExterno.Nombre,
                ConexionValida = esConexionValida,
                FechaPrueba = DateTime.UtcNow,
                Mensaje = esConexionValida ? "Conexión exitosa" : "Error de conexión"
            };
        }

        private bool BancoExternoExists(int id)
        {
            return _context.BancosExternos.Any(e => e.Id == id);
        }
    }
}