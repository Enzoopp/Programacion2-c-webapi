# 🗄️ GUÍA DE PRESENTACIÓN DE LA BASE DE DATOS

Esta guía contiene queries SQL para demostrar la estructura y datos de la base de datos durante la presentación.

---

## 📋 PREPARACIÓN

### 1. Conectarse a la Base de Datos

**Opción A: SQL Server Management Studio (SSMS)**
- Server: `.\SQLEXPRESS` o `localhost\SQLEXPRESS`
- Database: `BankLinkDb`
- Authentication: Windows Authentication

**Opción B: Desde la terminal (sqlcmd)**
```bash
sqlcmd -S .\SQLEXPRESS -d BankLinkDb -E
```

**Opción C: Azure Data Studio**
- Server: `.\SQLEXPRESS`
- Database: `BankLinkDb`
- Connection type: Microsoft SQL Server

---

## 🔍 QUERIES PARA LA DEMOSTRACIÓN

### **QUERY 1: Ver la Estructura de las Tablas** 📊

**QUÉ DECIR:**
> "Primero les voy a mostrar las 5 tablas principales del sistema."

**SQL:**
```sql
-- Ver todas las tablas del sistema
SELECT 
    TABLE_NAME AS [Tabla],
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) AS [Cantidad de Columnas]
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
```

**QUÉ MOSTRAR:**
- BancosExternos
- Clientes
- Cuentas
- Movimientos
- Transferencias

**QUÉ EXPLICAR:**
> "Como ven, tenemos 5 tablas que representan las entidades principales del sistema bancario."

---

### **QUERY 2: Contar Registros en Cada Tabla** 🔢

**QUÉ DECIR:**
> "Veamos cuántos registros hay en cada tabla después de la demo."

**SQL:**
```sql
-- Contar registros en cada tabla
SELECT 'Clientes' AS Tabla, COUNT(*) AS Cantidad FROM Clientes
UNION ALL
SELECT 'Cuentas', COUNT(*) FROM Cuentas
UNION ALL
SELECT 'Movimientos', COUNT(*) FROM Movimientos
UNION ALL
SELECT 'Transferencias', COUNT(*) FROM Transferencias
UNION ALL
SELECT 'BancosExternos', COUNT(*) FROM BancosExternos;
```

**QUÉ MOSTRAR:**
- Clientes: 1
- Cuentas: 2
- Movimientos: ~6-7 (dependiendo de la demo)
- Transferencias: 1-2
- BancosExternos: 1

**QUÉ EXPLICAR:**
> "Después de la demo, tenemos 1 cliente con 2 cuentas, y varios movimientos que registran todas las operaciones realizadas."

---

### **QUERY 3: Ver la Estructura de la Tabla Clientes** 👤

**QUÉ DECIR:**
> "Veamos la estructura de la tabla Clientes con sus constraints."

**SQL:**
```sql
-- Estructura de la tabla Clientes
SELECT 
    COLUMN_NAME AS [Columna],
    DATA_TYPE AS [Tipo],
    CHARACTER_MAXIMUM_LENGTH AS [Longitud],
    IS_NULLABLE AS [Permite NULL]
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Clientes'
ORDER BY ORDINAL_POSITION;
```

**QUÉ MOSTRAR:**
- Id (int, no nullable) - PK
- NombreUsuario (nvarchar, 50, no nullable) - UNIQUE
- ContraseñaHash (nvarchar, 255, no nullable)
- Nombre, Apellido (nvarchar, 100)
- Dni (nvarchar, 20, no nullable) - UNIQUE
- Email, Telefono, Direccion

**QUÉ EXPLICAR:**
> "La tabla tiene índices UNIQUE en NombreUsuario y Dni para evitar duplicados. La contraseña se guarda como hash de 255 caracteres (BCrypt genera 60, pero dejamos margen)."

---

### **QUERY 4: Ver Cliente con Contraseña Hasheada** 🔐

