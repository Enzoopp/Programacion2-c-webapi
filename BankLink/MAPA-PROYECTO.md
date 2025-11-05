# 🗺️ MAPA COMPLETO DEL PROYECTO BANKLINK

## 📁 ESTRUCTURA DE CARPETAS Y ARCHIVOS

```
BankLink/
├── 📂 Controllers/          → Endpoints de la API (6 archivos)
├── 📂 Models/               → Entidades de base de datos (6 archivos)
├── 📂 Dtos/                 → Objetos de transferencia (2 archivos)
├── 📂 Context/              → Configuración de Entity Framework (1 archivo)
├── 📂 interfaces/           → Contratos de servicios (7 archivos)
├── 📂 Service/              → Lógica de negocio (12 archivos)
├── 📂 Migrations/           → Historial de base de datos (3 archivos)
├── 📂 data/                 → Archivos JSON de respaldo (3 archivos)
├── 📂 Properties/           → Configuración de lanzamiento (1 archivo)
├── 📂 bin/                  → Archivos compilados (generado)
├── 📂 obj/                  → Archivos temporales (generado)
├── 📄 Program.cs            → Punto de entrada de la aplicación
├── 📄 appsettings.json      → Configuración principal
├── 📄 BankLink.csproj       → Archivo del proyecto
├── 📄 BankLink.http         → Ejemplos de prueba HTTP
├── 📄 README.md             → Documentación del proyecto
└── 📄 reset-db.bat          → Script para limpiar base de datos
```

---

## 📂 CARPETA: Controllers/ (6 archivos)

### **1. AuthController.cs**
**Ubicación:** `Controllers/AuthController.cs`  
**Propósito:** Maneja autenticación y registro  
**Endpoints:**
- `POST /api/auth/register` → Registrar nuevo cliente
- `POST /api/auth/login` → Iniciar sesión y obtener token JWT

**Dependencias:** `IAuthService`, `IClienteService`

**Código clave:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterDto dto)
    {
        // Valida, crea cliente, genera token
    }
    
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginDto dto)
    {
        // Valida credenciales, retorna token
    }
}
```

**Qué decir si preguntan:** "Este controlador maneja la autenticación. El endpoint register crea un cliente nuevo con contraseña hasheada usando BCrypt, y login valida las credenciales y genera un token JWT con 60 minutos de validez."

---

### **2. ClientesController.cs**
**Ubicación:** `Controllers/ClientesController.cs`  
**Propósito:** CRUD completo de clientes  
**Endpoints:**
- `GET /api/Clientes` → Listar todos
- `GET /api/Clientes/{id}` → Obtener por ID
- `GET /api/Clientes/dni/{dni}` → Buscar por DNI
- `POST /api/Clientes` → Crear cliente
- `PUT /api/Clientes/{id}` → Actualizar cliente
- `DELETE /api/Clientes/{id}` → Eliminar cliente

**Dependencias:** `IClienteService`

**Qué decir si preguntan:** "Este controlador expone todas las operaciones CRUD sobre clientes. Incluye búsqueda por DNI que es útil para validaciones. Usa IClienteService inyectado por dependencia."

---

### **3. CuentasController.cs**
**Ubicación:** `Controllers/CuentasController.cs`  
**Propósito:** CRUD de cuentas + operaciones bancarias  
**Endpoints:**
- `GET /api/Cuentas` → Listar todas
- `GET /api/Cuentas/{id}` → Obtener por ID
- `GET /api/Cuentas/numero/{numero}` → Buscar por número
- `GET /api/Cuentas/cliente/{idCliente}` → Cuentas de un cliente
- `POST /api/Cuentas` → Crear cuenta
- `POST /api/Cuentas/deposito` → Realizar depósito
- `POST /api/Cuentas/retiro` → Realizar retiro
- `PUT /api/Cuentas/{id}` → Actualizar cuenta
- `DELETE /api/Cuentas/{id}` → Eliminar cuenta

**Dependencias:** `ICuentaService`, `IClienteService`, `IMovimientoService`

**Métodos importantes:**
```csharp
[HttpPost("deposito")]
public async Task<ActionResult> Deposito([FromBody] DepositoDto dto)
{
    // 1. Obtener cuenta
    // 2. Actualizar saldo: saldo += monto
    // 3. Crear movimiento automático
}

