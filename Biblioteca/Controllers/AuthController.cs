using Microsoft.AspNetCore.Mvc;
using Biblioteca.interfaces;
using Biblioteca.Dtos;


namespace Biblioteca.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
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

        // Aquí puedes implementar la lógica de registro
        return Ok(new { message = "Usuario registrado exitosamente" });
    }
}