**QUÉ DECIR:**
> "Veamos cómo se guardó la contraseña del cliente que registramos."

**SQL:**
```sql
-- Ver cliente con contraseña hasheada
SELECT 
    Id,
    NombreUsuario,
    Nombre,
    Apellido,
    Dni,
    LEFT(ContraseñaHash, 30) + '...' AS [ContraseñaHash (primeros 30 chars)],
    LEN(ContraseñaHash) AS [Longitud del Hash]
FROM Clientes;
```

**QUÉ MOSTRAR:**
- ContraseñaHash empieza con `$2a$11$...` (formato BCrypt)
- Longitud: 60 caracteres
- No hay forma de recuperar la contraseña original

**QUÉ EXPLICAR:**
> "BCrypt genera un hash de 60 caracteres que incluye:
> - `$2a$` = algoritmo BCrypt
> - `11` = factor de costo (2^11 iteraciones)
> - 22 caracteres de salt aleatorio
> - 31 caracteres de hash
>
> Esto hace imposible descifrar la contraseña, incluso si alguien accede a la base de datos."

---

### **QUERY 5: Relación Cliente → Cuentas (1:N)** 🔗

**QUÉ DECIR:**
> "Veamos la relación entre un cliente y sus cuentas."

**SQL:**
```sql
-- Ver cliente con sus cuentas (JOIN)
SELECT 
    c.Id AS [ClienteId],
    c.NombreUsuario,
    c.Nombre + ' ' + c.Apellido AS [Nombre Completo],
    cu.Id AS [CuentaId],
    cu.NumeroCuenta,
    cu.TipoCuenta,
    cu.SaldoActual,
    cu.Estado,
    cu.FechaApertura
FROM Clientes c
INNER JOIN Cuentas cu ON c.Id = cu.IdClientePropietario
ORDER BY c.Id, cu.Id;
```

**QUÉ MOSTRAR:**
- 1 cliente con 2 cuentas
- Cada cuenta tiene su número único, tipo, saldo y estado

**QUÉ EXPLICAR:**
> "Esta es una relación 1:N (un cliente puede tener muchas cuentas). La clave foránea `IdClientePropietario` en Cuentas apunta al `Id` en Clientes. Entity Framework configuró esto con:
> ```csharp
> modelBuilder.Entity<Cuenta>()
>     .HasOne(c => c.ClientePropietario)
>     .WithMany(cl => cl.Cuentas)
>     .HasForeignKey(c => c.IdClientePropietario)
>     .OnDelete(DeleteBehavior.Restrict);
> ```
> El `DeleteBehavior.Restrict` impide borrar un cliente si tiene cuentas asociadas."

---

### **QUERY 6: Precisión Decimal para Valores Monetarios** 💰

**QUÉ DECIR:**
> "Veamos cómo se configuró la precisión decimal para los montos."

**SQL:**
```sql
-- Ver precisión de columnas decimales
SELECT 
    TABLE_NAME AS [Tabla],
    COLUMN_NAME AS [Columna],
    DATA_TYPE AS [Tipo],
    NUMERIC_PRECISION AS [Precisión],
    NUMERIC_SCALE AS [Escala]
FROM INFORMATION_SCHEMA.COLUMNS
WHERE DATA_TYPE = 'decimal'
ORDER BY TABLE_NAME, COLUMN_NAME;
```

**QUÉ MOSTRAR:**
- Cuentas.SaldoActual: decimal(18, 2)
- Movimientos.Monto: decimal(18, 2)
- Transferencias.Monto: decimal(18, 2)

**QUÉ EXPLICAR:**
> "Usamos `decimal(18, 2)` para valores monetarios:
> - 18 = dígitos totales (permite hasta 9,999,999,999,999,999.99)
> - 2 = dígitos después del punto decimal (centavos)
>
> ¿Por qué decimal y no float/double?
> - **float/double son binarios** → causan errores de redondeo (ej: 0.1 + 0.2 ≠ 0.3)
> - **decimal es base 10** → precisión exacta para dinero
> - En Entity Framework se configura con:
> ```csharp
> modelBuilder.Entity<Cuenta>()
>     .Property(c => c.SaldoActual)
>     .HasPrecision(18, 2);
> ```"

