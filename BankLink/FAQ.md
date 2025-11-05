# ❓ FAQ - PREGUNTAS PROBABLES DE LOS PROFESORES

Esta guía contiene las preguntas más probables que pueden hacerte los profesores durante la presentación, con respuestas técnicas detalladas.

---

## 🔐 SEGURIDAD Y AUTENTICACIÓN

### **P1: ¿Por qué usaste JWT y no sesiones tradicionales?**

**Respuesta:**
> "Elegí JWT (JSON Web Tokens) porque:
>
> **Ventajas de JWT:**
> - **Stateless:** El servidor no necesita guardar sesiones en memoria/base de datos. Toda la info está en el token.
> - **Escalabilidad:** Puedo tener múltiples servidores sin compartir estado de sesión.
> - **Cross-domain:** El token se puede usar en diferentes dominios (útil si tengo frontend en otro servidor).
> - **Mobile-friendly:** Fácil de usar en apps móviles (solo guardar el token en storage).
>
> **Cómo funciona:**
> 1. Usuario hace login con usuario/contraseña
> 2. Servidor valida credenciales con BCrypt
> 3. Servidor crea un JWT con claims (id, nombre, rol) y lo firma con clave secreta
> 4. Cliente guarda el token
> 5. En cada request posterior, el cliente envía `Authorization: Bearer {token}`
> 6. Servidor valida la firma del token sin consultar la BD
>
> **Código en AuthService.cs:**
> ```csharp
> var tokenHandler = new JwtSecurityTokenHandler();
> var key = Encoding.ASCII.GetBytes(_authOptions.SecretKey);
> var tokenDescriptor = new SecurityTokenDescriptor
> {
>     Subject = new ClaimsIdentity(new[] {
>         new Claim(ClaimTypes.NameIdentifier, cliente.Id.ToString()),
>         new Claim(ClaimTypes.Name, cliente.NombreUsuario),
>         // ... más claims
>     }),
>     Expires = DateTime.UtcNow.AddMinutes(60),
>     SigningCredentials = new SigningCredentials(
>         new SymmetricSecurityKey(key), 
>         SecurityAlgorithms.HmacSha256Signature)
> };
> ```
>
> **Desventaja:** No se puede revocar un token antes de que expire (a menos que uses una blacklist)."

---

### **P2: ¿Cómo garantizás la seguridad de las contraseñas?**

**Respuesta:**
> "Uso **BCrypt.Net-Next** para hashear contraseñas, que es un algoritmo diseñado específicamente para este propósito:
>
> **Características de BCrypt:**
> - **Lento por diseño:** Usa 2^11 iteraciones (configurable), lo que hace inviable ataques de fuerza bruta.
> - **Salt automático:** Cada hash tiene un salt único de 22 caracteres, evitando rainbow tables.
> - **Unidireccional:** No se puede recuperar la contraseña original, solo verificar.
> - **Formato:** `$2a$11${salt}{hash}` (60 caracteres totales)
>
> **En AuthService.cs:**
> ```csharp
> // Al registrar:
> var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Contraseña);
> 
> // Al hacer login:
> if (!BCrypt.Net.BCrypt.Verify(dto.Contraseña, cliente.ContraseñaHash))
> {
>     return null; // Contraseña incorrecta
> }
> ```
>
> **¿Por qué no SHA256 o MD5?**
> - MD5 y SHA256 son **rápidos** → un atacante puede probar millones de contraseñas por segundo.
> - BCrypt es **lento** → un atacante solo puede probar cientos por segundo.
>
> **Seguridad adicional:**
> - La contraseña nunca se guarda en texto plano.
> - La contraseña no se retorna en ningún endpoint.
> - En producción, agregaría: HTTPS obligatorio, rate limiting, 2FA."

---

### **P3: ¿Cómo prevenís inyección SQL?**