[HttpPost("retiro")]
public async Task<ActionResult> Retiro([FromBody] RetiroDto dto)
{
    // 1. Validar saldo suficiente
    // 2. Actualizar saldo: saldo -= monto
    // 3. Crear movimiento automático
}
```

**Qué decir si preguntan:** "Este es uno de los controladores más complejos. Además del CRUD, implementa depósito y retiro que automáticamente actualizan el saldo y registran movimientos. El retiro valida que haya saldo suficiente antes de procesar."

---

### **4. MovimientosController.cs**
**Ubicación:** `Controllers/MovimientosController.cs`  
**Propósito:** Consulta de historial de movimientos  
**Endpoints:**
- `GET /api/Movimientos` → Listar todos
- `GET /api/Movimientos/{id}` → Obtener por ID
- `GET /api/Movimientos/cuenta/{idCuenta}` → Movimientos de una cuenta
- `DELETE /api/Movimientos/{id}` → Eliminar movimiento

**Dependencias:** `IMovimientoService`

**Qué decir si preguntan:** "Este controlador permite consultar el historial de movimientos. El endpoint más usado es el que filtra por cuenta, que muestra todos los depósitos, retiros y transferencias de una cuenta específica. Los movimientos se crean automáticamente, no manualmente."

---

### **5. BancosExternosController.cs**
**Ubicación:** `Controllers/BancosExternosController.cs`  
**Propósito:** CRUD de bancos externos  
**Endpoints:**
- `GET /api/BancosExternos` → Listar todos
- `GET /api/BancosExternos/{id}` → Obtener por ID
- `GET /api/BancosExternos/codigo/{codigo}` → Buscar por código
- `POST /api/BancosExternos` → Registrar banco
- `PUT /api/BancosExternos/{id}` → Actualizar banco
- `DELETE /api/BancosExternos/{id}` → Eliminar banco

**Dependencias:** `IBancoExternoService`

**Qué decir si preguntan:** "Este controlador administra el catálogo de bancos externos. Cada banco tiene un código único y una URL de API que se usa para hacer transferencias externas."

---

### **6. TransferenciasController.cs**
**Ubicación:** `Controllers/TransferenciasController.cs`  
**Propósito:** Ejecución de transferencias  
**Endpoints:**
- `GET /api/Transferencias` → Listar todas
- `GET /api/Transferencias/{id}` → Obtener por ID
- `GET /api/Transferencias/cuenta/{idCuenta}` → Transferencias de una cuenta
- `POST /api/Transferencias/interna` → Transferencia entre cuentas BankLink
- `POST /api/Transferencias/externa` → Transferencia hacia banco externo
- `POST /api/Transferencias/recibir` → Recibir transferencia de banco externo
- `POST /api/Transferencias/automatica` → Detecta tipo automáticamente

**Dependencias:** `ITransferenciaService`, `ICuentaService`

**Código clave:**
```csharp
[HttpPost("interna")]
public async Task<ActionResult> TransferenciaInterna([FromBody] TransferenciaDto dto)
{
    // Llama a TransferenciaDbService que ejecuta:
    // 1. BeginTransaction
    // 2. Validaciones
    // 3. Actualizar ambos saldos
    // 4. Registrar movimientos
    // 5. CommitAsync o RollbackAsync
}
```

**Qué decir si preguntan:** "Este es el controlador más crítico porque aquí se ejecuta la lógica transaccional. El endpoint 'interna' llama al servicio que implementa la transacción completa con commit/rollback. El endpoint 'externa' hace una llamada HTTP a la API del banco destino. Y 'recibir' es para que otros bancos nos transfieran."

---

## 📂 CARPETA: Models/ (6 archivos)

### **1. Cliente.cs**
**Ubicación:** `Models/Cliente.cs`  
**Propósito:** Entidad principal de cliente  
**Tabla en BD:** `Clientes`  
**Relaciones:** 1:N con Cuenta

**Propiedades clave:**
```csharp
public class Cliente
{
    public int Id { get; set; }  // PK
    
