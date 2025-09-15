using Biblioteca.interfaces;
using Biblioteca.Services;
using EFCorePersonaApi.Data;
using EFCorePersonaApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// 1. Añadir cadena de conexión y DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
builder.Services.AddSingleton<IPersonaService, PersonaFileServices>();
builder.Services.AddSingleton<IAutorService, AutorFileService>();
builder.Services.AddSingleton<ILibroService, LibroFileService>();
builder.Services.Addscoped<IPersonaService, PersonaDbServices>();
builder.Services.Addscoped<IAutorService, AutorDbService>();
builder.Services.AddScoped<ILibroService, LibroDbService>();
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
