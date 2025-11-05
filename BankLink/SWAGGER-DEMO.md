# 🎯 GUÍA DE DEMOSTRACIÓN CON SWAGGER

Esta es una guía paso a paso para demostrar la API BankLink usando Swagger.  
**Duración estimada:** 5-7 minutos  
**URL:** http://localhost:5193/swagger

---

## 📋 PREPARACIÓN ANTES DE LA DEMO

### 1. Limpiar la Base de Datos
```bash
# Ejecutar el script reset-db.bat para empezar con BD vacía
cd c:\Users\enzop\OneDrive\Documentos\GitHub\Programacion2-c-webapi\BankLink
.\reset-db.bat
```

### 2. Iniciar la Aplicación
```bash
dotnet run
```

### 3. Abrir Swagger
Navegar a: **http://localhost:5193/swagger**

---

## 🎬 SECUENCIA DE DEMOSTRACIÓN

### **PASO 1: Registrar un Cliente** 🆕
**Endpoint:** `POST /api/Auth/register`

**QUÉ DECIR:**
> "Primero vamos a registrar un nuevo cliente en el sistema. Este endpoint crea el cliente y automáticamente genera un hash seguro de la contraseña usando BCrypt."

**JSON a enviar:**
```json
{
  "nombreUsuario": "jperez",
  "contraseña": "Demo2025!",
  "nombre": "Juan",
  "apellido": "Pérez",
  "dni": "12345678",
  "email": "jperez@example.com",
  "telefono": "+549111234567",
  "direccion": "Av. Corrientes 1234, CABA"
}
```

**Click:** "Execute"

**QUÉ MOSTRAR:**
- HTTP 200 OK
- Respuesta con el cliente creado (sin la contraseña hasheada)
- **Copiar el `id` del cliente** (lo necesitarás en los siguientes pasos)

**QUÉ EXPLICAR:**
> "Como ven, la contraseña no se devuelve en la respuesta por seguridad. BCrypt la convirtió en un hash irreversible de 60 caracteres."

---

### **PASO 2: Crear Primera Cuenta** 💳
**Endpoint:** `POST /api/Cuentas`

**QUÉ DECIR:**
> "Ahora vamos a crear una cuenta de ahorro para Juan con un saldo inicial de $50,000."

**JSON a enviar:**
```json
{
  "idClientePropietario": 1,
  "tipoCuenta": "Ahorro",
  "saldoInicial": 50000
}
```
⚠️ **IMPORTANTE:** Reemplaza `1` con el `id` del cliente que copiaste en el Paso 1.

**Click:** "Execute"

**QUÉ MOSTRAR:**
- HTTP 201 Created
- Header "Location" con la URL del nuevo recurso
- `numeroCuenta` generado automáticamente (8 dígitos)
- `saldoActual` = 50000
- **Copiar el `id` de la cuenta** (lo necesitarás más adelante)
- **Copiar el `numeroCuenta`** (lo usarás para transferencias)

**QUÉ EXPLICAR:**
> "El endpoint retorna HTTP 201 Created con un header 'Location' que indica dónde encontrar el recurso creado. El número de cuenta se generó automáticamente usando un algoritmo que garantiza unicidad."

**BONUS:** Ir al endpoint `GET /api/Movimientos/cuenta/{id}` para mostrar el movimiento inicial:

**QUÉ DECIR:**
> "Si consulto los movimientos de esta cuenta, verán que automáticamente se creó un movimiento de 'Depósito inicial' de $50,000. Esto garantiza la auditoría completa."

---

### **PASO 3: Realizar un Depósito** 💰
**Endpoint:** `POST /api/Cuentas/deposito`

**QUÉ DECIR:**
> "Voy a hacer un depósito de $10,000 en la cuenta que acabo de crear."

**JSON a enviar:**
```json
{
  "idCuenta": 1,
  "monto": 10000,
  "descripcion": "Depósito por transferencia"
}
```
⚠️ **IMPORTANTE:** Reemplaza `1` con el `id` de la cuenta.

**Click:** "Execute"

**QUÉ MOSTRAR:**
- HTTP 200 OK
- `nuevoSaldo`: 60000 (50000 + 10000)
- Objeto `movimiento` con el registro del depósito

**QUÉ EXPLICAR:**
> "Como ven, el saldo se actualizó a $60,000 y automáticamente se registró un movimiento. Cada cambio de saldo SIEMPRE genera un movimiento para mantener la trazabilidad."

---

### **PASO 4: Crear Segunda Cuenta** 💳💳
**Endpoint:** `POST /api/Cuentas`

**QUÉ DECIR:**
> "Para demostrar las transferencias, voy a crear una segunda cuenta corriente para el mismo cliente, esta vez sin saldo inicial."

**JSON a enviar:**
```json
{
  "idClientePropietario": 1,
  "tipoCuenta": "Corriente",
  "saldoInicial": 0
}
```

**Click:** "Execute"

**QUÉ MOSTRAR:**
- Cuenta creada con `saldoActual`: 0
- Nuevo `numeroCuenta` diferente al anterior
- **Copiar el `numeroCuenta` de la segunda cuenta**

