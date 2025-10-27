// using BankLink.interfaces; // Comentado temporalmente
using BankLink.Context;
using BankLink.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar cadena de conexión y DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BankLinkDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container
builder.Services.AddControllers();
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

// Configurar HttpClient para APIs externas
builder.Services.AddHttpClient();

// Servicios comentados temporalmente porque están excluidos del proyecto
// builder.Services.AddScoped<IClienteService, ClienteService>();
// builder.Services.AddScoped<IMovimientoService, MovimientoService>();
// builder.Services.AddScoped<IBancoExternoService, BancoExternoService>();
// builder.Services.AddScoped<IAuthService, AuthService>();
// builder.Services.AddScoped<ICuentaService, CuentaService>();
// builder.Services.AddScoped<ITransferenciaService, TransferenciaService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();