**Respuesta:**
> "Entity Framework previene inyección SQL automáticamente mediante **parameterización de queries**:
>
> **Código vulnerable (NO usar):**
> ```csharp
> // ❌ PELIGROSO: Concatenación directa
> var query = $\"SELECT * FROM Clientes WHERE NombreUsuario = '{usuario}';\";
> // Si usuario = \"admin' OR '1'='1\", la query se rompe
> ```
>
> **Código seguro (Entity Framework):**
> ```csharp
> // ✅ SEGURO: EF Core parametriza automáticamente
> var cliente = await _context.Clientes
>     .FirstOrDefaultAsync(c => c.NombreUsuario == usuario);
> 
> // EF genera:
> // SELECT * FROM Clientes WHERE NombreUsuario = @p0
> // Parámetro @p0 = valor escapado
> ```
>
> **Por qué es seguro:**
> - Entity Framework trata los valores como **datos**, no como **código SQL**.
> - Cualquier comilla o carácter especial se escapa automáticamente.
> - Incluso si el usuario envía `'; DROP TABLE Clientes; --`, se trata como un string literal.
>
> **Ventaja adicional:** Las queries parametrizadas también mejoran el **plan de ejecución** en SQL Server (query caching)."

---

## 💾 BASE DE DATOS Y ENTITY FRAMEWORK

### **P4: ¿Por qué usaste `decimal` y no `double` para dinero?**

**Respuesta:**
> "Usé `decimal(18, 2)` porque **double/float son binarios y causan errores de redondeo con dinero**:
>
> **Problema con float/double:**
> ```csharp
> double saldo = 0.1 + 0.2; // Resultado: 0.30000000000000004 ❌
> ```
>
> **Ventaja de decimal:**
> ```csharp
> decimal saldo = 0.1m + 0.2m; // Resultado: 0.3 ✅
> ```
>
> **Configuración en BankLinkDbContext.cs:**
> ```csharp
> modelBuilder.Entity<Cuenta>()
>     .Property(c => c.SaldoActual)
>     .HasPrecision(18, 2); // 18 dígitos totales, 2 decimales
> 
> // En SQL Server se crea como: decimal(18, 2)
> ```
>
> **¿Por qué (18, 2)?**
> - **18 dígitos totales:** Permite hasta $9,999,999,999,999,999.99 (suficiente para sistemas bancarios).
> - **2 decimales:** Representa centavos/céntimos.
>
> **Regla general:** **SIEMPRE usar decimal para dinero, float/double SOLO para cálculos científicos.**"

---

### **P5: ¿Qué es `DeleteBehavior.Restrict` y por qué lo usaste?**

**Respuesta:**
> "`DeleteBehavior.Restrict` evita eliminaciones en cascada que podrían borrar datos críticos por error:
>
> **Ejemplo del problema:**
> ```csharp
> // Si usara Cascade:
> modelBuilder.Entity<Cuenta>()
>     .HasOne(c => c.ClientePropietario)
>     .WithMany(cl => cl.Cuentas)
>     .OnDelete(DeleteBehavior.Cascade); // ❌ Peligroso
> 
> // Si borro un Cliente, se borran TODAS sus Cuentas automáticamente
> // Y si esas Cuentas tienen Movimientos, también se borran
> // Se perdería TODO el historial de transacciones
> ```
>
> **Con Restrict:**
> ```csharp
> modelBuilder.Entity<Cuenta>()
>     .HasOne(c => c.ClientePropietario)
>     .WithMany(cl => cl.Cuentas)
>     .OnDelete(DeleteBehavior.Restrict); // ✅ Seguro
> 
> // Si intento borrar un Cliente que tiene Cuentas:
> // SQL Server lanza excepción: FK constraint violation
> ```
>
> **Relaciones con Restrict en BankLink:**
> - Cliente → Cuentas: No puedo borrar un cliente con cuentas activas.
> - Cuenta → Movimientos: No puedo borrar una cuenta con historial.
> - Transferencia → BancoExterno: No puedo borrar un banco con transferencias registradas.
>
> **Flujo correcto para borrar:**
> 1. Primero eliminar/archivar movimientos (o moverlos a tabla histórica)
> 2. Luego cerrar las cuentas
> 3. Finalmente borrar el cliente
>
> **Esto protege la integridad de los datos bancarios.**"

