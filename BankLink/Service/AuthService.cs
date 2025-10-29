using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using BankLink.Dtos;
using BankLink.interfaces;
using BankLink.Models;

namespace BankLink.Service
{
    public class AuthService : IAuthService
    {
        private readonly AuthOptions _options;
        private readonly IClienteService _clienteService;

        public AuthService(IOptions<AuthOptions> options, IClienteService clienteService)
        {
            _options = options.Value;
            _clienteService = clienteService;
        }

        public string CreateToken(CreateTokenDto createTokenDto)
        {
            var claims = new List<Claim>
            {
                // Id del usuario
                new(JwtRegisteredClaimNames.Sub, createTokenDto.Id.ToString()),
                // Nombre de login del usuario
                new(JwtRegisteredClaimNames.UniqueName, createTokenDto.NombreUsuario),
                // Nombre del usuario
                new(ClaimTypes.Name, createTokenDto.Nombre),
                // Rol del usuario
                new(ClaimTypes.Role, createTokenDto.Rol)
            };

            // --- Crear la CLAVE de firma ---
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));

            // --- Crear las CREDENCIALES ---
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // --- Establecer tiempos de validez ---
            var expires = DateTime.UtcNow.AddMinutes(_options.ExpMinutes);
            var notBefore = DateTime.UtcNow;

            // --- Crear el TOKEN ---
            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: expires,
                notBefore: notBefore,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string Login(LoginDto loginDto)
        {
            var cliente = _clienteService.GetByNombreUsuario(loginDto.NombreUsuario);

            // Verificar si la contraseña coincide con el hash almacenado
            if (cliente != null && BCrypt.Net.BCrypt.Verify(loginDto.Contraseña, cliente.PassHash))
            {
                return CreateToken(new CreateTokenDto(cliente.NombreUsuario, cliente.Id, cliente.Nombre, cliente.Rol));
            }
            // Si la contraseña no coincide o el usuario no existe se retorna null
            return null;
        }
    }
}
