// ============================================================================
// PROGRAM.CS - Punto de entrada y configuración de la aplicación BankLink
// ============================================================================
// Este archivo configura todos los servicios, middleware y dependencias
// necesarios para que la API REST funcione correctamente.
// ============================================================================

using BankLink.interfaces;      // Interfaces de servicios (contratos)
using BankLink.Service;          // Implementaciones de servicios
using BankLink.Context;          // DbContext de Entity Framework
using BankLink.Models;           // Modelos/Entidades
using Microsoft.EntityFrameworkCore;                    // ORM para SQL Server
using Microsoft.AspNetCore.Authentication.JwtBearer;   // Autenticación JWT
using Microsoft.IdentityModel.Tokens;                   // Tokens y seguridad
using System.Text;               // Para encoding de strings

// ============================================================================
// SECCIÓN 1: CREAR EL BUILDER DE LA APLICACIÓN
// ============================================================================
// WebApplicationBuilder es el objeto que configura todos los servicios
var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// SECCIÓN 2: CONFIGURACIÓN DE BASE DE DATOS
// ============================================================================
// Obtener la cadena de conexión desde appsettings.json
// Formato: Server=.\SQLEXPRESS;Database=BankLinkDb;Trusted_Connection=True
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Registrar el DbContext en el contenedor de inyección de dependencias
// AddDbContext hace que BankLinkDbContext esté disponible en controllers y services
builder.Services.AddDbContext<BankLinkDbContext>(options =>
    // UseSqlServer configura EF Core para usar SQL Server
    options.UseSqlServer(connectionString));