---

### **P6: ¿Qué son las migraciones y para qué sirven?**

**Respuesta:**
> "Las migraciones son archivos C# que **versionan los cambios del esquema de la base de datos**:
>
> **Problema que resuelven:**
> - Sin migraciones: Cada desarrollador modifica la BD manualmente → caos en equipo.
> - Con migraciones: Los cambios se guardan en código → reproducibles y versionables.
>
> **Cómo funcionan:**
> ```bash
> # 1. Modifico mis modelos en C# (agrego propiedad, relación, etc.)
> # 2. Genero migración
> dotnet ef migrations add NombreMigracion
> 
> # 3. Se crea un archivo con:
> # - Up(): SQL para aplicar cambio
> # - Down(): SQL para revertir cambio
> 
> # 4. Aplico migración a la BD
> dotnet ef database update
> ```
>
> **Ejemplo de InitialCreate.cs:**
> ```csharp
> protected override void Up(MigrationBuilder migrationBuilder)
> {
>     migrationBuilder.CreateTable(
>         name: \"Clientes\",
>         columns: table => new
>         {
>             Id = table.Column<int>(nullable: false)
>                 .Annotation(\"SqlServer:Identity\", \"1, 1\"),
>             NombreUsuario = table.Column<string>(maxLength: 50, nullable: false),
>             // ... más columnas
>         },
>         constraints: table =>
>         {
>             table.PrimaryKey(\"PK_Clientes\", x => x.Id);
>         });
> }
> 
> protected override void Down(MigrationBuilder migrationBuilder)
> {
>     migrationBuilder.DropTable(name: \"Clientes\");
> }
> ```
>
> **Ventajas:**
> - **Versionado:** Git rastrea cambios en el esquema.
> - **Reproducibilidad:** Cualquier dev puede recrear la BD con `dotnet ef database update`.
> - **Rollback:** Si algo sale mal, puedo revertir con el método Down().
> - **Trabajo en equipo:** Evita conflictos de esquema entre desarrolladores.
>
> **En BankLink tengo 2 migraciones:**
> 1. `InitialCreate` → Crea las 5 tablas principales
> 2. `AddAutorLibroRelationship` → (esto parece ser de otro proyecto, error de copy-paste)"

---

## 🔄 TRANSACCIONES Y CONSISTENCIA

### **P7: ¿Cómo garantizás la consistencia transaccional?**

**Respuesta:**
> "Uso **transacciones explícitas de Entity Framework** para garantizar las propiedades ACID:
>
> **ACID significa:**
> - **A**tomicity (Atomicidad): Todo o nada.
> - **C**onsistency (Consistencia): Las reglas de negocio siempre se cumplen.
> - **I**solation (Aislamiento): Transacciones concurrentes no se interfieren.
> - **D**urability (Durabilidad): Los cambios confirmados son permanentes.
>
> **Código en TransferenciaDbService.cs:**
> ```csharp
> using var transaction = await _context.Database.BeginTransactionAsync();
> try
> {
>     // Paso 1: Validar cuenta origen
>     var cuentaOrigen = await _cuentaService.GetByIdAsync(...);
>     if (cuentaOrigen.SaldoActual < dto.Monto)
>         throw new InvalidOperationException(\"Saldo insuficiente\");
> 
>     // Paso 2: Validar cuenta destino
>     var cuentaDestino = await _cuentaService.GetByIdAsync(...);
> 
>     // Paso 3: Actualizar saldos
>     cuentaOrigen.SaldoActual -= dto.Monto;
>     cuentaDestino.SaldoActual += dto.Monto;
> 
>     // Paso 4: Registrar movimientos
>     await _movimientoService.CreateAsync(movimientoDebito);
>     await _movimientoService.CreateAsync(movimientoCredito);
> 
>     // Paso 5: Registrar transferencia
>     await _context.Transferencias.AddAsync(transferencia);
>     await _context.SaveChangesAsync();
> 
>     // ✅ TODO OK → Confirmar transacción
>     await transaction.CommitAsync();
> }
> catch (Exception ex)
> {
>     // ❌ Algo falló → Revertir TODO
>     await transaction.RollbackAsync();
>     throw;
> }
> ```
>
> **¿Qué pasa si falla en el Paso 3?**
> - RollbackAsync() **revierte todos los cambios** (Steps 1 y 2).
> - Los saldos quedan EXACTAMENTE como estaban antes.
> - No se registran movimientos ni transferencias.
>
> **Ventaja:** Imposible tener inconsistencias como:
> - Dinero que desaparece (se resta de origen pero no se suma a destino)
> - Saldos actualizados sin movimientos que lo justifiquen
> - Transferencia registrada pero saldos sin cambiar"

