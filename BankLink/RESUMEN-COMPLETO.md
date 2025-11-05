# 📚 RESUMEN COMPLETO - PREPARACIÓN PARA PRESENTACIÓN ORAL

Este documento es un índice de TODOS los materiales preparados para tu presentación del proyecto BankLink.

---

## ✅ ARCHIVOS CREADOS

### 1️⃣ **PRESENTACION-ORAL.md** 🎤
**Ubicación:** `BankLink/PRESENTACION-ORAL.md`  
**Propósito:** Script completo de 10-15 minutos para la presentación  
**Contenido:**
- Introducción (1 min)
- Descripción del proyecto (5 módulos)
- Tecnologías usadas (stack completo)
- Arquitectura en capas
- Modelos y relaciones (6 entidades)
- Servicios (FileService vs DbService)
- Demo de Swagger (9 pasos)
- Consistencia transaccional (ACID)
- Conclusión y cierre

**Cuándo usarlo:** Leer antes de la presentación, seguir como guía durante la exposición.

---

### 2️⃣ **MAPA-PROYECTO.md** 🗺️
**Ubicación:** `BankLink/MAPA-PROYECTO.md`  
**Propósito:** Guía de dónde está cada archivo y qué decir si preguntan  
**Contenido:**
- Estructura completa de carpetas
- Controllers/ (6 archivos con endpoints)
- Models/ (6 entidades con propiedades)
- Dtos/ (2 archivos con records)
- Context/ (BankLinkDbContext con configuración)
- interfaces/ (7 contratos)
- Service/ (12 implementaciones)
- Migrations/ (InitialCreate)
- Archivos raíz (Program.cs, appsettings.json)

**Cuándo usarlo:** Si un profesor pregunta "¿Dónde está configurado X?", consultar rápidamente.

---

### 3️⃣ **SWAGGER-DEMO.md** 🎯
**Ubicación:** `BankLink/SWAGGER-DEMO.md`  
**Propósito:** Script paso a paso para demostrar la API en vivo  
**Contenido:**
- Preparación (reset DB, iniciar app)
- 9 pasos de demo con JSONs de ejemplo:
  1. Registrar cliente
  2. Crear primera cuenta (con saldo inicial)
  3. Realizar depósito
  4. Crear segunda cuenta
  5. Transferencia interna (EXPLICAR ACID)
  6. Registrar banco externo
  7. Transferencia externa (mostrar manejo de errores)
  8. Recibir transferencia externa
  9. Consultar extracto completo
- Tips para la demo
- Qué decir si algo falla
- Cierre de la demo

**Cuándo usarlo:** Durante la parte práctica de la presentación con Swagger abierto.

---

### 4️⃣ **DB-GUIDE.md** 🗄️
**Ubicación:** `BankLink/DB-GUIDE.md`  
**Propósito:** Queries SQL para demostrar la base de datos  
**Contenido:**
- 13 queries con explicaciones:
  1. Ver estructura de tablas
  2. Contar registros
  3. Estructura de Clientes
  4. Ver contraseña hasheada (BCrypt)
  5. Relación Cliente→Cuentas (JOIN)
  6. Precisión decimal para dinero
  7. Extracto de movimientos
  8. Constraint UNIQUE en número de cuenta
  9. Transferencias con LEFT JOIN
  10. Claves foráneas (FK constraints)
  11. Auditoría temporal
  12. Simulación de transacción con ROLLBACK
  13. Estadísticas de la BD
- Preparación (cómo conectarse)
- Qué explicar en cada query

**Cuándo usarlo:** Si tienes tiempo extra o te piden ver la base de datos directamente.

---

### 5️⃣ **FAQ.md** ❓
**Ubicación:** `BankLink/FAQ.md`  
**Propósito:** Respuestas a preguntas probables de los profesores  
**Contenido:**
- **15 preguntas técnicas con respuestas detalladas:**

**Seguridad:**
- ¿Por qué JWT y no sesiones? (stateless, escalable, mobile-friendly)
- ¿Cómo garantizás seguridad de contraseñas? (BCrypt con salt)
- ¿Cómo prevenís inyección SQL? (EF parametriza automáticamente)

