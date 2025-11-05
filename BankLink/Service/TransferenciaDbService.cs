// ============================================================================
// TRANSFERENCIADBSERVICE.CS - Servicio de Transferencias con SQL Server
// ============================================================================
// Este es el servicio MÁS CRÍTICO del proyecto porque implementa
// CONSISTENCIA TRANSACCIONAL usando transacciones de base de datos.
//
// DESAFÍO PRINCIPAL DEL TP:
// Las transferencias deben ser ATÓMICAS: o se completan TODAS las operaciones
// o NINGUNA. Si algo falla, todo se deshace (Rollback).
// ============================================================================

using BankLink.Context;
using BankLink.Dtos;
using BankLink.interfaces;
using BankLink.Models;
using Microsoft.EntityFrameworkCore;

namespace BankLink.Service
{
    public class TransferenciaDbService : ITransferenciaService
    {
        // ========================================================================
        // DEPENDENCIAS INYECTADAS
        // ========================================================================
        // Este servicio necesita múltiples dependencias para funcionar
        
        private readonly BankLinkDbContext _context;  // Acceso directo a la BD
        private readonly ICuentaService _cuentaService;  // Para validar y actualizar cuentas
        private readonly IMovimientoService _movimientoService;  // Para registrar movimientos
        private readonly IBancoExternoService _bancoExternoService;  // Para validar bancos externos
        private readonly IHttpClientFactory _httpClientFactory;  // Para llamar APIs externas

        // Constructor: recibe todas las dependencias por inyección
        public TransferenciaDbService(
            BankLinkDbContext context,
            ICuentaService cuentaService,
            IMovimientoService movimientoService,
            IBancoExternoService bancoExternoService,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _cuentaService = cuentaService;
            _movimientoService = movimientoService;
            _bancoExternoService = bancoExternoService;
            _httpClientFactory = httpClientFactory;
        }

        // ========================================================================
        // MÉTODOS CRUD BÁSICOS
        // ========================================================================
        
        /// <summary>
        /// Crea un nuevo registro de transferencia en la base de datos
        /// </summary>
        public Transferencia Create(Transferencia transferencia)
        {
            _context.Transferencias.Add(transferencia);
            _context.SaveChanges();
            return transferencia;
        }

        /// <summary>
        /// Obtiene todas las transferencias con sus relaciones (Include)
        /// Ordenadas de más reciente a más antigua
        /// </summary>
        public List<Transferencia> GetAll()
        {
            return _context.Transferencias
                .Include(t => t.CuentaOrigen)      // Cargar cuenta origen
                .Include(t => t.BancoDestino)      // Cargar banco destino (si existe)
                .OrderByDescending(t => t.FechaHora)  // Más recientes primero
                .ToList();
        }

        /// <summary>
        /// Obtiene una transferencia específica por ID
        /// </summary>
        public Transferencia GetById(int id)
        {
            return _context.Transferencias
                .Include(t => t.CuentaOrigen)
                .Include(t => t.BancoDestino)
                .FirstOrDefault(t => t.Id == id);
        }

        /// <summary>
        /// Obtiene todas las transferencias de una cuenta específica
        /// Útil para ver el historial de transferencias de un cliente
        /// </summary>
        public List<Transferencia> GetByCuentaOrigenId(int cuentaId)
        {
            return _context.Transferencias
                .Include(t => t.CuentaOrigen)
                .Include(t => t.BancoDestino)
                .Where(t => t.IdCuentaOrigen == cuentaId)  // Filtrar por cuenta origen
                .OrderByDescending(t => t.FechaHora)
                .ToList();
        }

        /// <summary>
        /// Actualiza el estado o descripción de una transferencia
        /// Solo permite modificar estos campos por seguridad
        /// </summary>
        public void Update(int id, Transferencia transferencia)
        {
            var transferenciaExistente = _context.Transferencias.Find(id);
            if (transferenciaExistente != null)
            {
                // Solo actualizar campos permitidos
                transferenciaExistente.Estado = transferencia.Estado;
                transferenciaExistente.Descripcion = transferencia.Descripcion;
                _context.SaveChanges();
            }
        }

