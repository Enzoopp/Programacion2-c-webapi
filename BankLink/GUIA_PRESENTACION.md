# 🎯 GUÍA PARA LA PRESENTACIÓN ORAL DEL TP

## 📌 ESTRUCTURA SUGERIDA (10-15 minutos)

### 1️⃣ INTRODUCCIÓN (1-2 minutos)
**Qué decir:**
> "Buenos días/tardes. Hoy voy a presentar **BankLink**, una API REST bancaria que implementa todas las funcionalidades requeridas en el trabajo práctico integrador:
> - Gestión completa de clientes y cuentas bancarias
> - Operaciones de depósito y retiro con validaciones
> - **Transferencias interbancarias con consistencia transaccional** (el desafío principal del TP)
> - Integración con APIs de bancos externos
> - Autenticación con JWT y validaciones robustas"

---

### 2️⃣ ARQUITECTURA DEL PROYECTO (2 minutos)

**Mostrar:** Estructura de carpetas en VS Code

**Qué decir:**
> "La API está construida con **ASP.NET Core 8.0** y sigue una arquitectura en capas:
> - **Controllers:** Manejan las peticiones HTTP y devuelven respuestas
> - **Services:** Contienen toda la lógica de negocio
> - **Models:** Definen las entidades del dominio
> - **Context:** Acceso a la base de datos con Entity Framework Core
> - **DTOs:** Objetos para transferencia de datos entre capas
> 
> Usamos **Inyección de Dependencias** para mantener el código desacoplado y testeable."

**Mostrar:** `Program.cs` donde se registran los servicios

---

### 3️⃣ BASE DE DATOS (2 minutos)

**Mostrar:** `BankLinkDbContext.cs`

**Qué decir:**
> "Tenemos 5 tablas principales en SQL Server:
> - **Clientes:** Usuarios del banco con autenticación
> - **Cuentas:** Cuentas bancarias (Ahorro/Corriente)
> - **Movimientos:** Auditoría de todas las operaciones (depósitos, retiros, transferencias)
> - **Transferencias:** Registro de transferencias entre cuentas
> - **BancosExternos:** Catálogo de bancos para integraciones
> 
> Entity Framework genera automáticamente estas tablas a partir del código C#."

**Mostrar:** Relaciones en el DbContext (HasOne/WithMany)

**Puntos clave:**
- `DeleteBehavior.Restrict` previene eliminaciones accidentales
- Índices únicos en DNI, NumeroCuenta, NombreUsuario
- Precisión decimal(18,2) para valores monetarios

---

### 4️⃣ AUTENTICACIÓN JWT (1-2 minutos)

**Mostrar:** Swagger abierto en el navegador

**Demostración en vivo:**
1. **POST** `/api/auth/register` - Registrar un cliente
2. **POST** `/api/auth/login` - Obtener el token JWT

**Qué decir:**
> "La API usa **JWT (JSON Web Tokens)** para autenticación:
> 1. El usuario se registra con sus datos
> 2. Hace login con usuario y contraseña
> 3. Recibe un token válido por 60 minutos
> 4. Incluye ese token en todas las peticiones subsiguientes
> 
> Las contraseñas se hashean con **BCrypt** (nunca se guardan en texto plano).
> 
> Todos los endpoints están protegidos con `[Authorize]` excepto los de autenticación y el de recibir transferencias externas."

---

### 5️⃣ OPERACIONES BANCARIAS BÁSICAS (2-3 minutos)

**Demostración en vivo en Swagger/Postman:**

#### 📝 Crear Cuenta
**POST** `/api/cuentas`
```json
{
  "tipoCuenta": "Ahorro",
  "idClientePropietario": 1,
  "saldoInicial": 50000.00
}
```

**Qué decir:**
> "Al crear una cuenta:
> - Se genera un número único de 8 dígitos
> - Si hay saldo inicial, se registra automáticamente un movimiento de 'Depósito inicial'
> - Esto garantiza que todo cambio de saldo tenga un movimiento que lo justifique (auditoría)"

#### 💰 Depósito
**POST** `/api/cuentas/deposito`
```json
{
  "idCuenta": 1,
  "monto": 10000.00,
  "descripcion": "Depósito en efectivo"
}
```