// ============================================================================
// SECCIÓN 3: CONFIGURACIÓN DE CONTROLLERS Y SERIALIZACIÓN JSON
// ============================================================================
// AddControllers registra todos los controladores de la carpeta Controllers/
builder.Services.AddControllers()
    // Configurar opciones de serialización JSON
    .AddJsonOptions(options =>
    {
        // CRÍTICO: Prevenir bucles infinitos en referencias circulares
        // Ejemplo: Cliente -> Cuentas -> Cliente -> Cuentas... (infinito)
        // IgnoreCycles corta el ciclo y evita el error 500
        options.JsonSerializerOptions.ReferenceHandler = 
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// ============================================================================
// SECCIÓN 4: CONFIGURACIÓN DE SWAGGER (Documentación de API)
// ============================================================================
// AddEndpointsApiExplorer: Permite que Swagger descubra los endpoints
builder.Services.AddEndpointsApiExplorer();

// AddSwaggerGen: Genera la documentación interactiva en /swagger
builder.Services.AddSwaggerGen();

// ============================================================================
// SECCIÓN 5: CONFIGURACIÓN DE AUTENTICACIÓN JWT
// ============================================================================
// Mapear la sección "AuthOptions" del appsettings.json a la clase AuthOptions
// Esto permite acceder a Issuer, Key, ExpMinutes, etc.
builder.Services.Configure<AuthOptions>(
    builder.Configuration.GetSection("AuthOptions"));

// Obtener los valores de configuración JWT
var authOptions = builder.Configuration.GetSection("AuthOptions").Get<AuthOptions>();

// Configurar el esquema de autenticación
builder.Services.AddAuthentication(options =>
{
    // Usar JWT Bearer como esquema por defecto para autenticar
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    
    // Usar JWT Bearer como esquema cuando el usuario no está autorizado
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
// Configurar las opciones específicas de JWT Bearer
.AddJwtBearer(options =>
{
    // RequireHttpsMetadata = false: Permite HTTP en desarrollo (no solo HTTPS)
    // En producción debería ser true por seguridad
    options.RequireHttpsMetadata = false;
    
    // SaveToken = true: Guarda el token en el contexto HTTP después de validarlo
    options.SaveToken = true;
    
    // Parámetros de validación del token
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Validar que el token fue firmado con nuestra clave secreta
        ValidateIssuerSigningKey = true,
        
        // Clave simétrica para firmar y validar tokens
        // Se convierte la Key string a bytes usando UTF8
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(authOptions.Key)),
        
        // Validar que el token fue emitido por nosotros (Issuer)
        ValidateIssuer = true,
        ValidIssuer = authOptions.Issuer,  // "BankLinkAPI"
        
        // Validar que el token está destinado a nosotros (Audience)
        ValidateAudience = true,
        ValidAudience = authOptions.Audience,  // "BankLinkAPI"
        
        // Validar que el token no haya expirado
        ValidateLifetime = true,
        
        // ClockSkew = 0: No tolerar diferencias de tiempo entre servidores
        // Por defecto ASP.NET tolera 5 minutos, nosotros lo desactivamos
        ClockSkew = TimeSpan.Zero
    };
});

// ============================================================================
// SECCIÓN 6: CONFIGURACIÓN DE HTTPCLIENT (Para APIs externas)
// ============================================================================
// Registrar HttpClientFactory para hacer llamadas HTTP a bancos externos
// Factory pattern permite reutilizar conexiones HTTP eficientemente
builder.Services.AddHttpClient();

// ============================================================================
// SECCIÓN 7: INYECCIÓN DE DEPENDENCIAS - REGISTRO DE SERVICIOS
// ============================================================================
// Aquí registramos las implementaciones de servicios que los controllers usarán
// Lifetime Scopes:
// - Singleton: Una instancia para toda la aplicación
// - Scoped: Una instancia por request HTTP
// - Transient: Una instancia nueva cada vez que se solicita

// FileStorageService: Utilidad para leer/escribir archivos JSON
// Singleton porque no tiene estado y se reutiliza
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();

// AuthService: Maneja login, registro y generación de tokens JWT
// Scoped porque necesita acceso al DbContext (que también es Scoped)
builder.Services.AddScoped<IAuthService, AuthService>();

// ============================================================================
// ESTRATEGIA DE PERSISTENCIA: USAR SIEMPRE BASE DE DATOS (SQL Server)
// ============================================================================
// Todos los servicios usan las implementaciones DbService que persisten
// en SQL Server a través de Entity Framework Core
// Alternativa: FileService que guarda en archivos JSON (solo para testing)

// ClienteDbService: CRUD de clientes en SQL Server
builder.Services.AddScoped<IClienteService, ClienteDbService>();

// CuentaDbService: CRUD de cuentas + actualización de saldos
builder.Services.AddScoped<ICuentaService, CuentaDbService>();

// MovimientoDbService: CRUD de movimientos (auditoría de transacciones)
builder.Services.AddScoped<IMovimientoService, MovimientoDbService>();

// BancoExternoDbService: CRUD de catálogo de bancos externos
builder.Services.AddScoped<IBancoExternoService, BancoExternoDbService>();

// TransferenciaDbService: Lógica transaccional compleja (BeginTransaction/Commit/Rollback)
// Este es el servicio más crítico porque implementa consistencia transaccional
builder.Services.AddScoped<ITransferenciaService, TransferenciaDbService>();

// ============================================================================
// SECCIÓN 8: BUILD DE LA APLICACIÓN
// ============================================================================
// Construir la aplicación con todas las configuraciones anteriores
var app = builder.Build();

// ============================================================================
// SECCIÓN 9: CONFIGURACIÓN DEL PIPELINE DE MIDDLEWARE
// ============================================================================
// El pipeline define el orden en que se procesan los requests HTTP

// En modo Development, habilitar Swagger para documentación interactiva
if (app.Environment.IsDevelopment())
{
    // UseSwagger: Genera el JSON de OpenAPI en /swagger/v1/swagger.json
    app.UseSwagger();
    
    // UseSwaggerUI: Genera la interfaz web en /swagger
    app.UseSwaggerUI();
}

// UseHttpsRedirection: Redirigir HTTP a HTTPS
// Comentado para desarrollo local sin certificados SSL
//app.UseHttpsRedirection();

// ============================================================================
// ORDEN CRÍTICO: Authentication DEBE ir ANTES de Authorization
// ============================================================================
// UseAuthentication: Valida el token JWT y establece User.Identity
// Debe ejecutarse antes de UseAuthorization
app.UseAuthentication();

// UseAuthorization: Verifica que el usuario tenga permisos para el endpoint
// Usa la información establecida por UseAuthentication
app.UseAuthorization();

// ============================================================================
// SECCIÓN 10: MAPEO DE CONTROLADORES
// ============================================================================
// MapControllers: Escanea todos los controllers y mapea sus rutas
// Ejemplo: [Route("api/[controller]")] en ClientesController -> /api/Clientes
app.MapControllers();

// ============================================================================
// SECCIÓN 11: EJECUTAR LA APLICACIÓN
// ============================================================================
// Run: Inicia el servidor web y queda escuchando requests HTTP
// Bloquea hasta que se detenga la aplicación (Ctrl+C)
app.Run();