---

### **P8: ¿Qué pasa si hay dos transferencias simultáneas desde la misma cuenta?**

**Respuesta:**
> "SQL Server maneja esto con **niveles de aislamiento** y **locks**:
>
> **Escenario:**
> - Usuario A intenta transferir $100 desde Cuenta 1 (saldo: $200)
> - Usuario B intenta transferir $150 desde Cuenta 1 AL MISMO TIEMPO
>
> **Sin transacciones (MAL):**
> 1. A lee saldo: $200
> 2. B lee saldo: $200 (aún no se actualizó)
> 3. A resta $100 → saldo = $100
> 4. B resta $150 → saldo = $50 ❌ (debería ser -$50, error!)
>
> **Con transacciones (BIEN):**
> 1. A inicia transacción y **lockea** la fila de Cuenta 1
> 2. B intenta leer Cuenta 1 → **espera** a que A termine
> 3. A actualiza saldo a $100 y **commit**
> 4. B lee saldo: $100
> 5. B valida: $100 < $150 → **falla** \"Saldo insuficiente\" ✅
>
> **Nivel de aislamiento en Entity Framework:**
> ```csharp
> // Por defecto: READ COMMITTED
> using var transaction = await _context.Database.BeginTransactionAsync();
> 
> // O explícitamente:
> using var transaction = await _context.Database.BeginTransactionAsync(
>     System.Data.IsolationLevel.ReadCommitted);
> ```
>
> **Tipos de locks:**
> - **Shared Lock (S):** Múltiples lecturas, sin escrituras.
> - **Exclusive Lock (X):** Solo una transacción puede modificar.
>
> **Optimización:**
> - Para mejor concurrencia, se podría usar **SNAPSHOT isolation** (requiere configuración en SQL Server).
> - O implementar **versiones optimistas** con un campo `RowVersion`."

---

## 🌐 INTEGRACIÓN CON APIS EXTERNAS

### **P9: ¿Qué pasa si la API del banco externo no responde?**