**Qué decir:**
> "Cada depósito:
> 1. Valida que la cuenta existe y está activa
> 2. Actualiza el saldo sumando el monto
> 3. Registra un movimiento para auditoría
> 4. Devuelve el nuevo saldo"

#### 💸 Retiro
**POST** `/api/cuentas/retiro`
```json
{
  "idCuenta": 1,
  "monto": 5000.00,
  "descripcion": "Retiro cajero automático"
}
```

**Qué decir:**
> "Los retiros tienen una validación adicional crítica:
> - **Verifican que haya saldo suficiente**
> - Si no hay saldo, devuelve 400 Bad Request con mensaje descriptivo
> - Esto previene sobregiros no autorizados"

**Mostrar:** Intento de retiro con saldo insuficiente (debe fallar)

#### 📊 Consultar Movimientos
**GET** `/api/movimientos/cuenta/1`

**Qué decir:**
> "Este endpoint devuelve el extracto bancario de una cuenta:
> - Todos los depósitos
> - Todos los retiros
> - Todas las transferencias enviadas y recibidas
> - Ordenados por fecha, del más reciente al más antiguo"

---

### 6️⃣ ⭐ TRANSFERENCIAS - EL DESAFÍO DEL TP (3-4 minutos)

**Mostrar:** Código de `TransferenciaDbService.cs` (método `RealizarTransferenciaInternaAsync`)

**Qué decir:**
> "Este es el **corazón del trabajo práctico**: las transferencias con **consistencia transaccional**.
> 
> El desafío era garantizar que las transferencias sean **ATÓMICAS**: o se completan TODAS las operaciones o NINGUNA.
> 
> Veamos cómo lo implementé:"

#### Explicar el código paso a paso:

```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
```
> "Iniciamos una transacción de base de datos"

```csharp
try {
    // 1. Validar cuenta origen
    // 2. Validar saldo suficiente
    // 3. Validar cuenta destino
```
> "Primero hacemos TODAS las validaciones"

```csharp
    // 4. Restar de cuenta origen
    cuentaOrigen.SaldoActual -= dto.Monto;
    
    // 5. Sumar a cuenta destino
    cuentaDestino.SaldoActual += dto.Monto;
```
> "Actualizamos ambos saldos"

```csharp
    // 6. Registrar movimiento en cuenta origen
    // 7. Registrar movimiento en cuenta destino
    // 8. Crear registro de transferencia
```
> "Registramos los movimientos para auditoría"

```csharp
    await transaction.CommitAsync();
```
> "Si TODO salió bien, hacemos **COMMIT**: los cambios se hacen permanentes en la base de datos"

```csharp
} catch {
    await transaction.RollbackAsync();
    throw;
}
```
> "Si CUALQUIER operación falla, hacemos **ROLLBACK**: TODOS los cambios se deshacen.
> La base de datos queda exactamente como estaba antes de empezar.
> 
> Esto garantiza que NUNCA tengamos un estado inconsistente donde, por ejemplo:
> - Se resta dinero de una cuenta pero no se suma a la otra
> - Se actualiza un saldo pero no se registra el movimiento
> - Se pierde dinero en el sistema"

#### Demostración en vivo:

**Transferencia exitosa:**
```json
{
  "idCuentaOrigen": 1,
  "numeroCuentaDestino": "98765432",
  "monto": 5000.00,
  "descripcion": "Pago de servicios"
}
```

**Mostrar:**
1. Saldo de cuenta origen ANTES
2. Saldo de cuenta destino ANTES
3. Ejecutar transferencia
4. Saldo de cuenta origen DESPUÉS (restó el monto)
5. Saldo de cuenta destino DESPUÉS (sumó el monto)
6. Movimientos registrados en ambas cuentas

**Transferencia fallida (saldo insuficiente):**
```json
{
  "idCuentaOrigen": 1,
  "numeroCuentaDestino": "98765432",
  "monto": 999999.00,
  "descripcion": "Debería fallar"
}
```

**Mostrar:**
1. Error 400 Bad Request
2. Mensaje: "Saldo insuficiente"
3. Los saldos NO cambiaron (rollback funcionó)

---

### 7️⃣ INTEGRACIÓN CON BANCOS EXTERNOS (2 minutos)