    [Required]
    [StringLength(100)]
    public string Nombre { get; set; }
    
    [Required]
    [StringLength(8)]
    [RegularExpression(@"^\d{8}$")]
    public string Dni { get; set; }  // Índice único
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    public string NombreUsuario { get; set; }  // Índice único
    
    [Required]
    public string PassHash { get; set; }  // BCrypt hash
    
    public string Rol { get; set; } = "Cliente";
    
    // Navigation property
    public List<Cuenta> Cuentas { get; set; } = new();
}
```

**Qué decir si preguntan:** "Cliente es la entidad central. Tiene validaciones con DataAnnotations como Required y StringLength. El DNI está limitado a 8 dígitos con RegularExpression. PassHash nunca se expone en las respuestas JSON. La lista de Cuentas establece la relación uno-a-muchos."

---

### **2. Cuenta.cs**
**Ubicación:** `Models/Cuenta.cs`  
**Propósito:** Cuenta bancaria  
**Tabla en BD:** `Cuentas`  
**Relaciones:** N:1 con Cliente, 1:N con Movimiento

**Propiedades clave:**
```csharp
public class Cuenta
{
    public int Id { get; set; }
    
    [Required]
    public string NumeroCuenta { get; set; }  // 8 dígitos, único
    
    [Required]
    public string TipoCuenta { get; set; }  // "Ahorro" o "Corriente"
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal SaldoActual { get; set; }
    
    [Required]
    public string Estado { get; set; } = "Activa";
    
    public DateTime FechaApertura { get; set; } = DateTime.Now;
    
    // Foreign Key
    public int IdClientePropietario { get; set; }
    
    // Navigation properties
    public Cliente ClientePropietario { get; set; }
    public List<Movimiento> Movimientos { get; set; } = new();
}
```

**Qué decir si preguntan:** "La cuenta usa decimal(18,2) para el saldo, esto evita problemas de redondeo con dinero. El NumeroCuenta es único en la base de datos. Tiene dos navigation properties: hacia el cliente propietario y hacia la lista de movimientos."

---

### **3. Movimiento.cs**
**Ubicación:** `Models/Movimiento.cs`  
**Propósito:** Registro de transacción  
**Tabla en BD:** `Movimientos`  
**Relaciones:** N:1 con Cuenta

**Propiedades clave:**
```csharp
public class Movimiento
{
    public int Id { get; set; }
    
    // Foreign Key
    public int IdCuenta { get; set; }
    
    [Required]
    public string TipoMovimiento { get; set; }
    // Valores: "Depósito", "Retiro", 
    //          "Transferencia Enviada", "Transferencia Recibida"
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Monto { get; set; }
    
    public DateTime FechaHora { get; set; } = DateTime.Now;
    
    [Required]
    public string Descripcion { get; set; }
    
    // Navigation property
    public Cuenta Cuenta { get; set; }
}
```

**Qué decir si preguntan:** "Los movimientos son el registro de auditoría de todas las operaciones. Se crean automáticamente cuando hay un depósito, retiro o transferencia. TipoMovimiento indica qué operación se hizo. FechaHora se setea automáticamente con DateTime.Now."

---

### **4. BancoExterno.cs**
**Ubicación:** `Models/BancoExterno.cs`  
**Propósito:** Catálogo de bancos  
**Tabla en BD:** `BancosExternos`  
**Relaciones:** 1:N con Transferencia

**Propiedades clave:**
```csharp
public class BancoExterno
{
    public int Id { get; set; }
    
    [Required]
    public string NombreBanco { get; set; }
    
    [Required]
    public string CodigoIdentificacion { get; set; }  // Único
    
    [Required]
    [Url]
    public string UrlApiBase { get; set; }
    
    public string Descripcion { get; set; }
    