**Respuesta:**
> "Implementé **manejo de errores con rollback automático**:
>
> **Código en TransferenciaDbService.cs:**
> ```csharp
> using var transaction = await _context.Database.BeginTransactionAsync();
> try
> {
>     // Validar y actualizar cuenta origen
>     cuentaOrigen.SaldoActual -= dto.Monto;
>     await _context.SaveChangesAsync();
> 
>     // Llamar a API externa con timeout de 30 segundos
>     var request = new HttpRequestMessage(HttpMethod.Post, banco.UrlApi);
>     request.Headers.Add(\"Authorization\", banco.TokenAutorizacion);
>     var response = await _httpClient.SendAsync(request);
> 
>     if (!response.IsSuccessStatusCode)
>     {
>         // ❌ API externa falló → Revertir TODO
>         throw new HttpRequestException($\"Error {response.StatusCode}\");
>     }
> 
>     // ✅ API respondió OK → Confirmar transacción
>     await transaction.CommitAsync();
> }
> catch (Exception ex)
> {
>     // ❌ Timeout, error de red, o fallo de API → Revertir
>     await transaction.RollbackAsync();
>     throw new InvalidOperationException(\"Error al comunicarse con banco externo\", ex);
> }
> ```
>
> **Flujo con error:**
> 1. Se resta $5,000 de la cuenta origen (temporalmente)
> 2. Se intenta llamar a `https://api-banco-galicia.com/transferencias`
> 3. **Timeout de 30 segundos** o error HTTP
> 4. Se ejecuta RollbackAsync()
> 5. El saldo vuelve a su valor original
> 6. El cliente recibe error 400 con mensaje claro
>
> **En producción real agregaría:**
> - **Retry policy:** Reintentar 3 veces con backoff exponencial
> - **Circuit breaker:** Si el banco falla 5 veces seguidas, dejar de intentar por 1 minuto
> - **Cola de mensajes:** Guardar la transferencia pendiente y procesarla asíncronamente
> - **Compensación:** Si se confirmó en el banco externo pero falló localmente, crear un reverso
>
> **Bibliotecas útiles:**
> - **Polly:** Para retry, circuit breaker, timeout policies
> - **RabbitMQ/Azure Service Bus:** Para procesamiento asíncrono"

---

### **P10: ¿Por qué usaste HttpClient con Factory?**

**Respuesta:**
> "Usé **IHttpClientFactory** porque crear HttpClient manualmente causa problemas:
>
> **Problema con `new HttpClient()` directo:**
> ```csharp
> // ❌ MAL: Agota los sockets del sistema
> using (var client = new HttpClient())
> {
>     var response = await client.GetAsync(url);
> }
> // El socket no se libera inmediatamente → después de 1000 requests se cae la app
> ```
>
> **Solución con Factory:**
> ```csharp
> // En Program.cs:
> builder.Services.AddHttpClient();
> 
> // En TransferenciaDbService.cs:
> public class TransferenciaDbService
> {
>     private readonly IHttpClientFactory _httpClientFactory;
> 
>     public TransferenciaDbService(IHttpClientFactory httpClientFactory)
>     {
>         _httpClientFactory = httpClientFactory;
>     }
> 
>     public async Task RealizarTransferenciaExterna(...)
>     {
>         // ✅ BIEN: Factory reutiliza conexiones
>         var client = _httpClientFactory.CreateClient();
>         client.Timeout = TimeSpan.FromSeconds(30);
>         var response = await client.PostAsync(url, content);
>     }
> }
> ```
>
> **Ventajas:**
> - **Reutilización de conexiones:** El factory mantiene un pool de HttpHandlers.
> - **Configuración centralizada:** Puedo definir timeout, headers, políticas de retry en Program.cs.
> - **Inyección de dependencias:** Testeable con mocks.
>
> **Configuración avanzada (opcional):**
> ```csharp
> builder.Services.AddHttpClient(\"BancoGalicia\", client =>
> {
>     client.BaseAddress = new Uri(\"https://api-galicia.com\");
>     client.Timeout = TimeSpan.FromSeconds(30);
>     client.DefaultRequestHeaders.Add(\"Accept\", \"application/json\");
> }).AddPolicyHandler(GetRetryPolicy()); // Agrega retry automático con Polly
> ```"

---

## 🏗️ ARQUITECTURA Y DISEÑO

### **P11: ¿Por qué separaste en Controllers, Services e Interfaces?**

