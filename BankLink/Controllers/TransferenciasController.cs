using Microsoft.AspNetCore.Mvc;
using BankLink.Models;
using BankLink.Dtos;
using BankLink.interfaces;

namespace BankLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransferenciasController : ControllerBase
    {
        private readonly ITransferenciaService _transferenciaService;
        private readonly ICuentaService _cuentaService;

        public TransferenciasController(
            ITransferenciaService transferenciaService,
            ICuentaService cuentaService)
        {
            _transferenciaService = transferenciaService;
            _cuentaService = cuentaService;
        }

        // GET: api/transferencias
        [HttpGet]
        public ActionResult<List<Transferencia>> GetAll()
        {
            return Ok(_transferenciaService.GetAll());
        }

        // GET: api/transferencias/{id}
        [HttpGet("{id}")]
        public ActionResult<Transferencia> GetById(int id)
        {
            var transferencia = _transferenciaService.GetById(id);
            if (transferencia != null)
            {
                return Ok(transferencia);
            }
            else
            {
                return NotFound($"No se encontró la transferencia con id: {id}");
            }
        }

        // GET: api/transferencias/cuenta/{cuentaId}
        [HttpGet("cuenta/{cuentaId}")]
        public ActionResult<List<Transferencia>> GetByCuentaOrigenId(int cuentaId)
        {
            var transferencias = _transferenciaService.GetByCuentaOrigenId(cuentaId);
            return Ok(transferencias);
        }

        // POST: api/transferencias/interna
        [HttpPost("interna")]
        public async Task<ActionResult> RealizarTransferenciaInterna([FromBody] TransferenciaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var transferencia = await _transferenciaService.RealizarTransferenciaInternaAsync(dto);
                
                return Ok(new
                {
                    message = "Transferencia interna realizada exitosamente",
                    transferencia = transferencia
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al realizar la transferencia: {ex.Message}" });
            }
        }

        // POST: api/transferencias/externa
        [HttpPost("externa")]
        public async Task<ActionResult> RealizarTransferenciaExterna([FromBody] TransferenciaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var transferencia = await _transferenciaService.RealizarTransferenciaExternaAsync(dto);
                
                return Ok(new
                {
                    message = "Transferencia externa realizada exitosamente",
                    transferencia = transferencia
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al realizar la transferencia: {ex.Message}" });
            }
        }

        // POST: api/transferencias/recibir
        [HttpPost("recibir")]
        public async Task<ActionResult> RecibirTransferenciaExterna([FromBody] TransferenciaRecibidaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var transferencia = await _transferenciaService.RecibirTransferenciaExternaAsync(dto);
                
                return Ok(new
                {
                    message = "Transferencia externa recibida exitosamente",
                    transferencia = transferencia
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al recibir la transferencia: {ex.Message}" });
            }
        }

        // POST: api/transferencias/automatica
        [HttpPost("automatica")]
        public async Task<ActionResult> RealizarTransferenciaAutomatica([FromBody] TransferenciaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Determinar si es interna o externa
                var cuentaDestino = _cuentaService.GetByNumeroCuenta(dto.NumeroCuentaDestino);
                
                Transferencia transferencia;
                string tipoTransferencia;

                if (cuentaDestino != null)
                {
                    // Es una transferencia interna
                    transferencia = await _transferenciaService.RealizarTransferenciaInternaAsync(dto);
                    tipoTransferencia = "interna";
                }
                else if (dto.IdBancoDestino.HasValue)
                {
                    // Es una transferencia externa
                    transferencia = await _transferenciaService.RealizarTransferenciaExternaAsync(dto);
                    tipoTransferencia = "externa";
                }
                else
                {
                    return BadRequest(new { message = "La cuenta destino no existe y no se especificó un banco externo" });
                }

                return Ok(new
                {
                    message = $"Transferencia {tipoTransferencia} realizada exitosamente",
                    tipo = tipoTransferencia,
                    transferencia = transferencia
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al realizar la transferencia: {ex.Message}" });
            }
        }
    }
}