    public bool Activo { get; set; } = true;
}
```

**Qué decir si preguntan:** "BancoExterno almacena información de otros bancos. CodigoIdentificacion es único para evitar duplicados. UrlApiBase se usa con HttpClient para hacer llamadas POST cuando transferimos plata hacia ese banco."

---

### **5. Transferencia.cs**
**Ubicación:** `Models/Transferencia.cs`  
**Propósito:** Registro de transferencia  
**Tabla en BD:** `Transferencias`  
**Relaciones:** N:1 con Cuenta, N:1 con BancoExterno (opcional)

**Propiedades clave:**
```csharp
public class Transferencia
{
    public int Id { get; set; }
    
    // Foreign Keys
    public int IdCuentaOrigen { get; set; }
    public int? IdBancoDestino { get; set; }  // Nullable
    
    [Required]
    public string NumeroCuentaDestino { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Monto { get; set; }
    
    public DateTime FechaHora { get; set; } = DateTime.Now;
    
    [Required]
    public string Estado { get; set; }
    // Valores: "Pendiente", "Completada", "Fallida"
    
    [Required]
    public string Descripcion { get; set; }
    
    [Required]
    public string TipoTransferencia { get; set; }
    // Valores: "Enviada", "Recibida"
    
    // Navigation properties
    public Cuenta CuentaOrigen { get; set; }
    public BancoExterno? BancoDestino { get; set; }
}
```

**Qué decir si preguntan:** "La transferencia registra tanto internas como externas. IdBancoDestino es nullable porque si es interna no hay banco destino. El Estado permite trackear si está pendiente, completada o falló. TipoTransferencia diferencia si es enviada por nosotros o recibida de otro banco."

---

### **6. AuthOptions.cs**
**Ubicación:** `Models/AuthOptions.cs`  
**Propósito:** Configuración JWT  
**NO es tabla en BD** (es clase de configuración)

**Propiedades:**
```csharp
public class AuthOptions
{
    public string Issuer { get; set; }      // "BankLinkAPI"
    public string Audience { get; set; }    // "BankLinkAPI"
    public string Key { get; set; }         // Clave secreta para firmar tokens
    public int ExpMinutes { get; set; }     // 60 minutos
}
```

**Qué decir si preguntan:** "AuthOptions mapea la sección 'AuthOptions' del appsettings.json. Contiene la configuración para generar y validar tokens JWT: emisor, audiencia, clave secreta y tiempo de expiración."

---

## 📂 CARPETA: Dtos/ (2 archivos)

### **1. AuthDto.cs**
**Ubicación:** `Dtos/AuthDto.cs`  
**Propósito:** DTOs para autenticación  

**Records incluidos:**
```csharp
// Para login
public record LoginDto(
    string NombreUsuario,
    string Contraseña
);

// Para registro
public record RegisterDto(
    string Nombre,
    string Apellido,
    string Dni,
    string Direccion,
    string Telefono,
    string Email,
    string NombreUsuario,
    string Contraseña,
    string Rol = "Cliente"
);

// Respuesta de login
public record LoginResponseDto(
    string Token,
    string Rol,
    string NombreUsuario
);

// Para crear token
public record CreateTokenDto(
    string NombreUsuario,
    int Id,
    string Nombre,
    string Rol
);
```

**Qué decir si preguntan:** "Estos DTOs separan lo que entra/sale de la API de las entidades internas. Por ejemplo, RegisterDto recibe 'Contraseña' en texto plano pero nunca se guarda así, se hashea antes. LoginResponseDto solo devuelve token, rol y usuario, no toda la información del cliente."

---

### **2. OperacionesDto.cs**
**Ubicación:** `Dtos/OperacionesDto.cs`  
**Propósito:** DTOs para operaciones bancarias  

**Records incluidos:**
```csharp
public record DepositoDto(
    int IdCuenta,
    decimal Monto,
    string Descripcion
);

public record RetiroDto(
    int IdCuenta,
    decimal Monto,
    string Descripcion
);

public record TransferenciaDto(
    int IdCuentaOrigen,
    string NumeroCuentaDestino,
    decimal Monto,
    string Descripcion
);

public record TransferenciaExternaDto(
    int IdCuentaOrigen,
    int IdBancoDestino,
    string NumeroCuentaDestino,
    decimal Monto,
    string Descripcion
);

public record TransferenciaRecibidaDto(
    string BancoOrigen,
    string NumeroCuentaOrigen,
    int IdCuentaDestino,
    decimal Monto,
    string Descripcion
);

public record CrearCuentaDto(
    int IdClientePropietario,
    string TipoCuenta,
    decimal SaldoActual
);
```

**Qué decir si preguntan:** "Estos DTOs validan y estructuran las operaciones bancarias. Por ejemplo, DepositoDto solo necesita cuenta, monto y descripción. TransferenciaExternaDto incluye IdBancoDestino porque va a otro banco. Usar records hace el código más conciso."

---

## 📂 CARPETA: Context/ (1 archivo)

### **BankLinkDbContext.cs**
**Ubicación:** `Context/BankLinkDbContext.cs`  
**Propósito:** Configuración de Entity Framework  

**Contenido clave:**
```csharp
public class BankLinkDbContext : DbContext
{
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Cuenta> Cuentas { get; set; }
    public DbSet<Movimiento> Movimientos { get; set; }
    public DbSet<BancoExterno> BancosExternos { get; set; }
    public DbSet<Transferencia> Transferencias { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Relación Cliente -> Cuentas (1:N)
        modelBuilder.Entity<Cuenta>()
            .HasOne(c => c.ClientePropietario)
            .WithMany(cl => cl.Cuentas)
            .HasForeignKey(c => c.IdClientePropietario)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Relación Cuenta -> Movimientos (1:N)
        modelBuilder.Entity<Movimiento>()
            .HasOne(m => m.Cuenta)
            .WithMany(c => c.Movimientos)
            .HasForeignKey(m => m.IdCuenta)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Índices únicos
        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.Dni)
            .IsUnique();
        
        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.NombreUsuario)
            .IsUnique();
        
