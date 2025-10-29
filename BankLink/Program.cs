using BankLink.interfaces;
using BankLink.Service;
using BankLink.Context;
using BankLink.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Añadir cadena de conexión y DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BankLinkDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar AuthOptions desde appsettings.json
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("AuthOptions"));

// Configurar JWT Authentication
var authOptions = builder.Configuration.GetSection("AuthOptions").Get<AuthOptions>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Solo para desarrollo
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.Key)),
        ValidateIssuer = true,
        ValidIssuer = authOptions.Issuer,
        ValidateAudience = true,
        ValidAudience = authOptions.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Configurar HttpClient para llamadas a APIs externas
builder.Services.AddHttpClient();

// Registrar servicios
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Elegir UNA estrategia por entorno:
if (builder.Environment.IsDevelopment())
{
    // En desarrollo usamos archivos JSON
    builder.Services.AddScoped<IClienteService, ClienteFileService>();
    builder.Services.AddScoped<ICuentaService, CuentaFileService>();
    builder.Services.AddScoped<IMovimientoService, MovimientoFileService>();
    builder.Services.AddScoped<IBancoExternoService, BancoExternoFileService>();
    // Para transferencias siempre usamos DbService por la complejidad de las transacciones
    builder.Services.AddScoped<ITransferenciaService, TransferenciaDbService>();
}
else
{
    // En producción usamos Base de Datos
    builder.Services.AddScoped<IClienteService, ClienteDbService>();
    builder.Services.AddScoped<ICuentaService, CuentaDbService>();
    builder.Services.AddScoped<IMovimientoService, MovimientoDbService>();
    builder.Services.AddScoped<IBancoExternoService, BancoExternoDbService>();
    builder.Services.AddScoped<ITransferenciaService, TransferenciaDbService>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthentication(); // ¡Debe ir ANTES de UseAuthorization!
app.UseAuthorization();

app.MapControllers();

app.Run();
