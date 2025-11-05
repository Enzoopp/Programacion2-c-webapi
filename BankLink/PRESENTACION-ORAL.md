# 🎤 GUÍA COMPLETA PRESENTACIÓN ORAL - BANKLINK API

## ⏱️ DURACIÓN RECOMENDADA: 10-15 minutos

---

## 📋 ÍNDICE DE LA PRESENTACIÓN

1. **Introducción** (1 min)
2. **Descripción del Proyecto** (2 min)
3. **Tecnologías Utilizadas** (2 min)
4. **Arquitectura y Estructura** (2-3 min)
5. **Modelos de Datos** (2 min)
6. **Servicios y Lógica de Negocio** (2 min)
7. **Demostración en Swagger** (3-4 min)
8. **Desafío: Consistencia Transaccional** (2 min)
9. **Conclusión** (1 min)

---

## 🎯 PARTE 1: INTRODUCCIÓN (1 minuto)

### **QUÉ DECIR:**

> "Buenos días/tardes. Hoy voy a presentar **BankLink**, una API REST desarrollada en ASP.NET Core para gestionar operaciones bancarias.
>
> El proyecto implementa un sistema completo que permite:
> - Gestionar clientes y sus cuentas bancarias
> - Realizar operaciones financieras (depósitos, retiros, transferencias)
> - Conectarse con bancos externos mediante APIs
> - Y lo más importante: garantizar la **consistencia transaccional** en todas las operaciones.
>
> La API está completamente funcional, persiste los datos en SQL Server, y está documentada con Swagger para facilitar las pruebas."

### **POR QUÉ ESTO FUNCIONA:**
- Das contexto general inmediatamente
- Mencionás el desafío principal (consistencia transaccional)
- Establecés que es un proyecto completo y funcional

---

## 🏗️ PARTE 2: DESCRIPCIÓN DEL PROYECTO (2 minutos)

### **QUÉ DECIR:**

> "El proyecto cumple con 5 módulos principales según los requisitos del TP:
>
> **1. Gestión de Clientes**
> - CRUD completo para clientes del banco
> - Cada cliente tiene datos personales: nombre, apellido, DNI, dirección, teléfono, email
> - Además implementé autenticación con usuario y contraseña encriptada usando BCrypt
> - Se puede buscar clientes por DNI o ID
>
> **2. Gestión de Cuentas Bancarias**
> - Las cuentas están vinculadas a clientes mediante relación 1:N (un cliente puede tener varias cuentas)
> - Cada cuenta tiene un número único de 8 dígitos generado automáticamente
> - Tipos: Ahorro o Corriente
> - Saldo actual con precisión decimal(18,2)
> - Estado: Activa o Inactiva
>
> **3. Operaciones Bancarias**
> - **Depósitos:** Suma al saldo y registra el movimiento
> - **Retiros:** Valida saldo suficiente, resta y registra
> - Todas las operaciones generan movimientos automáticamente con fecha/hora
>
> **4. Bancos Externos**
> - Registro de bancos externos con código único
> - URL de API para integración
> - Permite transferencias hacia otras instituciones
>
> **5. Transferencias**
> - **Internas:** Entre cuentas de BankLink
> - **Externas:** Hacia bancos externos usando HttpClient
> - **Recibidas:** Endpoint para que otros bancos transfieran a BankLink
> - Todas con validaciones de saldo y existencia de cuentas"

### **POR QUÉ ESTO FUNCIONA:**
- Estructura clara (5 puntos)
- Mencionás detalles técnicos sin profundizar demasiado
- Mostrás que entendés las relaciones entre entidades

---

## 💻 PARTE 3: TECNOLOGÍAS UTILIZADAS (2 minutos)

### **QUÉ DECIR:**

