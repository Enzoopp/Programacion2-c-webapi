using Microsoft.AspNetCore.Mvc;
using BankLink.interfaces;
using BankLink.Dtos;

namespace BankLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IClienteService _clienteService;

        public AuthController(IAuthService authService, IClienteService clienteService)
        {
            _authService = authService;
            _clienteService = clienteService;
        }

        [HttpPost("login")]
        public ActionResult<object> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = _authService.Login(loginDto);
            if (token == null)
            {
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
            }

            return Ok(new { token = token, message = "Login exitoso" });
        }

        [HttpPost("register")]
        public ActionResult Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar si el usuario ya existe
            var usuarioExistente = _clienteService.GetByNombreUsuario(registerDto.NombreUsuario);
            if (usuarioExistente != null)
            {
                return BadRequest(new { message = "El nombre de usuario ya está en uso" });
            }

            // Verificar si el DNI ya existe
            var dniExistente = _clienteService.GetByDni(registerDto.Dni);
            if (dniExistente != null)
            {
                return BadRequest(new { message = "El DNI ya está registrado" });
            }

            // Crear el cliente
            var cliente = new BankLink.Models.Cliente
            {
                Nombre = registerDto.Nombre,
                Apellido = registerDto.Apellido,
                Dni = registerDto.Dni,
                Direccion = registerDto.Direccion,
                Telefono = registerDto.Telefono,
                Email = registerDto.Email,
                NombreUsuario = registerDto.NombreUsuario,
                PassHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Contraseña),
                Rol = registerDto.Rol
            };

            _clienteService.Create(cliente);

            return Ok(new { message = "Cliente registrado exitosamente" });
        }
    }
}
