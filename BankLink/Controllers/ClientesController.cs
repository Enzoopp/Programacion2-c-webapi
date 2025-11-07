using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BankLink.Models;
using BankLink.interfaces;

namespace BankLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // 🔒 REQUIERE AUTENTICACIÓN JWT
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        // GET: api/clientes
        [HttpGet]
        public ActionResult<List<Cliente>> GetAll()
        {
            return Ok(_clienteService.GetAll());
        }

        // GET: api/clientes/{id}
        [HttpGet("{id}")]
        public ActionResult<Cliente> GetById(int id)
        {
            var cliente = _clienteService.GetById(id);
            if (cliente != null)
            {
                return Ok(cliente);
            }
            else
            {
                return NotFound($"No se encontró el cliente con id: {id}");
            }
        }

        // GET: api/clientes/dni/{dni}
        [HttpGet("dni/{dni}")]
        public ActionResult<Cliente> GetByDni(string dni)
        {
            var cliente = _clienteService.GetByDni(dni);
            if (cliente != null)
            {
                return Ok(cliente);
            }
            else
            {
                return NotFound($"No se encontró el cliente con DNI: {dni}");
            }
        }

        // POST: api/clientes
        [HttpPost]
        public ActionResult<Cliente> Create([FromBody] Cliente cliente)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Hash de contraseña si viene sin hashear
            if (!string.IsNullOrEmpty(cliente.PassHash) && !cliente.PassHash.StartsWith("$2"))
            {
                cliente.PassHash = BCrypt.Net.BCrypt.HashPassword(cliente.PassHash);
            }

            var newCliente = _clienteService.Create(cliente);

            return CreatedAtAction(nameof(GetById), new { id = newCliente.Id }, newCliente);
        }

        // PUT: api/clientes/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Cliente cliente)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var clienteExistente = _clienteService.GetById(id);
            if (clienteExistente == null)
            {
                return NotFound($"No se encontró el cliente con id: {id}");
            }

            _clienteService.Update(id, cliente);
            return NoContent();
        }

        // DELETE: api/clientes/{id}
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var cliente = _clienteService.GetById(id);
            if (cliente == null)
            {
                return NotFound($"No se encontró el cliente con id: {id}");
            }

            _clienteService.Delete(id);
            return NoContent();
        }
    }
}
