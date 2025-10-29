using Microsoft.AspNetCore.Mvc;
using BankLink.Models;
using BankLink.interfaces;

namespace BankLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovimientosController : ControllerBase
    {
        private readonly IMovimientoService _movimientoService;

        public MovimientosController(IMovimientoService movimientoService)
        {
            _movimientoService = movimientoService;
        }

        // GET: api/movimientos
        [HttpGet]
        public ActionResult<List<Movimiento>> GetAll()
        {
            return Ok(_movimientoService.GetAll());
        }

        // GET: api/movimientos/{id}
        [HttpGet("{id}")]
        public ActionResult<Movimiento> GetById(int id)
        {
            var movimiento = _movimientoService.GetById(id);
            if (movimiento != null)
            {
                return Ok(movimiento);
            }
            else
            {
                return NotFound($"No se encontró el movimiento con id: {id}");
            }
        }

        // GET: api/movimientos/cuenta/{cuentaId}
        [HttpGet("cuenta/{cuentaId}")]
        public ActionResult<List<Movimiento>> GetByCuentaId(int cuentaId)
        {
            var movimientos = _movimientoService.GetByCuentaId(cuentaId);
            return Ok(movimientos);
        }

        // DELETE: api/movimientos/{id}
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var movimiento = _movimientoService.GetById(id);
            if (movimiento == null)
            {
                return NotFound($"No se encontró el movimiento con id: {id}");
            }

            _movimientoService.Delete(id);
            return NoContent();
        }
    }
}