> "Para implementar este sistema utilicé un stack de tecnologías modernas:
>
> **Backend Framework:**
> - **ASP.NET Core 9.0** - El framework web más reciente de Microsoft
> - Elegí este framework porque es multiplataforma, de alto rendimiento y tiene soporte nativo para APIs REST
>
> **Base de Datos:**
> - **SQL Server Express** como motor de base de datos
> - **Entity Framework Core 9.0** como ORM (Object-Relational Mapper)
> - Entity Framework me permite trabajar con objetos C# en lugar de escribir SQL directamente
> - Usé el enfoque Code-First: defino las clases y EF genera la base de datos
>
> **Seguridad:**
> - **JWT (JSON Web Tokens)** para autenticación
> - **BCrypt.Net** para hashear contraseñas - nunca guardo contraseñas en texto plano
> - Los tokens JWT tienen 60 minutos de validez
>
> **Documentación:**
> - **Swagger/OpenAPI** para documentación interactiva
> - Permite probar todos los endpoints sin necesidad de Postman
>
> **Integración:**
> - **HttpClient** con factory pattern para llamar a APIs externas
> - Configurado para timeout y manejo de errores
>
> **Paquetes NuGet principales:**
> - Microsoft.EntityFrameworkCore.SqlServer (9.0.9)
> - Microsoft.AspNetCore.Authentication.JwtBearer
> - BCrypt.Net-Next
> - Swashbuckle.AspNetCore"

### **POR QUÉ ESTO FUNCIONA:**
- Nombrás tecnologías específicas con versiones
- Explicás brevemente POR QUÉ elegiste cada una
- Mostrás conocimiento técnico sin ser demasiado denso

---

## 🏛️ PARTE 4: ARQUITECTURA Y ESTRUCTURA (2-3 minutos)

### **QUÉ DECIR:**

> "Implementé una arquitectura en capas clara y mantenible:
>
> **Capa de Modelos (Models/):**
> - Contiene las entidades que se mapean a tablas de la base de datos
> - 5 modelos principales: Cliente, Cuenta, Movimiento, BancoExterno, Transferencia
> - Uso DataAnnotations para validaciones: [Required], [StringLength], [EmailAddress]
> - Las relaciones entre entidades se definen con navigation properties
>
> **Capa de DTOs (Dtos/):**
> - Data Transfer Objects para separar la lógica interna de lo que expongo en la API
> - Por ejemplo: RegisterDto para registro NO incluye el hash de contraseña
> - LoginResponseDto solo devuelve token, rol y nombre de usuario
> - Esto mejora la seguridad y evita exponer datos sensibles
>
> **Capa de Contexto (Context/):**
> - BankLinkDbContext extiende DbContext
> - Configuro todas las relaciones en OnModelCreating:
>   - 1:N entre Cliente y Cuenta
>   - 1:N entre Cuenta y Movimiento
>   - Indices únicos en DNI, NombreUsuario, NumeroCuenta
>   - Precisión decimal para montos
>   - DeleteBehavior.Restrict para evitar eliminaciones en cascada
>
> **Capa de Interfaces (interfaces/):**
> - Defino contratos para cada servicio
> - IClienteService, ICuentaService, IMovimientoService, etc.
> - Esto permite inyección de dependencias y facilita el testing
> - Puedo cambiar la implementación sin tocar los controllers
>
> **Capa de Servicios (Service/):**
> - Aquí está toda la lógica de negocio
> - Implementé dos estrategias de persistencia:
>   - FileService: Guarda en JSON (para pruebas rápidas)
>   - DbService: Guarda en SQL Server (producción)
> - En Program.cs configuré para usar siempre DbService
> - Los servicios más importantes:
>   - **AuthService:** Maneja login, registro y generación de tokens JWT
>   - **TransferenciaDbService:** Implementa la lógica transaccional compleja
>
> **Capa de Controladores (Controllers/):**
> - 6 controladores REST con atributo [ApiController]
> - Cada uno expone endpoints HTTP (GET, POST, PUT, DELETE)
> - Usan inyección de dependencias para recibir servicios
> - Retornan códigos HTTP apropiados: 200 OK, 201 Created, 400 Bad Request, 404 Not Found, 500 Internal Server Error
>
> **Migraciones (Migrations/):**
> - Entity Framework mantiene el historial de cambios en la base de datos
> - InitialCreate con fecha 20251029133450
> - Puedo recrear la base de datos en cualquier momento con `dotnet ef database update`"

### **DIAGRAMA MENTAL QUE DEBERÍAS TRANSMITIR:**

```
Request HTTP → Controller → Service → DbContext → SQL Server
                   ↓
              Valida DTO
                   ↓
           Mapea a Modelo
                   ↓
         Ejecuta lógica negocio
                   ↓
           Persiste en BD
                   ↓
         Retorna respuesta
```

