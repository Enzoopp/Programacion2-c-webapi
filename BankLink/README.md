# 🏦 BankLink - API REST Bancaria

## 📋 Descripción del Proyecto

**BankLink** es una API RESTful para un sistema bancario interno que permite la gestión integral de:
- 👥 **Clientes**
- 💳 **Cuentas bancarias** (Ahorro, Corriente)
- 💰 **Movimientos** (Depósitos, Retiros, Transferencias)
- 🔄 **Transferencias interbancarias** (internas y externas)
- 🏛️ **Bancos externos** (para integración)

### 🎯 Objetivos del TP Integrador
- Implementar una API REST siguiendo las mejores prácticas (GET, POST, PUT, DELETE)
- Gestionar **consistencia transaccional** en transferencias (ATOMICIDAD)
- Integración con APIs externas de otros bancos
- Validación de datos y manejo de errores con códigos HTTP apropiados
- Autenticación con JWT
- Documentación de endpoints

---

## 🚀 Tecnologías Utilizadas

- **ASP.NET Core 8.0** - Framework web
- **Entity Framework Core** - ORM para acceso a datos
- **SQL Server** - Base de datos relacional
- **JWT (JSON Web Tokens)** - Autenticación
- **BCrypt** - Hashing de contraseñas
- **Swagger/OpenAPI** - Documentación interactiva

---

## 📦 Estructura del Proyecto

```
BankLink/
├── Controllers/          # Controladores REST (endpoints)
│   ├── AuthController.cs
│   ├── ClientesController.cs
│   ├── CuentasController.cs
│   ├── MovimientosController.cs
│   ├── TransferenciasController.cs
│   └── BancosExternosController.cs
├── Models/              # Entidades del dominio
│   ├── Cliente.cs
│   ├── Cuenta.cs
│   ├── Movimiento.cs
│   ├── Transferencia.cs
│   └── BancoExterno.cs
├── Dtos/                # Data Transfer Objects
│   ├── AuthDto.cs
│   └── OperacionesDto.cs
├── Services/            # Lógica de negocio
│   ├── AuthService.cs
│   ├── ClienteDbService.cs
│   ├── CuentaDbService.cs
│   ├── MovimientoDbService.cs
│   ├── TransferenciaDbService.cs  ⭐ (Lógica transaccional)
│   └── BancoExternoDbService.cs
├── Interfaces/          # Contratos de servicios
├── Context/             # DbContext de Entity Framework
│   └── BankLinkDbContext.cs
├── Migrations/          # Migraciones de base de datos
└── Program.cs           # Configuración de la aplicación
```

---

## ⚙️ Configuración Inicial

### 1. Requisitos Previos
- .NET 8 SDK
- SQL Server (LocalDB o Express)
- Visual Studio 2022 / VS Code
- Postman (opcional, para pruebas)

### 2. Configurar Base de Datos

Edita `appsettings.json` con tu cadena de conexión:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=BankLinkDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### 3. Aplicar Migraciones

```bash
cd BankLink
dotnet ef database update
```

### 4. Ejecutar la API

```bash
dotnet run
```

La API estará disponible en: `https://localhost:5001` (o el puerto configurado)

### 5. Acceder a Swagger

Navega a: `https://localhost:5001/swagger`

---

## 🔐 Autenticación

La API utiliza **JWT (JSON Web Tokens)** para autenticación.

### 1️⃣ Registrar un Cliente

**POST** `/api/auth/register`

```json
{
  "nombre": "Juan",
  "apellido": "Pérez",
  "dni": "12345678",
  "direccion": "Av. Siempreviva 123",
  "telefono": "1234567890",
  "email": "juan@email.com",
  "nombreUsuario": "juanperez",
  "password": "MiPassword123",
  "rol": "Cliente"
}
```

**Respuesta (201 Created):**
```json
{
  "id": 1,
  "nombre": "Juan",
  "apellido": "Pérez",
  "email": "juan@email.com",
  "nombreUsuario": "juanperez"
}
```

### 2️⃣ Iniciar Sesión

**POST** `/api/auth/login`

```json
{
  "nombreUsuario": "juanperez",
  "password": "MiPassword123"
}
```

