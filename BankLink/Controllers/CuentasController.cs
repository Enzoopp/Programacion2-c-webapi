// ============================================================================
// CUENTASCONTROLLER.CS - Controlador REST de Cuentas Bancarias
// ============================================================================
// Este controlador expone endpoints HTTP para gestionar cuentas bancarias
//
// ENDPOINTS:
// - GET /api/Cuentas → Listar todas
// - GET /api/Cuentas/{id} → Obtener por ID
// - GET /api/Cuentas/numero/{numero} → Buscar por número de cuenta
// - GET /api/Cuentas/cliente/{id} → Obtener cuentas de un cliente
// - POST /api/Cuentas → Crear cuenta
// - POST /api/Cuentas/deposito → Realizar depósito
// - POST /api/Cuentas/retiro → Realizar retiro
// - PUT /api/Cuentas/{id} → Actualizar cuenta
// - DELETE /api/Cuentas/{id} → Eliminar cuenta
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BankLink.Models;
using BankLink.Dtos;
using BankLink.interfaces;

namespace BankLink.Controllers
{
    /// <summary>
    /// Controlador REST para operaciones de cuentas bancarias
    /// </summary>
    [ApiController]  // Habilita validación automática de ModelState
    [Route("api/[controller]")]  // Ruta base: /api/Cuentas
    [Authorize]  // 🔒 REQUIERE AUTENTICACIÓN JWT para todos los endpoints
    public class CuentasController : ControllerBase
    {
        // ====================================================================
        // DEPENDENCIAS INYECTADAS
        // ====================================================================
        // Este controller necesita 3 servicios:
        private readonly ICuentaService _cuentaService;  // Para CRUD de cuentas
        private readonly IClienteService _clienteService;  // Para validar clientes
        private readonly IMovimientoService _movimientoService;  // Para registrar movimientos

        /// <summary>
        /// Constructor: recibe servicios por inyección de dependencias
        /// </summary>
        public CuentasController(
            ICuentaService cuentaService,
            IClienteService clienteService,
            IMovimientoService movimientoService)
        {
            _cuentaService = cuentaService;
            _clienteService = clienteService;
            _movimientoService = movimientoService;
        }

        // ====================================================================
        // GET /api/Cuentas → LISTAR TODAS LAS CUENTAS
        // ====================================================================
        /// <summary>
        /// Obtiene todas las cuentas del sistema
        /// </summary>
        /// <returns>HTTP 200 con lista de cuentas</returns>
        [HttpGet]
        public ActionResult<List<Cuenta>> GetAll()
        {
            // Ok() retorna HTTP 200 con el body
            return Ok(_cuentaService.GetAll());
        }

        // ====================================================================
        // GET /api/Cuentas/{id} → OBTENER CUENTA POR ID
        // ====================================================================
        /// <summary>
        /// Obtiene una cuenta específica por su ID
        /// </summary>
        /// <param name="id">ID de la cuenta</param>
        /// <returns>HTTP 200 con la cuenta | HTTP 404 si no existe</returns>
        [HttpGet("{id}")]
        public ActionResult<Cuenta> GetById(int id)
        {
            var cuenta = _cuentaService.GetById(id);
            
            if (cuenta != null)
            {
                // HTTP 200 OK con la cuenta en el body
                return Ok(cuenta);
            }
            else
            {
                // HTTP 404 Not Found con mensaje descriptivo
                return NotFound($"No se encontró la cuenta con id: {id}");
            }
        }

        // ====================================================================
        // GET /api/Cuentas/numero/{numeroCuenta} → BUSCAR POR NÚMERO
        // ====================================================================
        /// <summary>
        /// Busca una cuenta por su número único de 8 dígitos
        /// Útil para transferencias (se ingresa número, no ID)
        /// </summary>
        [HttpGet("numero/{numeroCuenta}")]
        public ActionResult<Cuenta> GetByNumeroCuenta(string numeroCuenta)
        {
            var cuenta = _cuentaService.GetByNumeroCuenta(numeroCuenta);
            
            if (cuenta != null)
            {
                return Ok(cuenta);
            }
            else
            {
                return NotFound($"No se encontró la cuenta con número: {numeroCuenta}");
            }
        }

        // ====================================================================
        // GET /api/Cuentas/cliente/{clienteId} → CUENTAS DE UN CLIENTE
        // ====================================================================
        /// <summary>
        /// Obtiene todas las cuentas de un cliente específico
        /// Útil para mostrar "Mis Cuentas" en la UI
        /// </summary>
        [HttpGet("cliente/{clienteId}")]
        public ActionResult<List<Cuenta>> GetByClienteId(int clienteId)
        {
            var cuentas = _cuentaService.GetByClienteId(clienteId);
            // Siempre retorna lista (vacía si el cliente no tiene cuentas)
            return Ok(cuentas);
        }

