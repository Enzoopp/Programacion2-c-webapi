using BankLink.interfaces;
using BankLink.Dtos;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace BankLink.Service
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        
        // Usuarios de prueba (en producción esto vendría de base de datos)
        private readonly List<UsuarioDto> _usuarios = new()
        {
            new UsuarioDto 
            { 
                Username = "admin", 
                Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "Admin"
            },
            new UsuarioDto 
            { 
                Username = "operator", 
                Password = BCrypt.Net.BCrypt.HashPassword("oper123"),
                Role = "Operator"
            }
        };

        // API Keys de prueba
        private readonly List<string> _apiKeys = new()
        {
            "BL2024-API-KEY-001",
            "BL2024-API-KEY-002",
            "BL2024-API-KEY-003"
        };

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            await Task.Delay(1); // Para hacer el método async

            var usuario = _usuarios.FirstOrDefault(u => u.Username == loginDto.Username);
            
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, usuario.Password))
            {
                throw new UnauthorizedAccessException("Credenciales inválidas");
            }

            var token = GenerarJwtToken(usuario);
            
            return new LoginResponseDto
            {
                Token = token,
                Username = usuario.Username,
                Role = usuario.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }

        public async Task<bool> ValidarApiKeyAsync(string apiKey)
        {
            await Task.Delay(1); // Para hacer el método async
            return _apiKeys.Contains(apiKey);
        }

        public async Task<bool> ValidarTokenAsync(string token)
        {
            try
            {
                await Task.Delay(1); // Para hacer el método async

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "BankLinkSecretKey2024ForDevelopment123456789");
                
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GenerarApiKeyAsync(string usuario)
        {
            await Task.Delay(1); // Para hacer el método async
            
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var apiKey = $"BL{timestamp}-{usuario.ToUpper()}-{Guid.NewGuid().ToString("N")[..8]}";
            
            // En producción, esto se guardaría en base de datos
            _apiKeys.Add(apiKey);
            
            return apiKey;
        }

        public async Task<ClaimsPrincipal> GetClaimsFromTokenAsync(string token)
        {
            try
            {
                await Task.Delay(1); // Para hacer el método async

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "BankLinkSecretKey2024ForDevelopment123456789");
                
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                throw new UnauthorizedAccessException("Token inválido");
            }
        }

        private string GenerarJwtToken(UsuarioDto usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "BankLinkSecretKey2024ForDevelopment123456789");
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, usuario.Username),
                    new Claim(ClaimTypes.Role, usuario.Role),
                    new Claim("username", usuario.Username)
                }),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}