### **POR QUÉ ESTO FUNCIONA:**
- Mostrás arquitectura profesional
- Mencionás patrones (inyección de dependencias, repository)
- Explicás el flujo de datos

---

## 📊 PARTE 5: MODELOS DE DATOS (2 minutos)

### **QUÉ DECIR:**

> "Voy a explicar las entidades principales y sus relaciones:
>
> **Cliente (Models/Cliente.cs):**
> ```csharp
> public class Cliente {
>     public int Id { get; set; }
>     public string Nombre { get; set; }
>     public string Apellido { get; set; }
>     public string Dni { get; set; }  // Único, índice en BD
>     public string Direccion { get; set; }
>     public string Telefono { get; set; }
>     public string Email { get; set; }
>     public string NombreUsuario { get; set; }  // Único
>     public string PassHash { get; set; }  // BCrypt
>     public string Rol { get; set; }  // Default: "Cliente"
>     public List<Cuenta> Cuentas { get; set; }  // Navigation property
> }
> ```
> - El DNI y NombreUsuario tienen índices únicos en la base de datos
> - PassHash nunca se expone en las respuestas de la API
> - La lista de Cuentas establece la relación 1:N
>
> **Cuenta (Models/Cuenta.cs):**
> ```csharp
> public class Cuenta {
>     public int Id { get; set; }
>     public string NumeroCuenta { get; set; }  // 8 dígitos, único
>     public string TipoCuenta { get; set; }  // "Ahorro" o "Corriente"
>     public decimal SaldoActual { get; set; }  // decimal(18,2)
>     public string Estado { get; set; }  // "Activa" o "Inactiva"
>     public DateTime FechaApertura { get; set; }
>     public int IdClientePropietario { get; set; }  // FK
>     public Cliente ClientePropietario { get; set; }  // Navigation
>     public List<Movimiento> Movimientos { get; set; }
> }
> ```
> - NumeroCuenta se genera automáticamente con Random (8 dígitos)
> - SaldoActual usa decimal para evitar problemas de redondeo con dinero
> - FechaApertura se setea automáticamente con DateTime.Now
>
> **Movimiento (Models/Movimiento.cs):**
> ```csharp
> public class Movimiento {
>     public int Id { get; set; }
>     public int IdCuenta { get; set; }  // FK
>     public string TipoMovimiento { get; set; }  // Tipo de operación
>     public decimal Monto { get; set; }
>     public DateTime FechaHora { get; set; }
>     public string Descripcion { get; set; }
>     public Cuenta Cuenta { get; set; }  // Navigation
> }
> ```
> - TipoMovimiento puede ser: 'Depósito', 'Retiro', 'Transferencia Enviada', 'Transferencia Recibida'
> - Se crean automáticamente, nunca manualmente
> - Actúan como auditoría de todas las operaciones
>
> **BancoExterno (Models/BancoExterno.cs):**
> ```csharp
> public class BancoExterno {
>     public int Id { get; set; }
>     public string NombreBanco { get; set; }
>     public string CodigoIdentificacion { get; set; }  // Único
>     public string UrlApiBase { get; set; }
>     public string Descripcion { get; set; }
>     public bool Activo { get; set; }
> }
> ```
> - UrlApiBase se usa para hacer llamadas HTTP a la API del banco
> - CodigoIdentificacion es único para evitar duplicados
>
> **Transferencia (Models/Transferencia.cs):**
> ```csharp
> public class Transferencia {
>     public int Id { get; set; }
>     public int IdCuentaOrigen { get; set; }  // FK
>     public int? IdBancoDestino { get; set; }  // FK nullable
>     public string NumeroCuentaDestino { get; set; }
>     public decimal Monto { get; set; }
>     public DateTime FechaHora { get; set; }
>     public string Estado { get; set; }  // Pendiente/Completada/Fallida
>     public string Descripcion { get; set; }
>     public string TipoTransferencia { get; set; }  // Enviada/Recibida
>     public Cuenta CuentaOrigen { get; set; }
>     public BancoExterno? BancoDestino { get; set; }
> }
> ```
> - IdBancoDestino es nullable porque puede ser transferencia interna
> - Estado permite tracking del proceso de transferencia
> - TipoTransferencia diferencia si es enviada o recibida"