**Respuesta:**
> "Usé **arquitectura en capas** siguiendo el principio de **Separation of Concerns**:
>
> **Estructura:**
> ```
> Controllers/         → Reciben HTTP requests, validan, retornan responses
>   ├─ CuentasController.cs
>   ├─ TransferenciasController.cs
> 
> interfaces/          → Contratos que definen QUÉ hace cada servicio
>   ├─ ICuentaService.cs
>   ├─ ITransferenciaService.cs
> 
> Service/             → Lógica de negocio, CÓMO se hacen las operaciones
>   ├─ CuentaDbService.cs
>   ├─ TransferenciaDbService.cs
> 
> Context/             → Acceso a base de datos con Entity Framework
>   └─ BankLinkDbContext.cs
> 
> Models/              → Entidades del dominio
>   ├─ Cuenta.cs
>   ├─ Transferencia.cs
> ```
>
> **Ventajas:**
> 1. **Testabilidad:** Puedo testear servicios sin levantar un servidor HTTP.
> ```csharp
> // Mock del servicio en tests
> var mockService = new Mock<ICuentaService>();
> mockService.Setup(s => s.GetById(1)).Returns(cuentaFake);
> var controller = new CuentasController(mockService.Object);
> ```
>
> 2. **Reusabilidad:** Si creo una app de consola, puedo reusar los servicios.
> ```csharp
> // Console app puede usar el mismo servicio
> var service = new CuentaDbService(context);
> var cuenta = await service.GetByIdAsync(1);
> ```
>
> 3. **Mantenibilidad:** Si cambio de SQL Server a MongoDB, solo cambio la implementación de los servicios, no los controllers.
>
> 4. **Inyección de Dependencias:** Los controllers no crean servicios, los reciben por constructor.
> ```csharp
> // En Program.cs:
> builder.Services.AddScoped<ICuentaService, CuentaDbService>();
> 
> // En CuentasController:
> public CuentasController(ICuentaService cuentaService) // Recibe por DI
> {
>     _cuentaService = cuentaService;
> }
> ```
>
> **Patrón Repository:** Los servicios actúan como repositorios que abstraen el acceso a datos."

---

### **P12: ¿Qué es Dependency Injection y por qué la usaste?**

**Respuesta:**
> "**Dependency Injection (DI)** es un patrón que invierte el control de creación de dependencias:
>
> **Sin DI (MAL):**
> ```csharp
> public class CuentasController
> {
>     private readonly CuentaDbService _service;
> 
>     public CuentasController()
>     {
>         // ❌ El controller CREA la dependencia
>         var options = new DbContextOptionsBuilder<BankLinkDbContext>()...
>         var context = new BankLinkDbContext(options);
>         _service = new CuentaDbService(context); // Acoplamiento fuerte
>     }
> }
> ```
>
> **Con DI (BIEN):**
> ```csharp
> public class CuentasController
> {
>     private readonly ICuentaService _service;
> 
>     // ✅ El controller RECIBE la dependencia
>     public CuentasController(ICuentaService service)
>     {
>         _service = service; // Acoplamiento débil (interfaz)
>     }
> }
> 
> // En Program.cs (contenedor de DI):
> builder.Services.AddScoped<ICuentaService, CuentaDbService>();
> // Cuando alguien pida ICuentaService, ASP.NET crea CuentaDbService
> ```
>
> **Ventajas:**
> 1. **Testeable:** Puedo inyectar un mock en lugar del servicio real.
> ```csharp
> var mockService = new Mock<ICuentaService>();
> var controller = new CuentasController(mockService.Object);
> ```
>
> 2. **Flexible:** Cambio de implementación sin tocar el controller.
> ```csharp
> // De base de datos a archivo JSON:
> builder.Services.AddScoped<ICuentaService, CuentaFileService>();
> ```
>
> 3. **Manejo automático de ciclo de vida:**
> - **Transient:** Nueva instancia cada vez (`AddTransient`)
> - **Scoped:** Una instancia por request HTTP (`AddScoped`) ← Uso esto para DbContext
> - **Singleton:** Una única instancia para toda la app (`AddSingleton`)
>
> **En BankLink registro 12 servicios:**
> ```csharp
> builder.Services.AddScoped<ICuentaService, CuentaDbService>();
> builder.Services.AddScoped<IClienteService, ClienteDbService>();
> builder.Services.AddScoped<ITransferenciaService, TransferenciaDbService>();
> // ... etc
> ```"

