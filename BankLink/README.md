# BankLink - API REST Bancaria

## 📋 Descripción del Proyecto

**BankLink** es una API REST completa para gestionar un sistema bancario interno que permite:
- Gestión integral de **clientes** y sus datos
- Administración de **cuentas bancarias** (Ahorro y Corriente)
- Registro y consulta de **movimientos** (depósitos, retiros, transferencias)
- Integración con **bancos externos** para transferencias interbancarias
- **Transferencias** internas y externas con consistencia transaccional

---

## 🏗️ Arquitectura del Proyecto

### Estructura de Carpetas
```
BankLink/
├── Controllers/          # Endpoints REST de la API
├── Models/              # Entidades de negocio
├── Context/             # DbContext de Entity Framework
├── interfaces/          # Contratos de servicios
├── Service/             # Implementaciones de lógica de negocio
├── Dtos/                # Data Transfer Objects
├── Migrations/          # Migraciones de Entity Framework
├── data/                # Archivos JSON para desarrollo
└── Properties/          # Configuración de lanzamiento
```

### Patrones de Diseño Implementados
- ✅ **Repository Pattern** (Interfaces + Servicios)
- ✅ **Dependency Injection**
- ✅ **Strategy Pattern** (FileService / DbService)
- ✅ **DTO Pattern**
- ✅ **Transaction Pattern** (Consistencia en transferencias)

---

## 📊 Modelo de Datos

### Entidades Principales

#### 1. **Cliente**
- Id, Nombre, Apellido, DNI, Dirección, Teléfono, Email
- NombreUsuario, PassHash (BCrypt), Rol
- Relación: 1 Cliente → N Cuentas

#### 2. **Cuenta**
- Id, NumeroCuenta, TipoCuenta (Ahorro/Corriente)
- SaldoActual, Estado (Activa/Inactiva), FechaApertura
- IdClientePropietario (FK)
- Relación: 1 Cuenta → N Movimientos

#### 3. **Movimiento**
- Id, IdCuenta (FK), TipoMovimiento, Monto
- FechaHora, Descripción

#### 4. **BancoExterno**
- Id, NombreBanco, CodigoIdentificacion
- UrlApiBase, Descripción, Activo

#### 5. **Transferencia**
- Id, IdCuentaOrigen (FK), NumeroCuentaDestino
- IdBancoDestino (FK, opcional), Monto
- FechaHora, Estado, TipoTransferencia, Descripción

### Relaciones
```
Cliente 1─────N Cuenta
               │
               1
               │
               N
          Movimiento

Transferencia N─────1 Cuenta (origen)
              N─────1 BancoExterno (opcional)
```

---

## 🔧 Tecnologías Utilizadas

- **ASP.NET Core 9.0** - Framework web
- **Entity Framework Core 9.0** - ORM para base de datos
- **SQL Server Express** - Base de datos
- **BCrypt.Net** - Encriptación de contraseñas
- **JWT (JSON Web Tokens)** - Autenticación
- **Swagger** - Documentación automática de la API
- **HttpClient** - Llamadas a APIs externas

---

## 🚀 Endpoints de la API

### **Auth** - Autenticación
```http
POST /api/auth/register    # Registrar nuevo cliente
POST /api/auth/login       # Iniciar sesión (obtener token JWT)
```

### **Clientes** - Gestión de Clientes
```http
GET    /api/clientes              # Obtener todos los clientes
GET    /api/clientes/{id}         # Obtener cliente por ID
GET    /api/clientes/dni/{dni}    # Obtener cliente por DNI
POST   /api/clientes              # Crear nuevo cliente
PUT    /api/clientes/{id}         # Actualizar cliente
DELETE /api/clientes/{id}         # Eliminar cliente
```

