using Biblioteca.interfaces;
using Biblioteca.Services;
using Biblioteca.Context;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddSingleton<IFileStorageService, FileStorageService>();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
