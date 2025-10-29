using Microsoft.AspNetCore.Mvc;
using BankLink.Models;
using BankLink.Dtos;
using BankLink.interfaces;

namespace BankLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CuentasController : ControllerBase
    {
        private readonly ICuentaService _cuentaService;
        private readonly IClienteService _clienteService;
        private readonly IMovimientoService _movimientoService;

        public CuentasController(
            ICuentaService cuentaService,
            IClienteService clienteService,
            IMovimientoService movimientoService)
        {
            _cuentaService = cuentaService;
            _clienteService = clienteService;
            _movimientoService = movimientoService;
        }

        // GET: api/cuentas
        [HttpGet]
        public ActionResult<List<Cuenta>> GetAll()
        {
            return Ok(_cuentaService.GetAll());
        }

        // GET: api/cuentas/{id}
        [HttpGet("{id}")]
        public ActionResult<Cuenta> GetById(int id)
        {
            var cuenta = _cuentaService.GetById(id);
            if (cuenta != null)
            {
                return Ok(cuenta);
            }
            else
            {
                return NotFound($"No se encontró la cuenta con id: {id}");
            }
        }

        // GET: api/cuentas/numero/{numeroCuenta}
        [HttpGet("numero/{numeroCuenta}")]
        public ActionResult<Cuenta> GetByNumeroCuenta(string numeroCuenta)
        {
            var cuenta = _cuentaService.GetByNumeroCuenta(numeroCuenta);
            if (cuenta != null)
            {
                return Ok(cuenta);
            }
            else
            {
                return NotFound($"No se encontró la cuenta con número: {numeroCuenta}");
            }
        }

        // GET: api/cuentas/cliente/{clienteId}
        [HttpGet("cliente/{clienteId}")]
        public ActionResult<List<Cuenta>> GetByClienteId(int clienteId)
        {
            var cuentas = _cuentaService.GetByClienteId(clienteId);
            return Ok(cuentas);
        }

        // POST: api/cuentas
        [HttpPost]
        public ActionResult<Cuenta> Create([FromBody] CrearCuentaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validar que el cliente existe
            var cliente = _clienteService.GetById(dto.IdClientePropietario);
            if (cliente == null)
            {
                return BadRequest($"No se encontró el cliente con id: {dto.IdClientePropietario}");
            }

            // Generar número de cuenta único
            var numeroCuenta = GenerarNumeroCuenta();

            var cuenta = new Cuenta
            {
                NumeroCuenta = numeroCuenta,
                TipoCuenta = dto.TipoCuenta,
                SaldoActual = dto.SaldoInicial,
                Estado = "Activa",
                IdClientePropietario = dto.IdClientePropietario,
                FechaApertura = DateTime.Now
            };

            var newCuenta = _cuentaService.Create(cuenta);

            // Si hay saldo inicial, registrar movimiento
            if (dto.SaldoInicial > 0)
            {
                var movimiento = new Movimiento
                {
                    IdCuenta = newCuenta.Id,
                    TipoMovimiento = "Depósito",
                    Monto = dto.SaldoInicial,
                    FechaHora = DateTime.Now,
                    Descripcion = "Depósito inicial al crear la cuenta"
                };
                _movimientoService.Create(movimiento);
            }

            return CreatedAtAction(nameof(GetById), new { id = newCuenta.Id }, newCuenta);
        }

        // PUT: api/cuentas/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Cuenta cuenta)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cuentaExistente = _cuentaService.GetById(id);
            if (cuentaExistente == null)
            {
                return NotFound($"No se encontró la cuenta con id: {id}");
            }

            _cuentaService.Update(id, cuenta);
            return NoContent();
        }

        // DELETE: api/cuentas/{id}
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var cuenta = _cuentaService.GetById(id);
            if (cuenta == null)
            {
                return NotFound($"No se encontró la cuenta con id: {id}");
            }

            _cuentaService.Delete(id);
            return NoContent();
        }

        // POST: api/cuentas/deposito
        [HttpPost("deposito")]
        public ActionResult RealizarDeposito([FromBody] DepositoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Validar cuenta
                var cuenta = _cuentaService.GetById(dto.IdCuenta);
                if (cuenta == null)
                {
                    return NotFound($"No se encontró la cuenta con id: {dto.IdCuenta}");
                }

                if (cuenta.Estado != "Activa")
                {
                    return BadRequest("La cuenta no está activa");
                }

                // Actualizar saldo
                cuenta.SaldoActual += dto.Monto;
                _cuentaService.ActualizarSaldo(cuenta.Id, cuenta.SaldoActual);

                // Registrar movimiento
                var movimiento = new Movimiento
                {
                    IdCuenta = cuenta.Id,
                    TipoMovimiento = "Depósito",
                    Monto = dto.Monto,
                    FechaHora = DateTime.Now,
                    Descripcion = dto.Descripcion ?? "Depósito realizado"
                };
                _movimientoService.Create(movimiento);

                return Ok(new
                {
                    message = "Depósito realizado exitosamente",
                    nuevoSaldo = cuenta.SaldoActual,
                    movimiento = movimiento
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al realizar el depósito: {ex.Message}" });
            }
        }

        // POST: api/cuentas/retiro
        [HttpPost("retiro")]
        public ActionResult RealizarRetiro([FromBody] RetiroDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Validar cuenta
                var cuenta = _cuentaService.GetById(dto.IdCuenta);
                if (cuenta == null)
                {
                    return NotFound($"No se encontró la cuenta con id: {dto.IdCuenta}");
                }

                if (cuenta.Estado != "Activa")
                {
                    return BadRequest("La cuenta no está activa");
                }

                // Validar saldo suficiente
                if (cuenta.SaldoActual < dto.Monto)
                {
                    return BadRequest("Saldo insuficiente para realizar el retiro");
                }

                // Actualizar saldo
                cuenta.SaldoActual -= dto.Monto;
                _cuentaService.ActualizarSaldo(cuenta.Id, cuenta.SaldoActual);

                // Registrar movimiento
                var movimiento = new Movimiento
                {
                    IdCuenta = cuenta.Id,
                    TipoMovimiento = "Retiro",
                    Monto = dto.Monto,
                    FechaHora = DateTime.Now,
                    Descripcion = dto.Descripcion ?? "Retiro realizado"
                };
                _movimientoService.Create(movimiento);

                return Ok(new
                {
                    message = "Retiro realizado exitosamente",
                    nuevoSaldo = cuenta.SaldoActual,
                    movimiento = movimiento
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al realizar el retiro: {ex.Message}" });
            }
        }

        // Método auxiliar para generar número de cuenta único
        private string GenerarNumeroCuenta()
        {
            var random = new Random();
            string numeroCuenta;
            do
            {
                numeroCuenta = random.Next(10000000, 99999999).ToString();
            } while (_cuentaService.GetByNumeroCuenta(numeroCuenta) != null);

            return numeroCuenta;
        }
    }
}