---

## 📊 OTROS CONCEPTOS

### **P13: ¿Qué son los DTOs y por qué los usaste?**

**Respuesta:**
> "**DTOs (Data Transfer Objects)** son objetos que definen la estructura de datos que se envían/reciben por HTTP:
>
> **Problema sin DTOs:**
> ```csharp
> // ❌ Enviar la entidad completa
> [HttpPost]
> public ActionResult Create([FromBody] Cliente cliente)
> {
>     // Problema: El cliente puede enviar cualquier propiedad, incluso Id!
>     _context.Clientes.Add(cliente);
>     await _context.SaveChangesAsync();
> }
> ```
>
> **Solución con DTOs:**
> ```csharp
> // DTO en Dtos/CrearClienteDto.cs
> public record CrearClienteDto
> {
>     public string NombreUsuario { get; init; }
>     public string Contraseña { get; init; }
>     public string Dni { get; init; }
>     // NO tiene Id (se genera automáticamente)
>     // NO tiene ContraseñaHash (se calcula con BCrypt)
> }
> 
> [HttpPost]
> public ActionResult Create([FromBody] CrearClienteDto dto)
> {
>     // ✅ Solo recibo los campos que necesito
>     var cliente = new Cliente
>     {
>         NombreUsuario = dto.NombreUsuario,
>         ContraseñaHash = BCrypt.HashPassword(dto.Contraseña),
>         Dni = dto.Dni,
>         // Id se genera automáticamente
>     };
> }
> ```
>
> **Ventajas:**
> 1. **Seguridad:** Evito que el cliente modifique campos sensibles (Id, ContraseñaHash).
> 2. **Validación:** Puedo agregar DataAnnotations específicas.
> ```csharp
> public record CrearClienteDto
> {
>     [Required(ErrorMessage = \"El DNI es obligatorio\")]
>     [StringLength(20, MinimumLength = 7)]
>     public string Dni { get; init; }
> 
>     [EmailAddress(ErrorMessage = \"Email inválido\")]
>     public string Email { get; init; }
> }
> ```
> 3. **Flexibilidad:** El DTO puede tener estructura diferente a la entidad.
> ```csharp
> // DTO con datos de múltiples entidades
> public record TransferenciaDto
> {
>     public string NumeroCuentaOrigen { get; init; } // String en DTO
>     public string NumeroCuentaDestino { get; init; }
>     // En la entidad Transferencia, guardo IdCuentaOrigen (int)
> }
> ```
>
> **Usé `record` en lugar de `class` porque:**
> - Inmutables por defecto (con `init`)
> - Comparación por valor
> - Sintaxis concisa"

---

### **P14: ¿Cómo manejas errores en la API?**