### **POR QUÉ ESTO FUNCIONA:**
- Mostrás código real (no inventás)
- Explicás decisiones técnicas (por qué decimal, por qué nullable)
- Mencionás relaciones FK/Navigation properties

---

## ⚙️ PARTE 6: SERVICIOS Y LÓGICA DE NEGOCIO (2 minutos)

### **QUÉ DECIR:**

> "Los servicios contienen toda la lógica de negocio. Voy a explicar los más importantes:
>
> **AuthService (Service/AuthService.cs):**
> - **Login:** Valida usuario/contraseña con BCrypt.Verify
> - **Register:** Hashea la contraseña con BCrypt antes de guardar
> - **CreateToken:** Genera JWT con claims (Id, Nombre, Rol)
> - El token incluye:
>   - Issuer: 'BankLinkAPI'
>   - Audience: configurada en appsettings.json
>   - SigningCredentials con clave simétrica
>   - Expiración: 60 minutos
>
> **CuentaDbService (Service/CuentaDbService.cs):**
> - **Crear:** Genera número de cuenta único de 8 dígitos
> - **ActualizarSaldo:** Método crítico usado en depósitos/retiros
> - Valida que la cuenta exista antes de actualizar
> - Usa `SaveChangesAsync()` para persistir
>
> **MovimientoDbService (Service/MovimientoDbService.cs):**
> - Siempre crea movimientos con `FechaHora = DateTime.Now`
> - Valida que la cuenta asociada exista
> - Incluye la cuenta relacionada con `.Include(m => m.Cuenta)`
>
> **TransferenciaDbService (Service/TransferenciaDbService.cs):**
> Este es el servicio más complejo y donde implementé el desafío principal.
>
> Tiene 3 métodos principales:
>
> **1. RealizarTransferenciaInternaAsync:**
> ```csharp
> using var transaction = await _context.Database.BeginTransactionAsync();
> try {
>     // 1. Validar cuentas origen y destino existen
>     var cuentaOrigen = await _cuentaService.ObtenerPorNumeroAsync(origen);
>     var cuentaDestino = await _cuentaService.ObtenerPorNumeroAsync(destino);
>     
>     if (cuentaOrigen == null || cuentaDestino == null)
>         return null;
>     
>     // 2. Validar saldo suficiente
>     if (cuentaOrigen.SaldoActual < monto)
>         throw new InvalidOperationException("Saldo insuficiente");
>     
>     // 3. Actualizar saldos
>     await _cuentaService.ActualizarSaldo(cuentaOrigen.Id, -monto);
>     await _cuentaService.ActualizarSaldo(cuentaDestino.Id, monto);
>     
>     // 4. Registrar movimientos en ambas cuentas
>     await _movimientoService.CrearAsync(new Movimiento {
>         IdCuenta = cuentaOrigen.Id,
>         TipoMovimiento = "Transferencia Enviada",
>         Monto = monto,
>         Descripcion = descripcion
>     });
>     
>     await _movimientoService.CrearAsync(new Movimiento {
>         IdCuenta = cuentaDestino.Id,
>         TipoMovimiento = "Transferencia Recibida",
>         Monto = monto,
>         Descripcion = descripcion
>     });
>     
>     // 5. Crear registro de transferencia
>     var transferencia = new Transferencia { ... };
>     _context.Transferencias.Add(transferencia);
>     await _context.SaveChangesAsync();
>     
>     // 6. Confirmar transacción
>     await transaction.CommitAsync();
>     return transferencia;
> }
> catch (Exception) {
>     // 7. Si algo falla, deshacer TODO
>     await transaction.RollbackAsync();
>     throw;
> }
> ```
> Esta es la parte más importante: si cualquier paso falla, el Rollback deshace TODAS las operaciones.
>
> **2. RealizarTransferenciaExternaAsync:**
> - Similar a interna pero solo actualiza cuenta origen
> - Llama a la API del banco externo con HttpClient:
> ```csharp
> var httpClient = _httpClientFactory.CreateClient();
> var response = await httpClient.PostAsJsonAsync(
>     $"{banco.UrlApiBase}/transferencias/recibir",
>     new TransferenciaRecibidaDto { ... }
> );
> ```
> - Si la API externa falla, también hace Rollback
>
> **3. RecibirTransferenciaExternaAsync:**
> - Endpoint para que otros bancos transfieran a BankLink
> - Solo suma al destinatario
> - Valida que el banco origen esté registrado"