**Base de Datos:**
- ¿Por qué decimal y no double? (precisión exacta para dinero)
- ¿Qué es DeleteBehavior.Restrict? (evita borrados en cascada)
- ¿Qué son las migraciones? (versionado de esquema)

**Transacciones:**
- ¿Cómo garantizás consistencia? (BeginTransaction/Commit/Rollback, ACID)
- ¿Qué pasa con transferencias simultáneas? (locks, isolation levels)

**APIs Externas:**
- ¿Qué pasa si la API externa falla? (rollback automático, timeout 30s)
- ¿Por qué HttpClientFactory? (reutilización de conexiones, pool de handlers)

**Arquitectura:**
- ¿Por qué separaste en capas? (testeable, reusable, mantenible)
- ¿Qué es Dependency Injection? (inversión de control, acoplamiento débil)
- ¿Qué son los DTOs? (seguridad, validación, flexibilidad)
- ¿Cómo manejas errores? (códigos HTTP semánticos, try-catch)
- ¿Por qué ReferenceHandler.IgnoreCycles? (evita referencias circulares en JSON)

**Cuándo usarlo:** Leer ANTES de la presentación, tener cerca durante la exposición.

---

### 6️⃣ **reset-db.bat** 🔄
**Ubicación:** `BankLink/reset-db.bat`  
**Propósito:** Limpiar base de datos antes de la demo  
**Contenido:**
```batch
cd /d "c:\Users\enzop\OneDrive\Documentos\GitHub\Programacion2-c-webapi\BankLink"
dotnet ef database drop --force
dotnet ef database update
sqlcmd -S .\SQLEXPRESS -d BankLinkDb -Q "SELECT COUNT(*) FROM Clientes;"
```

**Cuándo usarlo:** Ejecutar 5 minutos antes de la presentación para tener BD limpia.

---

## 📝 ARCHIVOS COMENTADOS (CÓDIGO)

### 7️⃣ **Program.cs** ⚙️
**Estado:** ✅ Comentado completamente  
**Secciones explicadas (11):**
1. Builder creation
2. DbContext configuration
3. Controllers + ReferenceHandler.IgnoreCycles
4. Swagger/OpenAPI
5. JWT Authentication con TokenValidationParameters
6. HttpClient factory
7. DI service registration (Scoped/Singleton)
8. Build
9. Middleware pipeline order
10. MapControllers
11. Run

**Notas para presentación:** Incluidas al final del archivo

---

### 8️⃣ **TransferenciaDbService.cs** 🔄
**Estado:** ✅ Comentado completamente  
**Métodos explicados:**
- `RealizarTransferenciaInternaAsync` (10 pasos con BeginTransaction/Commit/Rollback)
- `RealizarTransferenciaExternaAsync` (HttpClient con timeout)
- `RecibirTransferenciaExternaAsync` (recibir de bancos externos)

**Highlight:** Explicación paso a paso del flujo transaccional (EL TEMA PRINCIPAL DEL TP)

**Notas para presentación:** 5 puntos clave sobre ACID y transacciones

---

### 9️⃣ **BankLinkDbContext.cs** 🗄️
**Estado:** ✅ Comentado completamente  
**Configuraciones explicadas:**
- DbSets (mapeo a tablas)
- OnModelCreating con 3 secciones:
  1. Relationships (HasOne/WithMany, DeleteBehavior.Restrict)
  2. Decimal Precision (HasPrecision(18, 2) para montos)
  3. Unique Indexes (Dni, NombreUsuario, NumeroCuenta)

**Notas para presentación:** Por qué decimal y no float, por qué Restrict

---

### 🔟 **AuthService.cs** 🔐
**Estado:** ✅ Comentado completamente  
**Métodos explicados:**
- `CreateToken` (6 pasos: claims → key → credentials → expiration → JwtSecurityToken → serialize)
- `Login` (BCrypt.Verify con explicación de seguridad)

**Highlight:** JWT structure (HEADER.PAYLOAD.SIGNATURE), BCrypt security (salt, rainbow tables)

**Notas para presentación:** Por qué JWT, por qué BCrypt, por qué HmacSha256

---