### **Cuentas** - Gestión de Cuentas
```http
GET    /api/cuentas                      # Obtener todas las cuentas
GET    /api/cuentas/{id}                 # Obtener cuenta por ID
GET    /api/cuentas/numero/{numero}      # Obtener cuenta por número
GET    /api/cuentas/cliente/{clienteId}  # Obtener cuentas de un cliente
POST   /api/cuentas                      # Crear nueva cuenta
PUT    /api/cuentas/{id}                 # Actualizar cuenta
DELETE /api/cuentas/{id}                 # Eliminar cuenta
POST   /api/cuentas/deposito             # Realizar depósito
POST   /api/cuentas/retiro               # Realizar retiro
```

### **Movimientos** - Historial de Movimientos
```http
GET    /api/movimientos                  # Obtener todos los movimientos
GET    /api/movimientos/{id}             # Obtener movimiento por ID
GET    /api/movimientos/cuenta/{cuentaId} # Movimientos de una cuenta
DELETE /api/movimientos/{id}             # Eliminar movimiento
```

### **Bancos Externos** - Administración de Bancos
```http
GET    /api/bancosexternos                    # Obtener todos los bancos
GET    /api/bancosexternos/{id}               # Obtener banco por ID
GET    /api/bancosexternos/codigo/{codigo}    # Obtener banco por código
POST   /api/bancosexternos                    # Registrar nuevo banco
PUT    /api/bancosexternos/{id}               # Actualizar banco
DELETE /api/bancosexternos/{id}               # Eliminar banco
```

### **Transferencias** - Transferencias de Fondos
```http
GET    /api/transferencias                     # Obtener todas las transferencias
GET    /api/transferencias/{id}                # Obtener transferencia por ID
GET    /api/transferencias/cuenta/{cuentaId}   # Transferencias de una cuenta
POST   /api/transferencias/interna             # Transferencia interna
POST   /api/transferencias/externa             # Transferencia a banco externo
POST   /api/transferencias/recibir             # Recibir transferencia externa
POST   /api/transferencias/automatica          # Transferencia automática
```

---

## 🔐 Características de Seguridad

1. **Autenticación JWT**
   - Tokens firmados con clave secreta
   - Expiración configurable (60 minutos por defecto)
   - Roles de usuario (Cliente, Admin)

2. **Encriptación de Contraseñas**
   - BCrypt con salt automático
   - No se almacenan contraseñas en texto plano

3. **Validaciones**
   - DataAnnotations en Models y DTOs
   - Validación de modelos en controllers
   - Restricciones de integridad referencial en BD

---

## ⚡ Funcionalidades Clave

### 1. **Depósitos y Retiros**
- Validación de saldo suficiente (retiros)
- Actualización atómica del saldo
- Registro automático de movimientos
- Validación de estado de cuenta

### 2. **Transferencias Internas**
```
1. Validar cuenta origen y destino
2. Verificar saldo suficiente
3. Iniciar transacción
4. Disminuir saldo origen
5. Aumentar saldo destino
6. Registrar movimiento en ambas cuentas
7. Crear registro de transferencia
8. Commit transacción
```

### 3. **Transferencias Externas**
```
1. Validar cuenta origen
2. Verificar saldo y banco externo
3. Iniciar transacción
4. Disminuir saldo origen
5. Registrar movimiento
6. Llamar API del banco externo (HttpClient)
7. Crear registro de transferencia
8. Commit transacción
```

### 4. **Recepción de Transferencias Externas**
```
1. Validar cuenta destino
2. Aumentar saldo
3. Registrar movimiento de entrada
4. Crear registro de transferencia recibida
```

---

## 💾 Persistencia de Datos

### Estrategia Dual

#### **Desarrollo** (FileService)
- Datos en archivos JSON en `/data`
- Rápido y fácil de debuggear
- No requiere base de datos activa

#### **Producción** (DbService)
- Base de datos SQL Server Express
- Transacciones ACID
- Integridad referencial
- Índices y optimizaciones

---

## 🔄 Consistencia Transaccional

### Implementación con Entity Framework
```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // Operaciones sobre múltiples entidades
    // Actualización de saldos
    // Registro de movimientos
    
    await transaction.CommitAsync();
}
catch (Exception)
{
    await transaction.RollbackAsync();
    throw;
}
```