        // ====================================================================
        // POST /api/Cuentas → CREAR NUEVA CUENTA
        // ====================================================================
        /// <summary>
        /// Crea una nueva cuenta bancaria para un cliente
        /// 
        /// FLUJO:
        /// 1. Validar que el cliente existe
        /// 2. Generar número de cuenta único
        /// 3. Crear cuenta con estado "Activa"
        /// 4. Si hay saldo inicial, registrar movimiento de depósito
        /// </summary>
        [HttpPost]
        public ActionResult<Cuenta> Create([FromBody] CrearCuentaDto dto)
        {
            // [ApiController] valida automáticamente el ModelState
            // Si hay errores de validación, retorna HTTP 400 automáticamente
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // ================================================================
            // VALIDACIÓN: Verificar que el cliente existe
            // ================================================================
            var cliente = _clienteService.GetById(dto.IdClientePropietario);
            if (cliente == null)
            {
                // HTTP 400 Bad Request si el cliente no existe
                return BadRequest($"No se encontró el cliente con id: {dto.IdClientePropietario}");
            }

            // ================================================================
            // GENERAR NÚMERO DE CUENTA ÚNICO
            // ================================================================
            // GenerarNumeroCuenta() crea un número aleatorio de 8 dígitos
            var numeroCuenta = GenerarNumeroCuenta();

            // ================================================================
            // CREAR OBJETO CUENTA
            // ================================================================
            var cuenta = new Cuenta
            {
                NumeroCuenta = numeroCuenta,
                TipoCuenta = dto.TipoCuenta,  // "Ahorro" o "Corriente"
                SaldoActual = dto.SaldoInicial,  // Puede ser 0 o más
                Estado = "Activa",  // Las cuentas nuevas siempre están activas
                IdClientePropietario = dto.IdClientePropietario,
                FechaApertura = DateTime.Now  // Timestamp de creación
            };

            // ================================================================
            // PERSISTIR EN BASE DE DATOS
            // ================================================================
            var newCuenta = _cuentaService.Create(cuenta);

            // ================================================================
            // REGISTRAR MOVIMIENTO INICIAL (si hay saldo inicial)
            // ================================================================
            // AUDITORÍA: Cada cambio de saldo debe tener un movimiento asociado
            // Si la cuenta se crea con $10,000, debe haber un movimiento que lo justifique
            if (dto.SaldoInicial > 0)
            {
                var movimiento = new Movimiento
                {
                    IdCuenta = newCuenta.Id,
                    TipoMovimiento = "Depósito",
                    Monto = dto.SaldoInicial,
                    FechaHora = DateTime.Now,
                    Descripcion = "Depósito inicial al crear la cuenta"
                };
                _movimientoService.Create(movimiento);
            }

            // ================================================================
            // RETORNAR HTTP 201 CREATED
            // ================================================================
            // CreatedAtAction retorna:
            // - HTTP 201 (recurso creado exitosamente)
            // - Header "Location: /api/Cuentas/{id}" (URL del nuevo recurso)
            // - Body con el objeto creado
            return CreatedAtAction(nameof(GetById), new { id = newCuenta.Id }, newCuenta);
        }

        // ====================================================================
        // PUT /api/Cuentas/{id} → ACTUALIZAR CUENTA
        // ====================================================================
        /// <summary>
        /// Actualiza una cuenta existente
        /// Nota: El saldo NO se actualiza aquí (usar deposito/retiro)
        /// </summary>
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Cuenta cuenta)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar que la cuenta existe antes de actualizar
            var cuentaExistente = _cuentaService.GetById(id);
            if (cuentaExistente == null)
            {
                return NotFound($"No se encontró la cuenta con id: {id}");
            }

            _cuentaService.Update(id, cuenta);
            