**Respuesta (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiracion": "2025-11-07T12:00:00Z",
  "usuario": {
    "id": 1,
    "nombreUsuario": "juanperez",
    "nombre": "Juan",
    "apellido": "Pérez",
    "rol": "Cliente"
  }
}
```

### 3️⃣ Usar el Token

En cada petición subsiguiente, incluye el token en el header:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 📚 Endpoints de la API

### 🔒 Nota sobre Autenticación
Todos los endpoints requieren autenticación JWT **excepto**:
- `/api/auth/register`
- `/api/auth/login`
- `/api/transferencias/recibir` (llamado por bancos externos)

---

## 👥 **CLIENTES**

### Listar todos los clientes
**GET** `/api/clientes`  
🔒 Requiere autenticación

**Respuesta (200 OK):**
```json
[
  {
    "id": 1,
    "nombre": "Juan",
    "apellido": "Pérez",
    "dni": "12345678",
    "direccion": "Av. Siempreviva 123",
    "telefono": "1234567890",
    "email": "juan@email.com",
    "nombreUsuario": "juanperez",
    "rol": "Cliente",
    "cuentas": []
  }
]
```

### Obtener cliente por ID
**GET** `/api/clientes/{id}`  
🔒 Requiere autenticación

**Ejemplo:** `/api/clientes/1`

### Obtener cliente por DNI
**GET** `/api/clientes/dni/{dni}`  
🔒 Requiere autenticación

**Ejemplo:** `/api/clientes/dni/12345678`

### Actualizar cliente
**PUT** `/api/clientes/{id}`  
🔒 Requiere autenticación

**Body:**
```json
{
  "nombre": "Juan Carlos",
  "apellido": "Pérez",
  "dni": "12345678",
  "direccion": "Av. Nueva 456",
  "telefono": "9876543210",
  "email": "juancarlos@email.com",
  "nombreUsuario": "juanperez",
  "rol": "Cliente"
}
```

### Eliminar cliente
**DELETE** `/api/clientes/{id}`  
🔒 Requiere autenticación

**Respuesta (204 No Content)**

---

## 💳 **CUENTAS**

### Listar todas las cuentas
**GET** `/api/cuentas`  
🔒 Requiere autenticación

### Obtener cuenta por ID
**GET** `/api/cuentas/{id}`  
🔒 Requiere autenticación

### Obtener cuenta por número
**GET** `/api/cuentas/numero/{numeroCuenta}`  
🔒 Requiere autenticación

**Ejemplo:** `/api/cuentas/numero/12345678`

### Obtener cuentas de un cliente
**GET** `/api/cuentas/cliente/{clienteId}`  
🔒 Requiere autenticación

**Ejemplo:** `/api/cuentas/cliente/1`

### Crear nueva cuenta
**POST** `/api/cuentas`  
🔒 Requiere autenticación

**Body:**
```json
{
  "tipoCuenta": "Ahorro",
  "idClientePropietario": 1,
  "saldoInicial": 10000.00
}
```

**Respuesta (201 Created):**
```json
{
  "id": 1,
  "numeroCuenta": "87654321",
  "tipoCuenta": "Ahorro",
  "saldoActual": 10000.00,
  "estado": "Activa",
  "fechaApertura": "2025-11-07T10:30:00",
  "idClientePropietario": 1
}
```

### Realizar Depósito
**POST** `/api/cuentas/deposito`  
🔒 Requiere autenticación

**Body:**
```json
{
  "idCuenta": 1,
  "monto": 5000.00,
  "descripcion": "Depósito en efectivo"
}
```

**Respuesta (200 OK):**
```json
{
  "message": "Depósito realizado exitosamente",
  "nuevoSaldo": 15000.00,
  "movimiento": {
    "id": 1,
    "idCuenta": 1,
    "tipoMovimiento": "Depósito",
    "monto": 5000.00,
    "fechaHora": "2025-11-07T11:00:00",
    "descripcion": "Depósito en efectivo"
  }
}
```

### Realizar Retiro
**POST** `/api/cuentas/retiro`  
🔒 Requiere autenticación

**Body:**
```json
{
  "idCuenta": 1,
  "monto": 2000.00,
  "descripcion": "Retiro por cajero"
}
```

**Validaciones:**
- ❌ Saldo insuficiente → `400 Bad Request`
- ❌ Cuenta inactiva → `400 Bad Request`

**Respuesta (200 OK):**
```json
{
  "message": "Retiro realizado exitosamente",
  "nuevoSaldo": 13000.00,
  "movimiento": { ... }
}
```

### Actualizar cuenta
**PUT** `/api/cuentas/{id}`  
🔒 Requiere autenticación

### Eliminar cuenta
**DELETE** `/api/cuentas/{id}`  
🔒 Requiere autenticación

---

## 💰 **MOVIMIENTOS**

### Listar todos los movimientos
**GET** `/api/movimientos`  
🔒 Requiere autenticación

### Obtener movimiento por ID
**GET** `/api/movimientos/{id}`  
🔒 Requiere autenticación

### Obtener movimientos de una cuenta
**GET** `/api/movimientos/cuenta/{cuentaId}`  
🔒 Requiere autenticación

**Ejemplo:** `/api/movimientos/cuenta/1`

**Respuesta (200 OK):**
```json
[
  {
    "id": 1,
    "idCuenta": 1,
    "tipoMovimiento": "Depósito",
    "monto": 10000.00,
    "fechaHora": "2025-11-07T10:30:00",
    "descripcion": "Depósito inicial al crear la cuenta"
  },
  {
    "id": 2,
    "idCuenta": 1,
    "tipoMovimiento": "Transferencia Enviada",
    "monto": 5000.00,
    "fechaHora": "2025-11-07T11:15:00",
    "descripcion": "Transferencia a cuenta 98765432"
  }
]
```

---

## 🔄 **TRANSFERENCIAS**

### ⭐ **CONSISTENCIA TRANSACCIONAL**
Todas las transferencias usan transacciones de base de datos para garantizar ATOMICIDAD:
- ✅ O se completan TODAS las operaciones (restar origen + sumar destino + registrar movimientos)
- ✅ O NINGUNA (Rollback si hay error)

### Listar todas las transferencias
**GET** `/api/transferencias`  
🔒 Requiere autenticación

### Obtener transferencia por ID
**GET** `/api/transferencias/{id}`  
🔒 Requiere autenticación

### Obtener transferencias de una cuenta
**GET** `/api/transferencias/cuenta/{cuentaId}`  
🔒 Requiere autenticación

### Transferencia Interna (entre cuentas de BankLink)
**POST** `/api/transferencias/interna`  
🔒 Requiere autenticación

**Body:**
```json
{
  "idCuentaOrigen": 1,
  "numeroCuentaDestino": "98765432",
  "monto": 5000.00,
  "descripcion": "Pago de servicios"
}
```

**Validaciones:**
- ❌ Cuenta origen no existe → `400 Bad Request`
- ❌ Cuenta origen inactiva → `400 Bad Request`
- ❌ Saldo insuficiente → `400 Bad Request`
- ❌ Cuenta destino no existe → `400 Bad Request`
- ❌ Cuenta destino inactiva → `400 Bad Request`

**Respuesta (200 OK):**
```json
{
  "message": "Transferencia interna realizada exitosamente",
  "transferencia": {
    "id": 1,
    "idCuentaOrigen": 1,
    "numeroCuentaDestino": "98765432",
    "monto": 5000.00,
    "fechaHora": "2025-11-07T11:15:00",
    "estado": "Completada",
    "descripcion": "Pago de servicios",
    "tipoTransferencia": "Enviada"
  }
}
```

### Transferencia Externa (hacia otro banco)
**POST** `/api/transferencias/externa`  
🔒 Requiere autenticación

**Body:**
```json
{
  "idCuentaOrigen": 1,
  "numeroCuentaDestino": "11223344",
  "monto": 3000.00,
  "descripcion": "Transferencia a otro banco",
  "idBancoDestino": 2
}
```

**Flujo:**
1. Disminuir saldo de cuenta origen en BankLink
2. Registrar movimiento "Transferencia Enviada"
3. Invocar API del banco externo (`POST /api/transferencias/recibir`)
4. Crear registro de transferencia

### Recibir Transferencia Externa (desde otro banco)
**POST** `/api/transferencias/recibir`  
🌐 **PÚBLICO** (No requiere autenticación JWT)

**Body:**
```json
{
  "numeroCuentaDestino": "87654321",
  "monto": 2000.00,
  "bancoOrigen": "BancoCompañero",
  "numeroCuentaOrigen": "99887766",
  "descripcion": "Transferencia desde otro banco"
}
```

**Flujo:**
1. Aumentar saldo de cuenta destino
2. Registrar movimiento "Transferencia Recibida"
3. Crear registro de transferencia

**Respuesta (200 OK):**
```json
{
  "message": "Transferencia externa recibida exitosamente",
  "transferencia": { ... }
}
```

### Transferencia Automática (detecta si es interna o externa)
**POST** `/api/transferencias/automatica`  
🔒 Requiere autenticación

**Body:**
```json
{
  "idCuentaOrigen": 1,
  "numeroCuentaDestino": "11223344",
  "monto": 1000.00,
  "descripcion": "Transferencia automática",
  "idBancoDestino": 2  // Opcional: solo si es externa
}
```

---

## 🏛️ **BANCOS EXTERNOS**

### Listar todos los bancos
**GET** `/api/bancosexternos`  
🔒 Requiere autenticación

**Respuesta (200 OK):**
```json
[
  {
    "id": 1,
    "nombreBanco": "BancoCompañero",
    "codigoIdentificacion": "BC001",
    "urlApiBase": "http://localhost:5002",
    "descripcion": "API del banco compañero para integraciones",
    "activo": true
  }
]
```

### Obtener banco por ID
**GET** `/api/bancosexternos/{id}`  
🔒 Requiere autenticación

### Obtener banco por código
**GET** `/api/bancosexternos/codigo/{codigo}`  
🔒 Requiere autenticación

### Registrar banco externo
**POST** `/api/bancosexternos`  
🔒 Requiere autenticación

**Body:**
```json
{
  "nombreBanco": "BancoCompañero",
  "codigoIdentificacion": "BC001",
  "urlApiBase": "http://localhost:5002",
  "descripcion": "API del banco compañero",
  "activo": true
}
```

### Actualizar banco
**PUT** `/api/bancosexternos/{id}`  
🔒 Requiere autenticación

### Eliminar banco
**DELETE** `/api/bancosexternos/{id}`  
🔒 Requiere autenticación

---

## 📊 Códigos de Estado HTTP

La API utiliza códigos HTTP semánticos:

| Código | Descripción | Uso |
|--------|-------------|-----|
| **200 OK** | Éxito con datos | GET, operaciones exitosas |
| **201 Created** | Recurso creado | POST exitoso |
| **204 No Content** | Éxito sin datos | PUT, DELETE exitosos |
| **400 Bad Request** | Error del cliente | Validaciones, saldo insuficiente |
| **401 Unauthorized** | No autenticado | Token JWT inválido o ausente |
| **404 Not Found** | Recurso no existe | GET de ID inexistente |
| **500 Internal Server Error** | Error del servidor | Excepciones no manejadas |

---

## 🧪 Pruebas con archivo .http

El proyecto incluye `BankLink.http` con ejemplos de todas las peticiones.

Para usar VS Code REST Client:
1. Instalar extensión "REST Client"
2. Abrir `BankLink.http`
3. Click en "Send Request"

---

## 🎓 Conceptos Técnicos Implementados

### 1. Arquitectura en Capas
- **Controllers:** Manejo de peticiones HTTP
- **Services:** Lógica de negocio
- **Context:** Acceso a datos (EF Core)
- **Models:** Entidades del dominio
- **DTOs:** Objetos de transferencia de datos

### 2. Inyección de Dependencias
Todos los servicios se registran en `Program.cs`:
```csharp
builder.Services.AddScoped<IClienteService, ClienteDbService>();
builder.Services.AddScoped<ICuentaService, CuentaDbService>();
// ...
```

### 3. Transacciones de Base de Datos (⭐ DESAFÍO DEL TP)
```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try {
    // Operaciones...
    await transaction.CommitAsync();  // ✅ Hacer permanente
} catch {
    await transaction.RollbackAsync();  // ❌ Deshacer todo
}
```

### 4. Validaciones
- **Data Annotations:** `[Required]`, `[EmailAddress]`, `[Range]`
- **ModelState:** Validación automática en controllers
- **Lógica de negocio:** Saldo suficiente, cuenta activa, etc.

### 5. Seguridad
- **JWT:** Tokens con expiración de 60 minutos
- **BCrypt:** Hashing seguro de contraseñas
- **[Authorize]:** Protección de endpoints sensibles

---

## 🚨 Manejo de Errores

### Ejemplos de Errores Comunes

#### Saldo Insuficiente
```json
{
  "message": "Error al realizar el retiro: Saldo insuficiente en cuenta origen"
}
```

#### Cuenta No Encontrada
```json
{
  "message": "No se encontró la cuenta con id: 999"
}
```

#### Token JWT Inválido
```
401 Unauthorized
```

---

## 📝 Entregas del TP

Según el documento del profesor, debes entregar:

1. ✅ **Código fuente de la API RESTful** (esta carpeta)
2. ✅ **Estructura de la base de datos** (ver Migrations/)
3. ✅ **Documentación de endpoints** (este README.md)
4. 🔄 **Pruebas con Postman** (usar BankLink.http como base)

---

## 🤝 Integración con Otros Bancos

Para recibir transferencias de compañeros:
1. Dale tu URL base: `http://localhost:XXXX`
2. Endpoint de recepción: `POST /api/transferencias/recibir`
3. No requiere autenticación JWT
4. Formato del body: `TransferenciaRecibidaDto`

Para enviar transferencias:
1. Registra el banco externo en `/api/bancosexternos`
2. Usa `/api/transferencias/externa` con `idBancoDestino`

---

## 📧 Autor

**Tu Nombre**  
TP Integrador - Programación 2  
2025

---

## 📄 Licencia

Este proyecto es parte de un trabajo práctico académico.