        modelBuilder.Entity<Cuenta>()
            .HasIndex(c => c.NumeroCuenta)
            .IsUnique();
        
        modelBuilder.Entity<BancoExterno>()
            .HasIndex(b => b.CodigoIdentificacion)
            .IsUnique();
        
        // Precisión decimal
        modelBuilder.Entity<Cuenta>()
            .Property(c => c.SaldoActual)
            .HasPrecision(18, 2);
        
        // Más configuraciones...
    }
}
```

**Qué decir si preguntan:** "El DbContext es el punto de entrada de Entity Framework. Aquí defino los DbSets que mapean a tablas. En OnModelCreating configuro las relaciones con HasOne/WithMany, los índices únicos para DNI y NumeroCuenta, y la precisión decimal. Use DeleteBehavior.Restrict para evitar eliminaciones en cascada no deseadas."

---

## 📂 CARPETA: interfaces/ (7 archivos)

Todas siguen el mismo patrón. Son **contratos** que definen qué métodos debe implementar cada servicio.

**Archivos:**
1. `IAuthService.cs` - Login, Register, CreateToken
2. `IClienteService.cs` - CRUD de clientes
3. `ICuentaService.cs` - CRUD + ActualizarSaldo
4. `IMovimientoService.cs` - CRUD de movimientos
5. `IBancoExternoService.cs` - CRUD de bancos
6. `ITransferenciaService.cs` - Métodos de transferencia
7. `IFileStorageService.cs` - Leer/escribir JSON

**Qué decir si preguntan:** "Las interfaces definen contratos para inyección de dependencias. Esto permite cambiar la implementación sin modificar los controllers. Por ejemplo, tengo IClienteService implementado por ClienteDbService y ClienteFileService, y puedo intercambiarlos en Program.cs."

---

## 📂 CARPETA: Service/ (12 archivos)

### **Implementaciones DbService (usan SQL Server):**

1. **ClienteDbService.cs** - CRUD con Entity Framework
2. **CuentaDbService.cs** - CRUD + ActualizarSaldo
3. **MovimientoDbService.cs** - CRUD con Include para relaciones
4. **BancoExternoDbService.cs** - CRUD simple
5. **TransferenciaDbService.cs** - Lógica transaccional compleja

### **Implementaciones FileService (usan JSON):**

6. **ClienteFileService.cs** - Lee/escribe `data/clientes.json`
7. **CuentaFileService.cs** - Lee/escribe `data/cuentas.json`
8. **MovimientoFileService.cs** - Lee/escribe `data/movimientos.json`
9. **BancoExternoFileService.cs** - Lee/escribe `data/bancos.json`

### **Servicios especiales:**

10. **AuthService.cs** - Autenticación y JWT
11. **FileStorageService.cs** - Utilidad para JSON
12. **AutorDbService.cs** (si existe) - Del proyecto Biblioteca

**Qué decir si preguntan:** "Implementé dos estrategias de persistencia: DbService para producción con SQL Server, y FileService para pruebas rápidas con JSON. En Program.cs configuré para usar siempre DbService. Los servicios más complejos son AuthService para JWT y TransferenciaDbService para transacciones."

---

## 📂 CARPETA: Migrations/ (3 archivos)

1. **20251029133450_InitialCreate.cs** - Migración inicial
2. **20251029133450_InitialCreate.Designer.cs** - Metadata
3. **BankLinkDbContextModelSnapshot.cs** - Estado actual

**Contenido de InitialCreate:**
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Crea tablas: Clientes, Cuentas, Movimientos, 
    //              BancosExternos, Transferencias
    // Define columnas, tipos, FKs, índices
}
```