### 1️⃣1️⃣ **CuentasController.cs** 💳
**Estado:** ✅ Comentado completamente  
**Endpoints explicados (9):**
- GET /api/Cuentas (listar todas)
- GET /api/Cuentas/{id} (por ID)
- GET /api/Cuentas/numero/{numero} (buscar por número)
- GET /api/Cuentas/cliente/{id} (cuentas de un cliente)
- POST /api/Cuentas (crear con saldo inicial)
- POST /api/Cuentas/deposito (actualizar saldo + movimiento)
- POST /api/Cuentas/retiro (validar saldo + actualizar + movimiento)
- PUT /api/Cuentas/{id} (actualizar)
- DELETE /api/Cuentas/{id} (eliminar)

**Método auxiliar:** `GenerarNumeroCuenta()` con algoritmo de unicidad

**Notas para presentación:** 8 puntos sobre operaciones bancarias, auditoría, códigos HTTP

---

## 📊 RESUMEN DE CONTENIDO

### Materiales de Presentación
| Archivo | Páginas | Tiempo | Propósito |
|---------|---------|--------|-----------|
| PRESENTACION-ORAL.md | ~8 | 10-15 min | Script de presentación |
| MAPA-PROYECTO.md | ~10 | Consulta | Ubicación de archivos |
| SWAGGER-DEMO.md | ~12 | 5-7 min | Demo práctica |
| DB-GUIDE.md | ~15 | 5-10 min | Queries SQL (opcional) |
| FAQ.md | ~20 | Pre-lectura | Respuestas a preguntas |

### Código Comentado
| Archivo | Líneas | Comentarios | Tema Principal |
|---------|--------|-------------|----------------|
| Program.cs | 215 | 11 secciones | Configuración |
| TransferenciaDbService.cs | 319 | 10 pasos | Transacciones ACID |
| BankLinkDbContext.cs | 185 | 3 secciones | EF Core |
| AuthService.cs | 200+ | 6 pasos | JWT + BCrypt |
| CuentasController.cs | 274 | 9 endpoints | Operaciones bancarias |

---

## 🎯 ESTRATEGIA DE PRESENTACIÓN

### Orden Recomendado (15 minutos)

**1. INTRODUCCIÓN (1 min)**
- Leer PRESENTACION-ORAL.md sección 1

**2. DESCRIPCIÓN DEL PROYECTO (2 min)**
- Leer PRESENTACION-ORAL.md sección 2
- Mencionar los 5 módulos principales

**3. TECNOLOGÍAS (2 min)**
- Leer PRESENTACION-ORAL.md sección 3
- Explicar stack completo

**4. ARQUITECTURA (2 min)**
- Mostrar MAPA-PROYECTO.md visualmente
- Explicar capas (Controllers → Services → DbContext)

**5. DEMO CON SWAGGER (5-7 min) ⭐ PARTE CLAVE**
- Seguir SWAGGER-DEMO.md paso a paso
- **IMPRESCINDIBLE:** Explicar detalladamente la transferencia interna (Paso 5)
  - Mencionar ACID
  - Explicar BeginTransaction/Commit/Rollback
  - Mostrar el código comentado en TransferenciaDbService.cs
- Opcional: Mostrar transferencia externa fallida (Paso 7)

**6. CÓDIGO (3 min) ⭐ SI TE PIDEN VER CÓDIGO**
- Abrir TransferenciaDbService.cs comentado
- Mostrar el método RealizarTransferenciaInternaAsync
- Leer los comentarios clave (10 pasos)

**7. CONCLUSIÓN (1 min)**
- Leer PRESENTACION-ORAL.md sección 9
- Invitar a preguntas

---

## ❓ SI TE PREGUNTAN...

### "Mostranos la base de datos"
→ Abrir SSMS o Azure Data Studio  
→ Seguir DB-GUIDE.md queries 1, 2, 5, 6, 7, 9  
→ Explicar relaciones y precisión decimal

### "¿Cómo garantizás la seguridad?"
→ Consultar FAQ.md preguntas P1 (JWT), P2 (BCrypt), P3 (SQL injection)  
→ Mostrar AuthService.cs comentado

### "¿Qué pasa si falla una transferencia?"
→ Consultar FAQ.md pregunta P7 (ACID)  
→ Mostrar TransferenciaDbService.cs con try/catch/rollback  
→ Opcional: Ejecutar DB-GUIDE.md query 12 (simulación de rollback)