            // NoContent() retorna HTTP 204 (éxito sin contenido en body)
            // Es el estándar para operaciones PUT exitosas
            return NoContent();
        }

        // ====================================================================
        // DELETE /api/Cuentas/{id} → ELIMINAR CUENTA
        // ====================================================================
        /// <summary>
        /// Elimina una cuenta del sistema
        /// IMPORTANTE: Solo se puede eliminar si no tiene movimientos asociados
        /// (esto lo controla DeleteBehavior.Restrict en BankLinkDbContext)
        /// </summary>
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var cuenta = _cuentaService.GetById(id);
            if (cuenta == null)
            {
                return NotFound($"No se encontró la cuenta con id: {id}");
            }

            _cuentaService.Delete(id);
            return NoContent();  // HTTP 204 (eliminación exitosa)
        }

        // ====================================================================
        // POST /api/Cuentas/deposito → REALIZAR DEPÓSITO
        // ====================================================================
        /// <summary>
        /// Realiza un depósito en una cuenta
        /// 
        /// FLUJO:
        /// 1. Validar que la cuenta existe
        /// 2. Validar que la cuenta está activa
        /// 3. Actualizar saldo (sumar monto)
        /// 4. Registrar movimiento para auditoría
        /// 5. Retornar nuevo saldo
        /// </summary>
        [HttpPost("deposito")]
        public ActionResult RealizarDeposito([FromBody] DepositoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // ============================================================
                // PASO 1: Validar que la cuenta existe
                // ============================================================
                var cuenta = _cuentaService.GetById(dto.IdCuenta);
                if (cuenta == null)
                {
                    return NotFound($"No se encontró la cuenta con id: {dto.IdCuenta}");
                }

                // ============================================================
                // PASO 2: Validar que la cuenta está activa
                // ============================================================
                // Las cuentas "Inactiva" o "Bloqueada" no pueden recibir depósitos
                if (cuenta.Estado != "Activa")
                {
                    return BadRequest("La cuenta no está activa");
                }

                // ============================================================
                // PASO 3: Actualizar saldo (SUMAR monto)
                // ============================================================
                cuenta.SaldoActual += dto.Monto;
                _cuentaService.ActualizarSaldo(cuenta.Id, cuenta.SaldoActual);

                // ============================================================
                // PASO 4: Registrar movimiento (AUDITORÍA)
                // ============================================================
                // IMPORTANTE: Este movimiento justifica el cambio de saldo
                // Permite rastrear: ¿Cuándo? ¿Cuánto? ¿Por qué?
                var movimiento = new Movimiento
                {
                    IdCuenta = cuenta.Id,
                    TipoMovimiento = "Depósito",
                    Monto = dto.Monto,
                    FechaHora = DateTime.Now,
                    Descripcion = dto.Descripcion ?? "Depósito realizado"  // ?? es el operador de coalescencia nula
                };
                _movimientoService.Create(movimiento);

                // ============================================================
                // PASO 5: Retornar respuesta exitosa
                // ============================================================
                // Objeto anónimo con datos útiles para el cliente
                return Ok(new
                {
                    message = "Depósito realizado exitosamente",
                    nuevoSaldo = cuenta.SaldoActual,  // Muestra saldo actualizado
                    movimiento = movimiento  // Devuelve comprobante
                });
            }
            catch (Exception ex)
            {
                // Manejo de errores genéricos (ejemplo: problemas de conexión BD)
                return BadRequest(new { message = $"Error al realizar el depósito: {ex.Message}" });
            }
        }

        // ====================================================================
        // POST /api/Cuentas/retiro → REALIZAR RETIRO
        // ====================================================================
        /// <summary>
        /// Realiza un retiro de una cuenta
        /// 
        /// FLUJO:
        /// 1. Validar que la cuenta existe
        /// 2. Validar que la cuenta está activa
        /// 3. Validar saldo suficiente (no permite sobregiro)
        /// 4. Actualizar saldo (restar monto)
        /// 5. Registrar movimiento para auditoría
        /// 6. Retornar nuevo saldo
        /// </summary>
        [HttpPost("retiro")]
        public ActionResult RealizarRetiro([FromBody] RetiroDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // ============================================================
                // PASO 1: Validar que la cuenta existe
                // ============================================================
                var cuenta = _cuentaService.GetById(dto.IdCuenta);
                if (cuenta == null)
                {
                    return NotFound($"No se encontró la cuenta con id: {dto.IdCuenta}");
                }

                // ============================================================
                // PASO 2: Validar que la cuenta está activa
                // ============================================================
                if (cuenta.Estado != "Activa")
                {
                    return BadRequest("La cuenta no está activa");
                }

                // ============================================================
                // PASO 3: Validar saldo suficiente (REGLA DE NEGOCIO)
                // ============================================================
                // IMPORTANTE: Esta validación evita sobregiros
                // Si necesitas permitir sobregiro, aquí se implementaría la lógica
                if (cuenta.SaldoActual < dto.Monto)
                {
                    return BadRequest("Saldo insuficiente para realizar el retiro");
                }

                // ============================================================
                // PASO 4: Actualizar saldo (RESTAR monto)
                // ============================================================
                cuenta.SaldoActual -= dto.Monto;
                _cuentaService.ActualizarSaldo(cuenta.Id, cuenta.SaldoActual);

                // ============================================================
                // PASO 5: Registrar movimiento (AUDITORÍA)
                // ============================================================
                var movimiento = new Movimiento
                {
                    IdCuenta = cuenta.Id,
                    TipoMovimiento = "Retiro",
                    Monto = dto.Monto,
                    FechaHora = DateTime.Now,
                    Descripcion = dto.Descripcion ?? "Retiro realizado"
                };
                _movimientoService.Create(movimiento);

                // ============================================================
                // PASO 6: Retornar respuesta exitosa
                // ============================================================
                return Ok(new
                {
                    message = "Retiro realizado exitosamente",
                    nuevoSaldo = cuenta.SaldoActual,
                    movimiento = movimiento
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al realizar el retiro: {ex.Message}" });
            }
        }

        // ====================================================================
        // MÉTODO AUXILIAR: GENERAR NÚMERO DE CUENTA ÚNICO
        // ====================================================================
        /// <summary>
        /// Genera un número de cuenta único de 8 dígitos
        /// 
        /// ALGORITMO:
        /// 1. Genera número aleatorio entre 10000000 y 99999999
        /// 2. Verifica que no exista en la base de datos
        /// 3. Si existe, repite el proceso hasta encontrar uno único
        /// 
        /// NOTA: En producción real, se usaría un contador incremental
        /// o un formato específico (ej: "COD-SUCURSAL-NUMERO")
        /// </summary>
        private string GenerarNumeroCuenta()
        {
            var random = new Random();
            string numeroCuenta;
            
            // Bucle do-while: ejecuta al menos una vez, luego repite si es necesario
            do
            {
                // Random.Next(min, max) genera número aleatorio
                // 10000000 = mínimo de 8 dígitos
                // 99999999 = máximo de 8 dígitos
                numeroCuenta = random.Next(10000000, 99999999).ToString();
            } 
            // Repite si el número ya existe (garantiza unicidad)
            while (_cuentaService.GetByNumeroCuenta(numeroCuenta) != null);

            return numeroCuenta;
        }
    }
}