**Respuesta:**
> "Uso **códigos HTTP semánticos** y **try-catch con respuestas estructuradas**:
>
> **Códigos HTTP que uso:**
> - **200 OK:** Operación exitosa con datos en body
> - **201 Created:** Recurso creado (con header Location)
> - **204 No Content:** Operación exitosa sin datos (PUT, DELETE)
> - **400 Bad Request:** Error de validación o lógica de negocio
> - **404 Not Found:** Recurso no existe
> - **500 Internal Server Error:** Error inesperado del servidor
>
> **Ejemplo en CuentasController:**
> ```csharp
> [HttpPost(\"deposito\")]
> public ActionResult RealizarDeposito([FromBody] DepositoDto dto)
> {
>     // Validación automática con [ApiController]
>     if (!ModelState.IsValid)
>     {
>         return BadRequest(ModelState); // 400 con detalles de validación
>     }
> 
>     try
>     {
>         var cuenta = _cuentaService.GetById(dto.IdCuenta);
>         if (cuenta == null)
>         {
>             return NotFound($\"Cuenta {dto.IdCuenta} no encontrada\"); // 404
>         }
> 
>         if (cuenta.Estado != \"Activa\")
>         {
>             return BadRequest(\"La cuenta no está activa\"); // 400
>         }
> 
>         // ... operación exitosa
>         return Ok(new { message = \"Depósito realizado\", nuevoSaldo }); // 200
>     }
>     catch (Exception ex)
>     {
>         // Log del error (en producción)
>         return StatusCode(500, new { message = \"Error interno\", detalle = ex.Message });
>     }
> }
> ```
>
> **En producción agregaría:**
> - **Middleware global de excepciones:**
> ```csharp
> app.UseExceptionHandler(\"/error\");
> app.Map(\"/error\", (HttpContext context) =>
> {
>     var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
>     // Log centralizado con Serilog
>     return Results.Problem(title: \"Error interno\", statusCode: 500);
> });
> ```
> - **Librería FluentValidation** para validaciones complejas
> - **Logging estructurado** con Serilog o NLog
> - **Respuestas consistentes** con un objeto `ApiResponse<T>`"

---

### **P15: ¿Por qué usaste `ReferenceHandler.IgnoreCycles`?**

**Respuesta:**
> "Lo usé para evitar **errores de referencia circular** en la serialización JSON:
>
> **Problema:**
> ```csharp
> // Modelo Cliente
> public class Cliente
> {
>     public int Id { get; set; }
>     public List<Cuenta> Cuentas { get; set; } // Cliente tiene cuentas
> }
> 
> // Modelo Cuenta
> public class Cuenta
> {
>     public int Id { get; set; }
>     public Cliente ClientePropietario { get; set; } // Cuenta tiene cliente
> }
> 
> // Al serializar:
> // Cliente.Cuentas[0].ClientePropietario.Cuentas[0].ClientePropietario...
> // ❌ Ciclo infinito → Error: \"A possible object cycle was detected\"
> ```
>
> **Solución en Program.cs:**
> ```csharp
> builder.Services.AddControllers()
>     .AddJsonOptions(options =>
>     {
>         options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
>         // Cuando detecta un ciclo, pone null en lugar de seguir serializando
>     });
> ```
>
> **Resultado:**
> ```json
> {
>   \"id\": 1,
>   \"nombreUsuario\": \"jperez\",
>   \"cuentas\": [
>     {
>       \"id\": 1,
>       \"numeroCuenta\": \"12345678\",
>       \"clientePropietario\": null  // ← Se cortó el ciclo
>     }
>   ]
> }
> ```
>
> **Alternativas:**
> 1. **Usar DTOs sin navegación:** Mapear entidades a DTOs sin propiedades de navegación.
> 2. **[JsonIgnore]:** Ignorar propiedades específicas.
> ```csharp
> public class Cuenta
> {
>     [JsonIgnore] // No serializar esta propiedad
>     public Cliente ClientePropietario { get; set; }
> }
> ```
> 3. **ReferenceHandler.Preserve:** Mantiene referencias con `$id` y `$ref` (más complejo)."

---

## 💡 TIPS FINALES

### Si no sabés algo...
> "Es una excelente pregunta. En este trabajo implementé [lo que hiciste], pero reconozco que en producción habría que investigar [tema que no sabés]. ¿Qué enfoque recomendarían ustedes?"

### Si te corrigen...
> "Tiene razón, gracias por la aclaración. Voy a investigar más sobre [tema] para la próxima implementación."

### Si te hacen una pregunta muy técnica...
> "No tengo esa información exacta en este momento, pero basándome en lo que implementé [explica lo que SÍ sabés]. Me gustaría profundizar ese aspecto después de la presentación."

---

**¡Estás listo para responder cualquier pregunta!** 🚀