---

### **QUERY 7: Ver Movimientos de una Cuenta (Extracto)** 📋

**QUÉ DECIR:**
> "Este es el equivalente a un extracto bancario: todos los movimientos de una cuenta."

**SQL:**
```sql
-- Extracto de una cuenta específica
SELECT 
    m.Id AS [MovimientoId],
    m.TipoMovimiento,
    m.Monto,
    m.FechaHora,
    m.Descripcion,
    c.NumeroCuenta,
    c.TipoCuenta
FROM Movimientos m
INNER JOIN Cuentas c ON m.IdCuenta = c.Id
WHERE c.Id = 1  -- Cambiar por el ID de la cuenta que quieras ver
ORDER BY m.FechaHora DESC;
```

**QUÉ MOSTRAR:**
- Movimientos ordenados del más reciente al más antiguo
- Tipos: "Depósito", "Débito", "Crédito"
- Cada movimiento con timestamp exacto

**QUÉ EXPLICAR:**
> "Cada movimiento está asociado a una cuenta mediante la FK `IdCuenta`. Esta es una relación 1:N (una cuenta tiene muchos movimientos). Los tipos son:
> - **Depósito:** agregar dinero (cajero, transferencia recibida)
> - **Débito:** quitar dinero (transferencia enviada, retiro)
> - **Crédito:** recibir dinero (transferencia externa entrante)"

---

### **QUERY 8: Constraint UNIQUE en Número de Cuenta** 🔑

**QUÉ DECIR:**
> "Veamos cómo se garantiza la unicidad de los números de cuenta."

**SQL:**
```sql
-- Ver índices UNIQUE de la tabla Cuentas
SELECT 
    i.name AS [Índice],
    c.name AS [Columna],
    i.is_unique AS [Es Único]
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID('Cuentas')
AND i.is_unique = 1;
```

**QUÉ MOSTRAR:**
- Índice en `NumeroCuenta` marcado como único

**QUÉ EXPLICAR:**
> "SQL Server creó automáticamente un índice UNIQUE cuando Entity Framework configuró:
> ```csharp
> modelBuilder.Entity<Cuenta>()
>     .HasIndex(c => c.NumeroCuenta)
>     .IsUnique();
> ```
> Esto garantiza que:
> - No hay dos cuentas con el mismo número
> - Las búsquedas por número son rápidas (índice B-tree)
> - Si intentas insertar un duplicado, la BD lanza excepción"

---

### **QUERY 9: Ver Transferencias con JOIN** 🔄

**QUÉ DECIR:**
> "Veamos las transferencias realizadas con toda la información relacionada."

**SQL:**
```sql
-- Ver transferencias con datos de cuentas origen/destino
SELECT 
    t.Id AS [TransferenciaId],
    t.Monto,
    t.FechaHora,
    t.Descripcion,
    t.TipoTransferencia,
    origen.NumeroCuenta AS [Cuenta Origen],
    origen.SaldoActual AS [Saldo Actual Origen],
    destino.NumeroCuenta AS [Cuenta Destino],
    destino.SaldoActual AS [Saldo Actual Destino],
    banco.NombreBanco AS [Banco Destino (si es externa)]
FROM Transferencias t
LEFT JOIN Cuentas origen ON t.IdCuentaOrigen = origen.Id
LEFT JOIN Cuentas destino ON t.IdCuentaDestino = destino.Id
LEFT JOIN BancosExternos banco ON t.IdBancoDestino = banco.Id
ORDER BY t.FechaHora DESC;
```