### **POR QUÉ ESTO FUNCIONA:**
- Mostrás código real del método más complejo
- Explicás paso a paso la lógica transaccional
- Demostración clara del desafío principal

---

## 🎨 PARTE 7: DEMOSTRACIÓN EN SWAGGER (3-4 minutos)

### **QUÉ DECIR ANTES DE LA DEMO:**

> "Ahora voy a demostrar el funcionamiento completo en Swagger.
> Swagger es una herramienta que genera documentación interactiva automáticamente.
> Voy a simular un flujo completo de usuario."

### **SCRIPT DE DEMO PASO A PASO:**

**PASO 1: Registrar Cliente**
- Endpoint: `POST /api/auth/register`
- JSON:
```json
{
  "nombre": "María",
  "apellido": "González",
  "dni": "98765432",
  "direccion": "Calle Falsa 123, Buenos Aires",
  "telefono": "1155667788",
  "email": "maria.gonzalez@email.com",
  "nombreUsuario": "mariag",
  "contraseña": "Password123",
  "rol": "Cliente"
}
```
- **QUÉ DECIR:** "Registro un nuevo cliente. La contraseña se hasheará automáticamente con BCrypt. El servidor devuelve un token JWT para futuras autenticaciones."

**PASO 2: Crear Primera Cuenta**
- Endpoint: `POST /api/Cuentas`
- JSON:
```json
{
  "idClientePropietario": 1,
  "tipoCuenta": "Ahorro",
  "saldoActual": 50000
}
```
- **QUÉ DECIR:** "Creo una cuenta de Ahorro con saldo inicial de $50,000. El sistema genera automáticamente un número de cuenta único de 8 dígitos."

**PASO 3: Hacer un Depósito**
- Endpoint: `POST /api/Cuentas/deposito`
- JSON:
```json
{
  "idCuenta": 1,
  "monto": 10000,
  "descripcion": "Depósito en ventanilla"
}
```
- **QUÉ DECIR:** "Deposito $10,000. El sistema actualiza el saldo de $50,000 a $60,000 y registra automáticamente un movimiento de tipo 'Depósito'."

**PASO 4: Crear Segunda Cuenta**
- Endpoint: `POST /api/Cuentas`
- JSON:
```json
{
  "idClientePropietario": 1,
  "tipoCuenta": "Corriente",
  "saldoActual": 5000
}
```
- **QUÉ DECIR:** "Creo una segunda cuenta para el mismo cliente, tipo Corriente con $5,000."

**PASO 5: Transferencia Interna**
- Endpoint: `POST /api/Transferencias/interna`
- JSON:
```json
{
  "idCuentaOrigen": 1,
  "numeroCuentaDestino": "[NÚMERO DE CUENTA 2]",
  "monto": 15000,
  "descripcion": "Transferencia entre mis cuentas"
}
```
- **QUÉ DECIR:** "Realizo una transferencia interna de $15,000 de la cuenta Ahorro a la Corriente. Aquí se activa la transacción: resta de origen, suma a destino, registra dos movimientos. Si algo falla, se deshace todo."

**PASO 6: Ver Movimientos**
- Endpoint: `GET /api/Movimientos/cuenta/1`
- **QUÉ DECIR:** "Consulto el historial de movimientos de la cuenta 1. Vemos: el saldo inicial, el depósito de $10,000 y la transferencia enviada de $15,000."

**PASO 7: Registrar Banco Externo**
- Endpoint: `POST /api/BancosExternos`
- JSON:
```json
{
  "nombreBanco": "Banco Santander",
  "codigoIdentificacion": "SANT-001",
  "urlApiBase": "https://api-santander.ejemplo.com",
  "descripcion": "Banco Santander Río",
  "activo": true
}
```
- **QUÉ DECIR:** "Registro un banco externo con su URL de API. Esto permite hacer transferencias hacia Santander."

**PASO 8: Transferencia Externa**
- Endpoint: `POST /api/Transferencias/externa`
- JSON:
```json
{
  "idCuentaOrigen": 1,
  "idBancoDestino": 1,
  "numeroCuentaDestino": "11223344",
  "monto": 5000,
  "descripcion": "Pago a proveedor"
}
```
- **QUÉ DECIR:** "Transfiero $5,000 hacia una cuenta en Santander. El sistema resta el dinero de mi cuenta y hace una llamada HTTP a la API de Santander para informar la transferencia."