**Qué decir:**
> "La API puede integrarse con otros bancos para transferencias externas.
> Hay dos flujos:"

#### A) Enviar dinero a otro banco
**POST** `/api/transferencias/externa`

**Proceso:**
1. Validar cuenta origen en BankLink
2. Restar dinero de nuestra cuenta
3. Registrar movimiento "Transferencia Enviada"
4. **Llamar a la API del banco externo** (consumir su endpoint)
5. Crear registro de transferencia

**Mostrar:** Código donde se hace el `HttpClient.PostAsJsonAsync()`

#### B) Recibir dinero de otro banco
**POST** `/api/transferencias/recibir`

**Qué decir:**
> "Este endpoint es **PÚBLICO** (no requiere JWT) porque lo llaman otros bancos.
> 
> Cuando nos llaman:
> 1. Validamos que la cuenta destino existe
> 2. Sumamos el dinero
> 3. Registramos el movimiento 'Transferencia Recibida'
> 4. Creamos el registro de transferencia
> 
> En una implementación real, validaríamos una API Key del banco emisor por seguridad."

---

### 8️⃣ VALIDACIONES Y CÓDIGOS HTTP (1 minuto)

**Qué decir:**
> "La API implementa validaciones robustas y devuelve códigos HTTP semánticos:"

**Mostrar ejemplos:**

| Situación | Código HTTP | Ejemplo |
|-----------|-------------|---------|
| Operación exitosa con datos | **200 OK** | GET de cuentas |
| Recurso creado | **201 Created** | POST de cuenta nueva |
| Operación exitosa sin datos | **204 No Content** | DELETE de cuenta |
| Error del cliente | **400 Bad Request** | Saldo insuficiente |
| No autenticado | **401 Unauthorized** | Sin token JWT |
| Recurso no existe | **404 Not Found** | Cuenta inexistente |
| Error del servidor | **500 Internal Server Error** | Excepción no manejada |

---

### 9️⃣ DOCUMENTACIÓN (30 segundos)

**Mostrar:** Swagger en el navegador

**Qué decir:**
> "La API tiene documentación automática con **Swagger**.
> Aquí se pueden ver todos los endpoints, probarlos interactivamente, y ver los esquemas de datos."

**Mostrar:** `README.md`

**Qué decir:**
> "También creé documentación completa en Markdown con:
> - Descripción de cada endpoint
> - Ejemplos de peticiones y respuestas
> - Guía de instalación
> - Explicación de la arquitectura"

---

### 🔟 CONCLUSIÓN Y PREGUNTAS (1 minuto)

**Qué decir:**
> "Para resumir, implementé:
> 
> ✅ Una API REST completa siguiendo las mejores prácticas
> ✅ CRUD de todas las entidades (Clientes, Cuentas, Movimientos, etc.)
> ✅ **Consistencia transaccional** en transferencias usando BeginTransaction/Commit/Rollback
> ✅ Autenticación con JWT
> ✅ Validaciones robustas con códigos HTTP apropiados
> ✅ Integración con bancos externos (enviar y recibir)
> ✅ Documentación completa
> 
> El código está en GitHub y la base de datos se puede recrear ejecutando las migraciones.
> 
> ¿Tienen alguna pregunta?"

---

## 🎓 PREGUNTAS FRECUENTES Y RESPUESTAS

### Q: ¿Por qué usaste Entity Framework en lugar de ADO.NET?
**A:** "Entity Framework es un ORM moderno que:
- Genera automáticamente las queries SQL
- Previene SQL Injection
- Simplifica el código (menos boilerplate)
- Facilita las migraciones de base de datos
- Es el estándar en proyectos .NET actuales"

### Q: ¿Cómo garantizas que no haya pérdida de dinero en las transferencias?
**A:** "Con las transacciones de base de datos:
- BeginTransaction marca el inicio
- Todas las operaciones quedan 'pendientes'
- Si hay error, Rollback las deshace
- Si todo OK, Commit las hace permanentes
- Es atómico: todo o nada"

### Q: ¿Qué pasa si se cae la conexión a mitad de una transferencia?
**A:** "La transacción automáticamente hace Rollback si:
- Hay una excepción
- Se pierde la conexión
- El proceso se interrumpe
- Entonces la BD queda consistente (como si nunca hubiéramos empezado)"