**QUÉ EXPLICAR:**
> "Esta cuenta se creó con saldo 0, por lo que NO se generó un movimiento inicial (solo se registran movimientos si hay cambio de saldo)."

---

### **PASO 5: Transferencia Interna** 🔄
**Endpoint:** `POST /api/Transferencias/interna`

**QUÉ DECIR:**
> "Ahora viene la parte más importante del trabajo práctico: voy a hacer una transferencia interna de $20,000 desde la primera cuenta a la segunda. Esta operación usa transacciones de base de datos para garantizar consistencia."

**JSON a enviar:**
```json
{
  "numeroCuentaOrigen": "12345678",
  "numeroCuentaDestino": "87654321",
  "monto": 20000,
  "descripcion": "Transferencia entre mis cuentas"
}
```
⚠️ **IMPORTANTE:** Reemplaza los números de cuenta con los que copiaste.

**Click:** "Execute"

**QUÉ MOSTRAR:**
- HTTP 200 OK
- `saldoOrigen`: 40000 (60000 - 20000)
- `saldoDestino`: 20000 (0 + 20000)
- Objeto `transferencia` con el registro completo

**QUÉ EXPLICAR (MUY IMPORTANTE):**
> "Esta operación hizo 5 cosas dentro de una TRANSACCIÓN de base de datos:
> 1. Validó que ambas cuentas existan y estén activas
> 2. Verificó que la cuenta origen tenga saldo suficiente
> 3. Actualizó el saldo de ambas cuentas
> 4. Registró 2 movimientos (uno de débito, otro de crédito)
> 5. Creó el registro de transferencia
>
> Si cualquiera de estos pasos fallaba, TODO se revertía (ROLLBACK). Esto garantiza las propiedades ACID: Atomicidad (todo o nada), Consistencia (reglas de negocio), Aislamiento (concurrencia) y Durabilidad (cambios permanentes)."

**BONUS:** Mostrar los movimientos de ambas cuentas:

**Endpoint:** `GET /api/Movimientos/cuenta/{id}`

**QUÉ DECIR:**
> "Si reviso los movimientos de la cuenta origen, veo un movimiento de tipo 'Débito' por $20,000. Y en la cuenta destino hay un 'Crédito' por $20,000. Esto es el equivalente al extracto bancario."

---

### **PASO 6: Registrar Banco Externo** 🏦
**Endpoint:** `POST /api/BancosExternos`

**QUÉ DECIR:**
> "Para demostrar transferencias externas, primero debo registrar un banco externo en el sistema."

**JSON a enviar:**
```json
{
  "nombreBanco": "Banco Galicia",
  "codigoIdentificacion": "GALICIA",
  "urlApi": "https://api-banco-galicia-demo.com/transferencias",
  "tokenAutorizacion": "Bearer demo-token-galicia-2025"
}
```

**Click:** "Execute"

**QUÉ MOSTRAR:**
- HTTP 201 Created
- Banco creado con `estado`: "Activo"
- **Copiar el `id` del banco**

---

### **PASO 7: Transferencia Externa** 🌐
**Endpoint:** `POST /api/Transferencias/externa`

**QUÉ DECIR:**
> "Ahora voy a hacer una transferencia de $5,000 desde mi cuenta de BankLink hacia una cuenta en Banco Galicia. Esta operación hace una llamada HTTP al API del banco externo."

**JSON a enviar:**
```json
{
  "numeroCuentaOrigen": "12345678",
  "idBancoDestino": 1,
  "numeroCuentaDestinoExterna": "9999888877776666",
  "monto": 5000,
  "descripcion": "Pago a proveedor externo"
}
```
⚠️ **IMPORTANTE:** Reemplaza `numeroCuentaOrigen` e `idBancoDestino`.

**Click:** "Execute"

**QUÉ MOSTRAR:**
- ⚠️ **Probablemente falle con HTTP 400**: "Error al comunicarse con el banco externo"
- Esto es ESPERADO porque la URL del banco externo es ficticia

**QUÉ EXPLICAR (ESTO ES CLAVE):**
> "Como ven, falló porque la API del banco externo no existe realmente. PERO lo importante es que el código detectó el error y NO modificó el saldo de mi cuenta. Esto demuestra el manejo correcto de errores:
>
> - Se inició una transacción
> - Se validó la cuenta origen
> - Se intentó llamar al banco externo con HttpClient (timeout 30 segundos)
> - Al detectar el error, se hizo ROLLBACK
> - La cuenta quedó exactamente igual que antes
>
> En producción real, si el banco externo responde OK, se confirmaría la transacción y se actualizaría el saldo."

**ALTERNATIVA (si quieres mostrar éxito):**
> "Para demostrar el flujo completo, podríamos crear un banco externo MOCK con una API de prueba que siempre responda OK. O podríamos usar el endpoint `/api/Transferencias/recibir` para simular que OTRO banco nos envía dinero a nosotros."

---

### **PASO 8: Recibir Transferencia Externa (OPCIONAL)** 📥
**Endpoint:** `POST /api/Transferencias/recibir`