**PASO 9: Consultar Todas las Transferencias**
- Endpoint: `GET /api/Transferencias`
- **QUÉ DECIR:** "Vemos todas las transferencias realizadas: la interna entre mis cuentas y la externa hacia Santander, con sus estados y fechas."

### **POR QUÉ ESTA DEMO FUNCIONA:**
- Flujo lógico y completo
- Muestra TODAS las funcionalidades principales
- Demuestra el desafío transaccional en vivo
- Los profes ven que realmente funciona

---

## 🔒 PARTE 8: DESAFÍO - CONSISTENCIA TRANSACCIONAL (2 minutos)

### **QUÉ DECIR:**

> "El desafío principal del TP era garantizar la **consistencia transaccional**.
>
> **¿Qué significa esto?**
> En un sistema bancario, las operaciones deben ser **atómicas**: o se completan TODAS las partes de una transferencia, o NINGUNA.
>
> **Ejemplo del problema:**
> Imaginen que transferimos $10,000 de la cuenta A a la cuenta B:
> 1. Resto $10,000 de cuenta A ✅
> 2. Se cae el servidor 💥
> 3. Nunca se suma a cuenta B ❌
> 4. Resultado: El dinero desapareció
>
> **Cómo lo resolví:**
> Implementé transacciones de base de datos usando Entity Framework:
>
> ```csharp
> using var transaction = await _context.Database.BeginTransactionAsync();
> ```
> Esto marca el inicio de una unidad de trabajo.
>
> Luego ejecuto todas las operaciones:
> - Validaciones
> - Actualización de saldos
> - Registro de movimientos
> - Creación de transferencia
>
> Si TODO sale bien:
> ```csharp
> await transaction.CommitAsync();
> ```
> Esto hace permanentes todos los cambios.
>
> Si ALGO falla:
> ```csharp
> await transaction.RollbackAsync();
> ```
> Esto deshace TODOS los cambios, como si nunca hubieran ocurrido.
>
> **Ventajas:**
> - ✅ Integridad de datos garantizada
> - ✅ No hay estados inconsistentes
> - ✅ Si falla algo, la base de datos queda como estaba antes
> - ✅ Cumple con propiedades ACID (Atomicity, Consistency, Isolation, Durability)
>
> **En el código:**
> Implementé esto en el método `RealizarTransferenciaInternaAsync` del `TransferenciaDbService`.
> Cada operación se ejecuta dentro del scope de la transacción, y solo se confirma cuando TODAS pasaron correctamente."

### **POR QUÉ ESTO FUNCIONA:**
- Explicás el problema claramente con ejemplo
- Mostrás la solución técnica
- Usás términos profesionales (ACID, Atomicity)
- Demostración de comprensión profunda

---

## 🎬 PARTE 9: CONCLUSIÓN (1 minuto)

### **QUÉ DECIR:**

> "En resumen, desarrollé una API REST completa para operaciones bancarias que:
>
> ✅ Implementa 5 módulos principales con más de 40 endpoints
> ✅ Persiste datos en SQL Server con Entity Framework
> ✅ Garantiza consistencia transaccional en operaciones financieras
> ✅ Implementa seguridad con JWT y BCrypt
> ✅ Está documentada con Swagger
> ✅ Se conecta con APIs externas usando HttpClient
>
> El proyecto cumple todos los requisitos del TP y está completamente funcional.
>
> Los aspectos más desafiantes fueron:
> 1. Implementar la lógica transaccional correctamente
> 2. Configurar las relaciones entre entidades en Entity Framework
> 3. Manejar la serialización JSON con referencias circulares
>
> Estoy disponible para responder preguntas."

### **POR QUÉ ESTO FUNCIONA:**
- Recap rápido de logros
- Mencionás desafíos (honestidad = credibilidad)
- Invitás a preguntas con confianza

---

## 📚 SIGUIENTES PASOS:

Ahora te voy a crear:
1. ✅ Mapa de dónde está cada archivo
2. ✅ Script detallado de Swagger
3. ✅ Guía de base de datos
4. ✅ FAQ con preguntas probables

¿Continúo?