**QUÉ MOSTRAR:**
- Transferencias con números de cuenta legibles
- `TipoTransferencia`: "Interna" o "Externa"
- Si es externa, muestra el banco destino
- Si es interna, `IdBancoDestino` es NULL

**QUÉ EXPLICAR:**
> "Usé LEFT JOIN porque:
> - En transferencias internas, `IdCuentaDestino` tiene valor pero `IdBancoDestino` es NULL
> - En transferencias externas, `IdBancoDestino` tiene valor pero `IdCuentaDestino` es NULL
>
> Entity Framework configuró estas relaciones opcionales con:
> ```csharp
> modelBuilder.Entity<Transferencia>()
>     .HasOne(t => t.BancoDestino)
>     .WithMany()
>     .HasForeignKey(t => t.IdBancoDestino)
>     .OnDelete(DeleteBehavior.Restrict);
> ```"

---

### **QUERY 10: Validación de Integridad Referencial** ✅

**QUÉ DECIR:**
> "Veamos todas las relaciones de clave foránea del sistema."

**SQL:**
```sql
-- Ver todas las claves foráneas (Foreign Keys)
SELECT 
    fk.name AS [Nombre FK],
    OBJECT_NAME(fk.parent_object_id) AS [Tabla Hija],
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS [Columna Hija],
    OBJECT_NAME(fk.referenced_object_id) AS [Tabla Padre],
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS [Columna Padre],
    fk.delete_referential_action_desc AS [Acción al Eliminar]
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
ORDER BY [Tabla Hija];
```

**QUÉ MOSTRAR:**
- Cuentas.IdClientePropietario → Clientes.Id (NO_ACTION)
- Movimientos.IdCuenta → Cuentas.Id (NO_ACTION)
- Transferencias.IdCuentaOrigen → Cuentas.Id (NO_ACTION)
- Transferencias.IdCuentaDestino → Cuentas.Id (NO_ACTION)
- Transferencias.IdBancoDestino → BancosExternos.Id (NO_ACTION)

**QUÉ EXPLICAR:**
> "`NO_ACTION` significa `DeleteBehavior.Restrict` en Entity Framework. Esto evita eliminaciones en cascada accidentales:
> - No puedo borrar un Cliente si tiene Cuentas
> - No puedo borrar una Cuenta si tiene Movimientos
> - No puedo borrar un BancoExterno si hay Transferencias hacia él
>
> Es una medida de protección de datos críticos."

---

### **QUERY 11: Auditoría Temporal (Historial de Cambios)** 📅

**QUÉ DECIR:**
> "Veamos el historial cronológico de operaciones del sistema."

**SQL:**
```sql
-- Timeline de todas las operaciones
SELECT 
    'Cliente Registrado' AS [Operación],
    CAST(Id AS VARCHAR) AS [Referencia],
    NULL AS [Monto],
    NULL AS [FechaHora]
FROM Clientes
UNION ALL
SELECT 
    'Cuenta Creada',
    NumeroCuenta,
    SaldoActual,
    FechaApertura
FROM Cuentas
UNION ALL
SELECT 
    'Movimiento: ' + TipoMovimiento,
    CAST(IdCuenta AS VARCHAR),
    Monto,
    FechaHora
FROM Movimientos
UNION ALL
SELECT 
    'Transferencia ' + TipoTransferencia,
    CAST(Id AS VARCHAR),
    Monto,
    FechaHora
FROM Transferencias
ORDER BY [FechaHora] DESC;
```

**QUÉ MOSTRAR:**
- Línea de tiempo completa de todas las operaciones
- Ordenadas de más reciente a más antigua

**QUÉ EXPLICAR:**
> "Esta query unifica todas las operaciones con timestamp. En un sistema real, esto podría complementarse con:
> - **Temporal Tables** de SQL Server (auditoría automática)
> - Tabla separada de Logs con usuario, IP, acción
> - Triggers para capturar cambios
> - Change Data Capture (CDC) para replicación"

