// ============================================================================
// AUTHSERVICE.CS - Servicio de Autenticación con JWT
// ============================================================================
// Este servicio maneja:
// 1. Login de usuarios (validación de credenciales)
// 2. Generación de tokens JWT (JSON Web Tokens)
//
// SEGURIDAD:
// - Las contraseñas se hashean con BCrypt (nunca se guardan en texto plano)
// - Los tokens JWT están firmados con clave secreta
// - Los tokens expiran después de 60 minutos
// ============================================================================

using System.IdentityModel.Tokens.Jwt;  // Para crear y manipular tokens JWT
using System.Security.Claims;            // Para agregar información (claims) al token
using System.Text;                       // Para convertir strings a bytes
using Microsoft.Extensions.Options;      // Para inyectar configuración
using Microsoft.IdentityModel.Tokens;    // Para claves y credenciales de firma
using BankLink.Dtos;
using BankLink.interfaces;
using BankLink.Models;

namespace BankLink.Service
{
    /// <summary>
    /// Servicio de autenticación que maneja login y generación de tokens JWT
    /// </summary>
    public class AuthService : IAuthService
    {
        // ====================================================================
        // DEPENDENCIAS INYECTADAS
        // ====================================================================
        private readonly AuthOptions _options;  // Configuración JWT (desde appsettings.json)
        private readonly IClienteService _clienteService;  // Para buscar usuarios

        /// <summary>
        /// Constructor: recibe configuración y servicio de clientes
        /// </summary>
        public AuthService(IOptions<AuthOptions> options, IClienteService clienteService)
        {
            // IOptions<T> es el patrón de configuración de ASP.NET Core
            // .Value extrae el objeto AuthOptions del wrapper
            _options = options.Value;
            _clienteService = clienteService;
        }

        // ====================================================================
        // MÉTODO: CREAR TOKEN JWT
        // ====================================================================
        /// <summary>
        /// Crea un token JWT firmado con información del usuario
        /// 
        /// ¿QUÉ ES UN JWT?
        /// Es un token compuesto por 3 partes separadas por puntos:
        /// HEADER.PAYLOAD.SIGNATURE
        /// 
        /// HEADER: Tipo de token y algoritmo de firma
        /// PAYLOAD: Claims (información del usuario)
        /// SIGNATURE: Firma criptográfica para validar autenticidad
        /// </summary>
        /// <param name="createTokenDto">Información del usuario para incluir en el token</param>
        /// <returns>String del token JWT</returns>
        public string CreateToken(CreateTokenDto createTokenDto)
        {
            // ================================================================
            // PASO 1: CREAR CLAIMS (Información que va dentro del token)
            // ================================================================
            // Los claims son pares clave-valor que viajan en el payload del JWT
            // El servidor puede leer estos claims para identificar al usuario
            var claims = new List<Claim>
            {
                // ============================================================
                // CLAIM 1: Sub (Subject) - ID del usuario
                // ============================================================
                // Este es el identificador único del usuario en la BD
                new(JwtRegisteredClaimNames.Sub, createTokenDto.Id.ToString()),
                
                // ============================================================
                // CLAIM 2: UniqueName - Nombre de usuario para login
                // ============================================================
                new(JwtRegisteredClaimNames.UniqueName, createTokenDto.NombreUsuario),
                
                // ============================================================
                // CLAIM 3: Name - Nombre real del usuario
                // ============================================================
                // Se usa para mostrar "Hola, Juan" en la UI
                new(ClaimTypes.Name, createTokenDto.Nombre),
                
                // ============================================================
                // CLAIM 4: Role - Rol del usuario (Cliente, Admin, etc.)
                // ============================================================
                // ASP.NET Core usa este claim para autorización
                // [Authorize(Roles = "Admin")] valida este claim
                new(ClaimTypes.Role, createTokenDto.Rol)
            };

            // ================================================================
            // PASO 2: CREAR CLAVE SIMÉTRICA DE FIRMA
            // ================================================================
            // La clave simétrica se usa para FIRMAR y VALIDAR el token
            // Debe mantenerse SECRETA en el servidor
            // Convertimos el string de configuración a bytes UTF-8
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));

            // ================================================================
            // PASO 3: CREAR CREDENCIALES DE FIRMA
            // ================================================================
            // SigningCredentials combina:
            // - La clave secreta
            // - El algoritmo de firma (HmacSha256 es el estándar para JWT)
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // ================================================================
            // PASO 4: ESTABLECER TIEMPOS DE VALIDEZ
            // ================================================================
            // expires: Cuándo deja de ser válido el token
            // Después de este tiempo, el usuario debe hacer login nuevamente
            var expires = DateTime.UtcNow.AddMinutes(_options.ExpMinutes);  // 60 minutos
            