**QUÉ DECIR:**
> "Este endpoint simula que OTRO banco (por ejemplo, Banco Macro) nos está enviando una transferencia. En la vida real, este endpoint sería llamado por la API de Banco Macro."

**JSON a enviar:**
```json
{
  "numeroCuentaDestino": "87654321",
  "bancoOrigen": "Banco Macro",
  "monto": 15000,
  "referencia": "REF-MACRO-00123",
  "descripcion": "Cobro de cliente externo"
}
```
⚠️ **IMPORTANTE:** Reemplaza `numeroCuentaDestino` con la segunda cuenta.

**Click:** "Execute"

**QUÉ MOSTRAR:**
- HTTP 200 OK
- Saldo de la cuenta destino aumentó en $15,000
- Movimiento registrado como "Crédito - Transferencia Externa"

**QUÉ EXPLICAR:**
> "En este caso SÍ funcionó porque no hay comunicación externa. Simplemente validamos que la cuenta destino existe, actualizamos su saldo y registramos el movimiento. El banco origen nos envió los datos necesarios."

---

### **PASO 9: Consultar Extracto Completo** 📊
**Endpoint:** `GET /api/Movimientos/cuenta/{id}`

**QUÉ DECIR:**
> "Por último, voy a mostrar el extracto completo de la primera cuenta para ver todos los movimientos."

**Parámetro:** `id` de la primera cuenta

**Click:** "Execute"

**QUÉ MOSTRAR:**
- Lista de movimientos con:
  - Depósito inicial ($50,000)
  - Depósito por transferencia ($10,000)
  - Débito por transferencia interna (-$20,000)
  - (Si hiciste la transferencia externa exitosa) Débito por transferencia externa (-$5,000)
- Cada movimiento con fecha/hora, tipo, monto y descripción

**QUÉ EXPLICAR:**
> "Como ven, hay un registro completo de TODOS los movimientos. Esto cumple con:
> - Requisitos regulatorios bancarios
> - Auditoría interna
> - Transparencia hacia el cliente
> - Detección de fraudes o errores
>
> Ningún cambio de saldo puede quedar sin justificación."

---

## 📊 RESUMEN DE LA DEMO

**Lo que demostraste:**
1. ✅ Registro de clientes con hash de contraseñas
2. ✅ Creación de cuentas con números únicos
3. ✅ Operaciones bancarias básicas (depósito)
4. ✅ Transferencias internas con transacciones ACID
5. ✅ Integración con APIs externas (aunque falló intencionalmente)
6. ✅ Auditoría completa con movimientos
7. ✅ Manejo de errores (rollback ante fallos)

---

## 💡 TIPS PARA LA DEMO

### Qué hacer SI...

**...algo falla inesperadamente:**
> "Esto es justamente lo que queríamos demostrar: cuando hay un error, la transacción se revierte automáticamente. Veamos qué pasó en la base de datos..."

**...te preguntan por la seguridad:**
> "Las contraseñas se hashean con BCrypt (algoritmo lento que previene ataques de fuerza bruta). Las validaciones evitan sobregiros. Las transacciones previenen inconsistencias. Y en producción usaríamos HTTPS y autenticación JWT."

**...te preguntan por concurrencia:**
> "Entity Framework usa transacciones con nivel de aislamiento READ COMMITTED por defecto. Esto significa que si dos usuarios intentan transferir desde la misma cuenta al mismo tiempo, uno esperará a que el otro termine. Esto evita el 'lost update problem'."

**...te preguntan qué falta:**
> "En producción agregaríamos: autenticación JWT obligatoria, límites de transferencia diaria, notificaciones por email/SMS, logs de auditoría, respaldos automáticos, manejo de concurrencia con versiones optimistas, pruebas unitarias y de integración."

---

## 🎤 CIERRE DE LA DEMO

**QUÉ DECIR:**
> "En resumen, este trabajo práctico implementa un sistema bancario básico pero funcional que demuestra:
> - Arquitectura en capas (Controllers → Services → DbContext)
> - Transacciones de base de datos para garantizar consistencia
> - Integración con APIs externas
> - Auditoría completa de operaciones
> - Manejo de errores y validaciones
>
> ¿Tienen alguna pregunta sobre el código o la arquitectura?"

---

## 📝 NOTAS ADICIONALES

- **Tiempo de ejecución:** Cada endpoint tarda menos de 1 segundo (excepto transferencias externas que tienen timeout de 30s)
- **Estado esperado final:**
  - 1 cliente registrado
  - 2 cuentas creadas
  - ~6-7 movimientos registrados
  - 1-2 transferencias realizadas
  - 1 banco externo registrado

- **Si tienes más tiempo:**
  - Mostrar un `GET /api/Clientes/{id}/cuentas` para ver todas las cuentas del cliente
  - Hacer un `PUT /api/Cuentas/{id}` para cambiar el estado de una cuenta a "Inactiva"
  - Intentar hacer una transferencia desde una cuenta inactiva (debe fallar)
  - Mostrar el endpoint `GET /api/Transferencias` para ver el historial completo

---

**¡Éxito en la presentación!** 🚀