---

## 🎯 DEMOSTRACIÓN DE TRANSACCIONES

### **QUERY 12: Simular Fallo de Transacción** ❌

**QUÉ DECIR:**
> "Voy a mostrar cómo una transacción se revierte si algo falla."

**SQL:**
```sql
-- Iniciar transacción manual
BEGIN TRANSACTION;

    -- Paso 1: Actualizar saldo de cuenta origen
    UPDATE Cuentas 
    SET SaldoActual = SaldoActual - 1000 
    WHERE Id = 1;

    -- Ver saldo temporal (aún no confirmado)
    SELECT Id, NumeroCuenta, SaldoActual FROM Cuentas WHERE Id = 1;

    -- Paso 2: Simular un error
    -- (Descomentar la siguiente línea para probar)
    -- THROW 50000, 'Error simulado para demostrar ROLLBACK', 1;

    -- Paso 3: Actualizar cuenta destino
    UPDATE Cuentas 
    SET SaldoActual = SaldoActual + 1000 
    WHERE Id = 2;

-- ROLLBACK para revertir todo
ROLLBACK TRANSACTION;

-- Verificar que los saldos NO cambiaron
SELECT Id, NumeroCuenta, SaldoActual FROM Cuentas WHERE Id IN (1, 2);
```

**QUÉ EXPLICAR:**
> "Como ven:
> - BEGIN TRANSACTION inicia la transacción
> - Los UPDATE modifican temporalmente los datos
> - ROLLBACK revierte TODO a su estado original
> - Los saldos quedaron iguales que antes
>
> En Entity Framework, esto se hace con:
> ```csharp
> using var transaction = await _context.Database.BeginTransactionAsync();
> try {
>     // ... operaciones ...
>     await transaction.CommitAsync();
> } catch {
>     await transaction.RollbackAsync();
> }
> ```"

---

## 📊 ESTADÍSTICAS DE LA BASE DE DATOS

### **QUERY 13: Tamaño de la Base de Datos** 💾

**QUÉ DECIR:**
> "Veamos el tamaño actual de la base de datos."

**SQL:**
```sql
-- Tamaño de la base de datos
EXEC sp_spaceused;

-- Tamaño por tabla
SELECT 
    t.name AS [Tabla],
    p.rows AS [Filas],
    SUM(a.total_pages) * 8 AS [Tamaño (KB)]
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE t.is_ms_shipped = 0
GROUP BY t.name, p.rows
ORDER BY [Tamaño (KB)] DESC;
```

**QUÉ MOSTRAR:**
- Tamaño total de BankLinkDb (probablemente ~10-20 MB con datos de demo)
- Distribución por tabla

---

## 💡 TIPS PARA LA PRESENTACIÓN

### Si te preguntan...

**"¿Por qué usaste Entity Framework y no SQL directo?"**
> "Entity Framework ofrece:
> - Seguridad contra inyección SQL (parameterización automática)
> - Mapeo objeto-relacional (POCOs en C# → tablas SQL)
> - Migraciones para versionado de esquema
> - LINQ para queries type-safe
> - Rastreo de cambios automático
>
> SQL directo es más rápido, pero requiere más código y es propenso a errores."

**"¿Qué pasa si hay dos transferencias simultáneas?"**
> "SQL Server usa niveles de aislamiento de transacciones:
> - Por defecto: READ COMMITTED
> - Si dos transacciones modifican la misma cuenta, una espera a que la otra termine (lock)
> - Esto evita el 'lost update problem'
> - Se puede optimizar con `SNAPSHOT` isolation para mejor concurrencia"

**"¿Cómo hacés backup de la base de datos?"**
> ```sql
> BACKUP DATABASE BankLinkDb
> TO DISK = 'C:\Backups\BankLinkDb.bak'
> WITH FORMAT, COMPRESSION;
> ```

---

**¡Listo para demostrar la base de datos!** 🚀