        // ========================================================================
        // MÉTODO PRINCIPAL: TRANSFERENCIA INTERNA
        // ========================================================================
        // Este método implementa la lógica transaccional completa
        // Es el CORAZÓN del desafío del TP
        // ========================================================================
        
        /// <summary>
        /// Realiza una transferencia entre dos cuentas de BankLink
        /// 
        /// FLUJO TRANSACCIONAL:
        /// 1. BeginTransaction: Inicia una transacción de BD
        /// 2. Validaciones: Cuenta origen, saldo, cuenta destino
        /// 3. Actualizar saldos: Restar origen, sumar destino
        /// 4. Registrar movimientos: Dos movimientos (enviada/recibida)
        /// 5. Crear transferencia: Registro de auditoría
        /// 6. CommitAsync: Si todo OK, hacer cambios permanentes
        /// 7. RollbackAsync: Si algo falla, deshacer TODO
        /// </summary>
        public async Task<Transferencia> RealizarTransferenciaInternaAsync(TransferenciaDto dto)
        {
            // ====================================================================
            // PASO 0: INICIAR TRANSACCIÓN DE BASE DE DATOS
            // ====================================================================
            // 'using' garantiza que la transacción se libere al terminar
            // BeginTransactionAsync marca el inicio de la unidad de trabajo
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // ================================================================
                // PASO 1: VALIDAR CUENTA ORIGEN
                // ================================================================
                var cuentaOrigen = _cuentaService.GetById(dto.IdCuentaOrigen);
                
                // Si la cuenta no existe, lanzar excepción
                if (cuentaOrigen == null)
                    throw new Exception("Cuenta origen no encontrada");

                // Solo cuentas activas pueden transferir dinero
                if (cuentaOrigen.Estado != "Activa")
                    throw new Exception("La cuenta origen no está activa");

                // ================================================================
                // PASO 2: VALIDAR SALDO SUFICIENTE
                // ================================================================
                // Regla de negocio: No permitir saldos negativos
                if (cuentaOrigen.SaldoActual < dto.Monto)
                    throw new Exception("Saldo insuficiente en cuenta origen");

                // ================================================================
                // PASO 3: VALIDAR CUENTA DESTINO
                // ================================================================
                var cuentaDestino = _cuentaService.GetByNumeroCuenta(dto.NumeroCuentaDestino);
                
                if (cuentaDestino == null)
                    throw new Exception("Cuenta destino no encontrada");

                if (cuentaDestino.Estado != "Activa")
                    throw new Exception("La cuenta destino no está activa");

                // ================================================================
                // PASO 4: ACTUALIZAR SALDO DE CUENTA ORIGEN (RESTAR)
                // ================================================================
                // Operación crítica: restar dinero de la cuenta origen
                cuentaOrigen.SaldoActual -= dto.Monto;
                _cuentaService.ActualizarSaldo(cuentaOrigen.Id, cuentaOrigen.SaldoActual);

                // ================================================================
                // PASO 5: ACTUALIZAR SALDO DE CUENTA DESTINO (SUMAR)
                // ================================================================
                // Operación crítica: sumar dinero a la cuenta destino
                cuentaDestino.SaldoActual += dto.Monto;
                _cuentaService.ActualizarSaldo(cuentaDestino.Id, cuentaDestino.SaldoActual);

                // ================================================================
                // PASO 6: REGISTRAR MOVIMIENTO EN CUENTA ORIGEN
                // ================================================================
                // Crear registro de auditoría: "Transferencia Enviada"
                var movimientoOrigen = new Movimiento
                {
                    IdCuenta = cuentaOrigen.Id,
                    TipoMovimiento = "Transferencia Enviada",  // Tipo de operación
                    Monto = dto.Monto,
                    FechaHora = DateTime.Now,  // Timestamp exacto
                    Descripcion = $"Transferencia a cuenta {dto.NumeroCuentaDestino}. {dto.Descripcion}"
                };
                _movimientoService.Create(movimientoOrigen);

                // ================================================================
                // PASO 7: REGISTRAR MOVIMIENTO EN CUENTA DESTINO
                // ================================================================
                // Crear registro de auditoría: "Transferencia Recibida"
                var movimientoDestino = new Movimiento
                {
                    IdCuenta = cuentaDestino.Id,
                    TipoMovimiento = "Transferencia Recibida",  // Contraparte
                    Monto = dto.Monto,
                    FechaHora = DateTime.Now,
                    Descripcion = $"Transferencia desde cuenta {cuentaOrigen.NumeroCuenta}. {dto.Descripcion}"
                };
                _movimientoService.Create(movimientoDestino);

                // ================================================================
                // PASO 8: CREAR REGISTRO DE TRANSFERENCIA
                // ================================================================
                // Registro maestro de la transferencia para tracking
                var transferencia = new Transferencia
                {
                    IdCuentaOrigen = cuentaOrigen.Id,
                    NumeroCuentaDestino = dto.NumeroCuentaDestino,
                    Monto = dto.Monto,
                    FechaHora = DateTime.Now,
                    Estado = "Completada",  // Estado exitoso
                    Descripcion = dto.Descripcion,
                    TipoTransferencia = "Enviada"  // Desde nuestra perspectiva
                };
                var transferenciaCreada = Create(transferencia);

                // ================================================================
                // PASO 9: COMMIT - HACER CAMBIOS PERMANENTES
                // ================================================================
                // Si llegamos aquí, TODO salió bien
                // CommitAsync hace permanentes TODOS los cambios en la BD
                await transaction.CommitAsync();

                // Retornar la transferencia creada
                return transferenciaCreada;
            }
            catch (Exception)
            {
                // ================================================================
                // PASO 10: ROLLBACK - DESHACER TODO SI HAY ERROR
                // ================================================================
                // Si CUALQUIER operación falla, deshacemos TODOS los cambios
                // La base de datos queda como si nunca hubiéramos empezado
                // Esto garantiza CONSISTENCIA: no hay estados intermedios
                await transaction.RollbackAsync();
                
                // Re-lanzar la excepción para que el controller la maneje
                throw;
            }
        }