            // notBefore: Cuándo empieza a ser válido el token
            // Normalmente es "ahora" (DateTime.UtcNow)
            var notBefore = DateTime.UtcNow;

            // ================================================================
            // PASO 5: CREAR EL TOKEN JWT
            // ================================================================
            // JwtSecurityToken es el objeto que representa el token completo
            var token = new JwtSecurityToken(
                // Issuer: Quién emitió el token (nuestra API)
                issuer: _options.Issuer,  // "BankLinkAPI"
                
                // Audience: Para quién es el token (también nuestra API)
                audience: _options.Audience,  // "BankLinkAPI"
                
                // Claims: La información del usuario que va en el payload
                claims: claims,
                
                // Expires: Fecha de expiración
                expires: expires,
                
                // NotBefore: Fecha desde cuándo es válido
                notBefore: notBefore,
                
                // SigningCredentials: Cómo se firma el token
                signingCredentials: creds
            );

            // ================================================================
            // PASO 6: SERIALIZAR EL TOKEN A STRING
            // ================================================================
            // JwtSecurityTokenHandler convierte el objeto token a string
            // El resultado es el JWT en formato: HEADER.PAYLOAD.SIGNATURE
            // Este string es lo que se envía al cliente
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ====================================================================
        // MÉTODO: LOGIN (Validar credenciales y generar token)
        // ====================================================================
        /// <summary>
        /// Valida las credenciales del usuario y genera un token JWT si son correctas
        /// 
        /// FLUJO:
        /// 1. Buscar usuario por nombre de usuario
        /// 2. Verificar contraseña con BCrypt
        /// 3. Si es válido, generar token JWT
        /// 4. Si no es válido, retornar null
        /// </summary>
        /// <param name="loginDto">Credenciales del usuario (usuario y contraseña)</param>
        /// <returns>Token JWT si login exitoso, null si falla</returns>
        public string Login(LoginDto loginDto)
        {
            // ================================================================
            // PASO 1: BUSCAR CLIENTE POR NOMBRE DE USUARIO
            // ================================================================
            // Buscar en la base de datos si existe un usuario con ese nombre
            var cliente = _clienteService.GetByNombreUsuario(loginDto.NombreUsuario);

            // ================================================================
            // PASO 2: VALIDAR EXISTENCIA Y CONTRASEÑA
            // ================================================================
            // Verificamos DOS cosas:
            // 1. Que el cliente exista (cliente != null)
            // 2. Que la contraseña sea correcta (BCrypt.Verify)
            
            // BCrypt.Verify compara:
            // - loginDto.Contraseña: La contraseña en texto plano que envió el usuario
            // - cliente.PassHash: El hash almacenado en la base de datos
            // 
            // BCrypt es unidireccional: NO se puede obtener la contraseña original
            // Solo se puede verificar si una contraseña coincide con el hash
            if (cliente != null && BCrypt.Net.BCrypt.Verify(loginDto.Contraseña, cliente.PassHash))
            {
                // ============================================================
                // LOGIN EXITOSO: Generar token JWT
                // ============================================================
                // Creamos un DTO con la información que va en el token
                var createTokenDto = new CreateTokenDto(
                    cliente.NombreUsuario,  // Para claim UniqueName
                    cliente.Id,             // Para claim Sub
                    cliente.Nombre,         // Para claim Name
                    cliente.Rol             // Para claim Role
                );
                
                // Generar y retornar el token
                return CreateToken(createTokenDto);
            }
            
            // ================================================================
            // LOGIN FALLIDO: Retornar null
            // ================================================================
            // Si el usuario no existe O la contraseña es incorrecta, retornar null
            // El controller interpretará null como credenciales inválidas
            // y retornará HTTP 401 Unauthorized
            return null;
        }
    }
}

// ============================================================================
// NOTAS PARA LA PRESENTACIÓN:
// ============================================================================
// 1. BCrypt es un algoritmo de hashing diseñado para contraseñas
//    - Es lento intencionalmente (previene ataques de fuerza bruta)
//    - Incluye "salt" automáticamente (previene rainbow tables)
//    - Es unidireccional (no se puede revertir)
//
// 2. JWT es un estándar de autenticación sin estado (stateless)
//    - El servidor NO guarda tokens en la BD
//    - El token viaja en el header: Authorization: Bearer <token>
//    - El servidor valida la firma para verificar autenticidad
//
// 3. Los Claims permiten almacenar información del usuario en el token
//    - El servidor puede leer claims sin consultar la BD
//    - Útil para autorización basada en roles
//    - No incluir información sensible (ej: contraseñas, tarjetas)
//
// 4. La clave secreta (_options.Key) es CRÍTICA
//    - Debe ser larga y aleatoria
//    - NUNCA commitear al repositorio
//    - En producción, usar variables de entorno
// ============================================================================