// ============================================================================
// NOTAS PARA LA PRESENTACIÓN ORAL
// ============================================================================
//
// QUÉ DECIR SI PREGUNTAN SOBRE ESTE CONTROLADOR:
//
// 1. PATRÓN REPOSITORY:
//    "Este controlador NO accede directamente a la base de datos.
//     Usa servicios inyectados (ICuentaService, IMovimientoService)
//     que abstraen la lógica de persistencia. Esto hace el código más testeable."
//
// 2. OPERACIONES BANCARIAS:
//    "Los endpoints deposito y retiro son especiales porque:
//     - Actualizan el saldo
//     - Registran un movimiento para auditoría
//     Esto garantiza que SIEMPRE haya un registro de por qué cambió el saldo."
//
// 3. VALIDACIONES:
//    "Antes de cada operación validamos:
//     - Que la cuenta existe (NotFound 404)
//     - Que está activa (BadRequest 400)
//     - En retiros: que hay saldo suficiente (regla de negocio)
//     Esto evita inconsistencias en la base de datos."
//
// 4. CÓDIGOS HTTP:
//    "Usamos códigos semánticos:
//     - 200 OK: operación exitosa con datos
//     - 201 Created: recurso creado (con header Location)
//     - 204 No Content: operación exitosa sin datos
//     - 400 Bad Request: error del cliente (validación)
//     - 404 Not Found: recurso no existe"
//
// 5. GENERACIÓN DE NÚMERO DE CUENTA:
//    "El método GenerarNumeroCuenta() crea números de 8 dígitos aleatorios.
//     Usa un bucle do-while para garantizar unicidad verificando contra la BD.
//     En producción real, usaríamos un formato estructurado con sucursal, tipo, etc."
//
// 6. MOVIMIENTOS COMO AUDITORÍA:
//    "Cada depósito/retiro crea un Movimiento. Esto permite:
//     - Rastrear historial completo de la cuenta
//     - Cumplir requisitos regulatorios bancarios
//     - Detectar fraudes o errores
//     Es el equivalente al 'extracto bancario'."
//
// 7. [FromBody]:
//    "El atributo [FromBody] indica que el parámetro viene del cuerpo HTTP en JSON.
//     ASP.NET automáticamente deserializa el JSON al DTO correspondiente."
//
// 8. DIFERENCIA ENTRE Create Y deposito:
//    "POST /api/Cuentas crea una NUEVA cuenta
//     POST /api/Cuentas/deposito agrega dinero a una cuenta EXISTENTE
//     Son operaciones diferentes aunque ambos usen POST."
//
// ============================================================================