        // ========================================================================
        // TRANSFERENCIA EXTERNA (Hacia otro banco)
        // ========================================================================
        
        /// <summary>
        /// Realiza una transferencia hacia un banco externo
        /// Similar a la interna pero solo resta del origen y llama API externa
        /// </summary>
        public async Task<Transferencia> RealizarTransferenciaExternaAsync(TransferenciaDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Validar cuenta origen (igual que interna)
                var cuentaOrigen = _cuentaService.GetById(dto.IdCuentaOrigen);
                if (cuentaOrigen == null)
                    throw new Exception("Cuenta origen no encontrada");

                if (cuentaOrigen.Estado != "Activa")
                    throw new Exception("La cuenta origen no está activa");

                // 2. Validar saldo suficiente
                if (cuentaOrigen.SaldoActual < dto.Monto)
                    throw new Exception("Saldo insuficiente en cuenta origen");

                // 3. Validar que se especificó el banco destino
                if (!dto.IdBancoDestino.HasValue)
                    throw new Exception("Debe especificar el banco destino");

                // 4. Obtener información del banco externo
                var bancoExterno = _bancoExternoService.GetById(dto.IdBancoDestino.Value);
                if (bancoExterno == null)
                    throw new Exception("Banco externo no encontrado");

                if (!bancoExterno.Activo)
                    throw new Exception("El banco externo no está activo");

                // 4. Disminuir saldo de cuenta origen
                cuentaOrigen.SaldoActual -= dto.Monto;
                _cuentaService.ActualizarSaldo(cuentaOrigen.Id, cuentaOrigen.SaldoActual);

                // 5. Registrar movimiento en cuenta origen
                var movimiento = new Movimiento
                {
                    IdCuenta = cuentaOrigen.Id,
                    TipoMovimiento = "Transferencia Enviada",
                    Monto = dto.Monto,
                    FechaHora = DateTime.Now,
                    Descripcion = $"Transferencia externa a {bancoExterno.NombreBanco} - Cuenta: {dto.NumeroCuentaDestino}. {dto.Descripcion}"
                };
                _movimientoService.Create(movimiento);

                // 6. Crear registro de transferencia
                var transferencia = new Transferencia
                {
                    IdCuentaOrigen = cuentaOrigen.Id,
                    IdBancoDestino = dto.IdBancoDestino,
                    NumeroCuentaDestino = dto.NumeroCuentaDestino,
                    Monto = dto.Monto,
                    FechaHora = DateTime.Now,
                    Estado = "Completada",
                    Descripcion = dto.Descripcion,
                    TipoTransferencia = "Enviada"
                };
                var transferenciaCreada = Create(transferencia);

                // 7. Llamar a la API del banco externo
                try
                {
                    await EnviarTransferenciaABancoExterno(bancoExterno, dto);
                }
                catch (Exception ex)
                {
                    // Si falla la llamada externa, registramos pero no hacemos rollback
                    Console.WriteLine($"Error al notificar al banco externo: {ex.Message}");
                    // En producción, aquí podrías marcar la transferencia como "Pendiente de confirmación"
                }

                // 8. Commit de la transacción
                await transaction.CommitAsync();

                return transferenciaCreada;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // RECIBIR TRANSFERENCIA EXTERNA (desde otro banco)
        public async Task<Transferencia> RecibirTransferenciaExternaAsync(TransferenciaRecibidaDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Validar cuenta destino
                var cuentaDestino = _cuentaService.GetByNumeroCuenta(dto.NumeroCuentaDestino);
                if (cuentaDestino == null)
                    throw new Exception("Cuenta destino no encontrada");

                if (cuentaDestino.Estado != "Activa")
                    throw new Exception("La cuenta destino no está activa");

                // 2. Aumentar saldo de cuenta destino
                cuentaDestino.SaldoActual += dto.Monto;
                _cuentaService.ActualizarSaldo(cuentaDestino.Id, cuentaDestino.SaldoActual);

                // 3. Registrar movimiento
                var movimiento = new Movimiento
                {
                    IdCuenta = cuentaDestino.Id,
                    TipoMovimiento = "Transferencia Recibida",
                    Monto = dto.Monto,
                    FechaHora = DateTime.Now,
                    Descripcion = $"Transferencia desde {dto.BancoOrigen} - Cuenta: {dto.NumeroCuentaOrigen}. {dto.Descripcion}"
                };
                _movimientoService.Create(movimiento);

                // 4. Crear registro de transferencia
                var transferencia = new Transferencia
                {
                    IdCuentaOrigen = cuentaDestino.Id, // Usamos la misma cuenta como referencia
                    NumeroCuentaDestino = dto.NumeroCuentaDestino,
                    Monto = dto.Monto,
                    FechaHora = DateTime.Now,
                    Estado = "Completada",
                    Descripcion = $"Desde {dto.BancoOrigen}. {dto.Descripcion}",
                    TipoTransferencia = "Recibida"
                };
                var transferenciaCreada = Create(transferencia);

                // 5. Commit de la transacción
                await transaction.CommitAsync();

                return transferenciaCreada;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Método privado para llamar a la API del banco externo
        private async Task EnviarTransferenciaABancoExterno(BancoExterno banco, TransferenciaDto dto)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(banco.UrlApiBase);
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            var payload = new
            {
                numeroCuentaDestino = dto.NumeroCuentaDestino,
                monto = dto.Monto,
                descripcion = dto.Descripcion,
                bancoOrigen = "BankLink",
                fecha = DateTime.Now
            };

            // Simular llamada a API externa (endpoint de ejemplo)
            var response = await httpClient.PostAsJsonAsync("/api/transferencias/recibir", payload);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error al enviar transferencia al banco externo: {response.StatusCode}");
            }
        }
    }
}