### Garantías
- ✅ Atomicidad: Todo o nada
- ✅ Consistencia: Saldos siempre correctos
- ✅ Aislamiento: Transacciones concurrentes
- ✅ Durabilidad: Cambios permanentes

---

## 🧪 Testing con BankLink.http

El archivo `BankLink.http` incluye ejemplos completos de todos los endpoints:

### Flujo de Testing Recomendado
1. Registrar un cliente
2. Hacer login (obtener token)
3. Crear una cuenta
4. Realizar depósito inicial
5. Crear segunda cuenta
6. Realizar transferencia interna
7. Registrar banco externo
8. Realizar transferencia externa
9. Consultar movimientos

---

## 📦 Configuración

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=BankLinkDb;..."
  },
  "AuthOptions": {
    "Issuer": "BankLinkAPI",
    "Audience": "BankLinkClients",
    "Key": "MiClaveSecretaSuperSeguraParaBankLink12345",
    "ExpMinutes": 60
  }
}
```

---

## 🚀 Cómo Ejecutar el Proyecto

### 1. Restaurar paquetes
```bash
dotnet restore
```

### 2. Aplicar migraciones
```bash
dotnet ef database update
```

### 3. Ejecutar la aplicación
```bash
dotnet run
```

### 4. Acceder a Swagger
```
http://localhost:5000/swagger
```

---

## 📝 Notas para la Presentación Oral

### Puntos Clave a Destacar

1. **Arquitectura en Capas**
   - Separación clara de responsabilidades
   - Fácil mantenimiento y escalabilidad

2. **Patrón Repository**
   - Interfaces para abstracción
   - Múltiples implementaciones (File/DB)

3. **Consistencia Transaccional**
   - Crítico en operaciones bancarias
   - Uso de transacciones de EF Core

4. **Integración con APIs Externas**
   - HttpClient para comunicación
   - Manejo de timeouts y errores

5. **Seguridad**
   - JWT para autenticación
   - BCrypt para contraseñas
   - Validaciones en todos los niveles

6. **Escalabilidad**
   - Inyección de dependencias
   - Strategy pattern para cambiar persistencia
   - Fácil agregar nuevas funcionalidades

### Preguntas Frecuentes que Pueden Hacer

**Q: ¿Por qué usaste dos tipos de persistencia?**
A: En desarrollo uso archivos JSON para rapidez y facilidad de debugging. En producción uso SQL Server para garantizar transacciones ACID y consistencia de datos.

**Q: ¿Cómo garantizas la consistencia en transferencias?**
A: Uso transacciones de Entity Framework con `BeginTransaction`, que aseguran que todas las operaciones se completen o ninguna. Si algo falla, se hace rollback automático.

**Q: ¿Cómo manejas las llamadas a bancos externos?**
A: Uso HttpClient configurado con timeout. Si la llamada falla, la transacción local ya se completó, pero registro el error para posterior reconciliación.

**Q: ¿Qué pasa si dos usuarios transfieren al mismo tiempo?**
A: Las transacciones de SQL Server manejan la concurrencia automáticamente con locks, garantizando que los saldos sean correctos.

---

## ✅ Requisitos del TP Cumplidos

- [x] Gestión de Clientes (CRUD completo)
- [x] Gestión de Cuentas (CRUD + vinculación a clientes)
- [x] Gestión de Movimientos (depósitos, retiros, transferencias)
- [x] Administración de Bancos Externos
- [x] Ejecución de Transferencias (internas y externas)
- [x] Transferencias Enviadas (Outbound)
- [x] Transferencias Recibidas (Inbound)
- [x] Consistencia Transaccional ✨
- [x] Validaciones y manejo de errores
- [x] Códigos de estado HTTP correctos
- [x] Documentación de endpoints (BankLink.http)
- [x] Estructura de base de datos (Migrations)

---

## 👨‍💻 Autor
Proyecto desarrollado para el TP Integrador de Programación 2