### "¿Por qué usaste [tecnología X]?"
→ Consultar FAQ.md (cubre: JWT, BCrypt, decimal, EF, HttpClient, DI, DTOs)

### "¿Cómo manejas concurrencia?"
→ Consultar FAQ.md pregunta P8 (locks, isolation levels)

---

## 📱 CHECKLIST ANTES DE LA PRESENTACIÓN

### 30 minutos antes
- [ ] Ejecutar `reset-db.bat` para limpiar la base de datos
- [ ] Iniciar la aplicación: `cd BankLink && dotnet run`
- [ ] Verificar que Swagger funciona: http://localhost:5193/swagger
- [ ] Tener PRESENTACION-ORAL.md abierto en una pantalla
- [ ] Tener FAQ.md abierto en otra pestaña
- [ ] Tener SWAGGER-DEMO.md con los JSONs listos para copiar/pegar

### 10 minutos antes
- [ ] Leer FAQ.md completo (15 preguntas)
- [ ] Repasar PRESENTACION-ORAL.md sección 7 (demo Swagger)
- [ ] Verificar que SQL Server Express está corriendo
- [ ] Cerrar pestañas innecesarias del navegador

### Durante la presentación
- [ ] Seguir PRESENTACION-ORAL.md como guía
- [ ] Al hacer la demo, seguir SWAGGER-DEMO.md paso a paso
- [ ] Si preguntan, consultar FAQ.md o MAPA-PROYECTO.md
- [ ] Si piden ver código, mostrar archivos comentados

---

## 💡 FRASES CLAVE PARA REPETIR

### Sobre Transacciones (EL TEMA PRINCIPAL)
> "Implementé transacciones explícitas con BeginTransaction/Commit/Rollback para garantizar las propiedades ACID: Atomicidad (todo o nada), Consistencia (reglas de negocio), Aislamiento (concurrencia) y Durabilidad (cambios permanentes)."

### Sobre Arquitectura
> "Usé arquitectura en capas: Controllers reciben HTTP requests, Services contienen la lógica de negocio, DbContext accede a la base de datos, y Models definen las entidades del dominio. Esto hace el código testeable, reusable y mantenible."

### Sobre Seguridad
> "Las contraseñas se hashean con BCrypt, que es lento por diseño para prevenir ataques de fuerza bruta. Además, Entity Framework previene inyección SQL mediante parametrización automática, y JWT permite autenticación stateless escalable."

### Sobre la Demo
> "En esta transferencia, pueden ver cómo el sistema valida ambas cuentas, verifica saldo suficiente, actualiza los balances, registra movimientos para auditoría, y si algo falla en cualquier paso, hace rollback automático para evitar inconsistencias."

---

## 🎓 RECORDATORIOS FINALES

1. **Habla despacio y con confianza:** Conocés el código porque lo tenés comentado.
2. **Usá los materiales:** No memorices, tenés guías para consultar.
3. **La demo es la estrella:** La parte de Swagger con transferencias es lo más importante.
4. **Si no sabés algo:** Consultá FAQ.md o decí "excelente pregunta, déjame verificar en el código".
5. **Mostrá el código comentado:** Los profesores van a valorar que hayas documentado todo.

---

## 📞 SI ALGO FALLA

### La API no inicia
```bash
# Verificar puerto ocupado
netstat -ano | findstr :5193

# Matar proceso
taskkill /PID <PID> /F

# Reiniciar
dotnet run
```

### SQL Server no responde
```bash
# Verificar servicio
net start MSSQL$SQLEXPRESS
```

### Error en Swagger
- Verificar que `launchSettings.json` tenga la URL correcta
- Abrir en modo Incógnito si hay problemas de cache

---

**¡ÉXITO EN TU PRESENTACIÓN!** 🚀

**Recordá:** Tenés 5 archivos de guía + 5 archivos de código comentado = **10 recursos** para responder cualquier cosa que te pregunten.

**Estrategia:** "Mientras hable más sobre todo, menos preguntas me pueden hacer los profes" → Seguí PRESENTACION-ORAL.md detalladamente y explicá cada concepto a fondo. Los materiales te cubren.