### Q: ¿Por qué el endpoint de recibir transferencias es público?
**A:** "Porque lo llaman otros bancos que no tienen nuestro token JWT.
En producción real:
- Validaríamos una API Key del banco emisor
- O usaríamos certificados SSL mutuos
- O firmas digitales de las peticiones"

### Q: ¿Cómo se registran los bancos externos?
**A:** "Hay un CRUD completo en `/api/bancosexternos` donde se registra:
- Nombre del banco
- Código de identificación único
- URL base de su API
- Estado (activo/inactivo)"

### Q: ¿Qué es el DTO y por qué lo usas?
**A:** "Data Transfer Object. Separa:
- Los modelos de base de datos (entidades)
- Los objetos que se envían/reciben en HTTP
- Esto permite validaciones específicas por operación
- Y evita exponer toda la entidad (seguridad)"

### Q: ¿Cómo funciona la autenticación JWT?
**A:** "1. Login genera un token firmado con clave secreta
2. El token contiene claims (id, username, rol)
3. El cliente lo envía en header 'Authorization: Bearer ...'
4. El middleware valida la firma y extrae el usuario
5. Expira en 60 minutos"

### Q: ¿Por qué guardas movimientos en tabla separada?
**A:** "Para auditoría y trazabilidad:
- Cumple requisitos regulatorios bancarios
- Permite generar extractos bancarios
- Facilita detectar fraudes
- Sirve para reconciliación contable
- NUNCA se eliminan (solo se consultan)"

---

## 📋 CHECKLIST PRE-PRESENTACIÓN

Antes de presentar, verifica:

- [ ] La API está corriendo (`dotnet run`)
- [ ] Swagger está accesible en `/swagger`
- [ ] SQL Server está corriendo
- [ ] La base de datos tiene las migraciones aplicadas
- [ ] Tienes al menos 2 clientes registrados
- [ ] Hay cuentas con saldo para demostrar
- [ ] Probaste todas las operaciones en Postman/Swagger
- [ ] Tienes el código abierto en puntos clave:
  - `Program.cs`
  - `TransferenciaDbService.cs` (mostrar transacción)
  - `BankLinkDbContext.cs` (mostrar relaciones)
  - `CuentasController.cs` (mostrar endpoints)
- [ ] Tienes abierto el `README.md`
- [ ] Sabes explicar el diagrama entidad-relación
- [ ] Practicaste la demo 2-3 veces

---

## 🎤 TIPS DE PRESENTACIÓN

1. **Habla claro y pausado** - Los profesores necesitan entender
2. **Mira a la cámara/profesores** - No solo a la pantalla
3. **Explica el POR QUÉ** - No solo el qué ("Use transacciones PORQUE...")
4. **Muestra código relevante** - No leas línea por línea
5. **Demuestra que funciona** - Ejecuta requests en vivo
6. **Prepara para errores** - "Si esto falla, es porque..."
7. **Sé honesto** - "Esto lo implementé así, pero podría mejorarse con..."
8. **Gestiona el tiempo** - Practica para no pasarte
9. **Ten agua cerca** - Vas a hablar mucho
10. **Relájate** - ¡Ya hiciste un gran trabajo!

---

## 🚀 BONUS: Mejoras Opcionales (si sobra tiempo)

Si quieres impresionar más:

1. **Logging:** Agregar logs con ILogger
2. **Paginación:** Implementar paginación en los GET
3. **Filtros:** Permitir filtrar movimientos por fecha
4. **Estadísticas:** Endpoint con total depositado/retirado
5. **Roles:** Diferenciar Cliente vs Admin
6. **Rate Limiting:** Limitar requests por minuto
7. **Health Check:** Endpoint `/health` para monitoring
8. **Docker:** Containerizar la aplicación

---

## 📞 ÚLTIMO CONSEJO

> **"No te pongas nervioso. Creaste una API completa, funcional, con transacciones, autenticación, validaciones y documentación. Eso es MUCHO trabajo. Los profesores verán que sabes lo que haces. Confía en tu código y explícalo con pasión. ¡Vas a aprobar!"**

🍀 **¡Mucha suerte!**
