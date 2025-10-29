using Microsoft.AspNetCore.Mvc;
using BankLink.Models;
using BankLink.interfaces;

namespace BankLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BancosExternosController : ControllerBase
    {
        private readonly IBancoExternoService _bancoExternoService;

        public BancosExternosController(IBancoExternoService bancoExternoService)
        {
            _bancoExternoService = bancoExternoService;
        }

        // GET: api/bancosexternos
        [HttpGet]
        public ActionResult<List<BancoExterno>> GetAll()
        {
            return Ok(_bancoExternoService.GetAll());
        }

        // GET: api/bancosexternos/{id}
        [HttpGet("{id}")]
        public ActionResult<BancoExterno> GetById(int id)
        {
            var banco = _bancoExternoService.GetById(id);
            if (banco != null)
            {
                return Ok(banco);
            }
            else
            {
                return NotFound($"No se encontró el banco externo con id: {id}");
            }
        }

        // GET: api/bancosexternos/codigo/{codigo}
        [HttpGet("codigo/{codigo}")]
        public ActionResult<BancoExterno> GetByCodigoIdentificacion(string codigo)
        {
            var banco = _bancoExternoService.GetByCodigoIdentificacion(codigo);
            if (banco != null)
            {
                return Ok(banco);
            }
            else
            {
                return NotFound($"No se encontró el banco externo con código: {codigo}");
            }
        }

        // POST: api/bancosexternos
        [HttpPost]
        public ActionResult<BancoExterno> Create([FromBody] BancoExterno banco)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newBanco = _bancoExternoService.Create(banco);

            return CreatedAtAction(nameof(GetById), new { id = newBanco.Id }, newBanco);
        }

        // PUT: api/bancosexternos/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] BancoExterno banco)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bancoExistente = _bancoExternoService.GetById(id);
            if (bancoExistente == null)
            {
                return NotFound($"No se encontró el banco externo con id: {id}");
            }

            _bancoExternoService.Update(id, banco);
            return NoContent();
        }

        // DELETE: api/bancosexternos/{id}
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var banco = _bancoExternoService.GetById(id);
            if (banco == null)
            {
                return NotFound($"No se encontró el banco externo con id: {id}");
            }

            _bancoExternoService.Delete(id);
            return NoContent();
        }
    }
}
