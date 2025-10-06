using Biblioteca.interfaces;
using Biblioteca.Services;
using Biblioteca.Context;
using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// 1. Añadir cadena de conexión y DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BibliotecaDbContext>(options =>
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

builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Elegir UNA estrategia por entorno:
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IPersonaService, PersonaFileServices>();
    builder.Services.AddScoped<IAutorService, AutorFileService>();
    builder.Services.AddScoped<ILibroService, LibroFileService>();
}
else
{
    builder.Services.AddScoped<IPersonaService, PersonaDbServices>();
    builder.Services.AddScoped<IAutorService, AutorDbService>();
    // builder.Services.AddScoped<ILibroService, LibroDbService>(); // Crear este servicio
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