**Qué decir si preguntan:** "Las migraciones son el historial de cambios en la base de datos. La inicial crea las 5 tablas con sus relaciones. Puedo recrear la base de datos en cualquier momento con `dotnet ef database update`. El snapshot mantiene el estado actual del modelo."

---

## 📄 ARCHIVOS RAÍZ

### **Program.cs**
**Ubicación:** Raíz del proyecto  
**Propósito:** Punto de entrada y configuración  

**Secciones clave:**
```csharp
// 1. Configuración de DbContext
builder.Services.AddDbContext<BankLinkDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Configuración de Controllers con ReferenceHandler
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.ReferenceHandler = 
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// 3. Configuración de JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { ... });

// 4. Registro de servicios (Dependency Injection)
builder.Services.AddScoped<IClienteService, ClienteDbService>();
// ... más servicios

// 5. Middleware pipeline
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

**Qué decir si preguntan:** "Program.cs es el corazón de la configuración. Aquí registro el DbContext con SQL Server, configuro JWT con tokens de 60 minutos, registro todos los servicios para inyección de dependencias, y defino el pipeline de middleware. El ReferenceHandler.IgnoreCycles previene bucles infinitos al serializar relaciones."

---

### **appsettings.json**
**Ubicación:** Raíz del proyecto  
**Propósito:** Configuración de la aplicación  

**Contenido:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=BankLinkDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "AuthOptions": {
    "Issuer": "BankLinkAPI",
    "Audience": "BankLinkAPI",
    "Key": "SuperSecretKeyForJWTAuthentication12345",
    "ExpMinutes": 60
  },
  "BancoExternoApi": {
    "BaseUrl": "https://api-externo.ejemplo.com",
    "Timeout": 30
  }
}
```

**Qué decir si preguntan:** "En appsettings.json están todas las configuraciones externas. La connection string apunta a SQL Server Express local. AuthOptions tiene la clave secreta para firmar tokens JWT. TrustServerCertificate en True es para desarrollo local sin certificados SSL."

---

## 🎯 RESUMEN DE FLUJO DE DATOS

```
1. REQUEST HTTP llega a Controller
   ↓
2. Controller valida DTO
   ↓
3. Controller llama a Service (inyectado)
   ↓
4. Service ejecuta lógica de negocio
   ↓
5. Service usa DbContext para acceder BD
   ↓
6. DbContext ejecuta SQL en SQL Server
   ↓
7. Service retorna resultado a Controller
   ↓
8. Controller retorna HTTP Response
```

---

¿Continúo con el script detallado de Swagger?
