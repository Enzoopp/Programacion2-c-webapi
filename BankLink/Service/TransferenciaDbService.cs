using BankLink.Context;
using BankLink.Dtos;
using BankLink.interfaces;
using BankLink.Models;
using Microsoft.EntityFrameworkCore;

namespace BankLink.Service
{
    public class TransferenciaDbService : ITransferenciaService
    {
        private readonly BankLinkDbContext _context;
        private readonly ICuentaService _cuentaService;
        private readonly IMovimientoService _movimientoService;
        private readonly IBancoExternoService _bancoExternoService;
        private readonly IHttpClientFactory _httpClientFactory;

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

        public Transferencia Create(Transferencia transferencia)
        {
            _context.Transferencias.Add(transferencia);
            _context.SaveChanges();
            return transferencia;
        }

        public List<Transferencia> GetAll()
        {
            return _context.Transferencias
                .Include(t => t.CuentaOrigen)
                .Include(t => t.BancoDestino)
                .OrderByDescending(t => t.FechaHora)
                .ToList();
        }

        public Transferencia GetById(int id)
        {
            return _context.Transferencias
                .Include(t => t.CuentaOrigen)
                .Include(t => t.BancoDestino)
                .FirstOrDefault(t => t.Id == id);
        }

        public List<Transferencia> GetByCuentaOrigenId(int cuentaId)
        {
            return _context.Transferencias
                .Include(t => t.CuentaOrigen)
                .Include(t => t.BancoDestino)
                .Where(t => t.IdCuentaOrigen == cuentaId)
                .OrderByDescending(t => t.FechaHora)
                .ToList();
        }

        public void Update(int id, Transferencia transferencia)
        {
            var transferenciaExistente = _context.Transferencias.Find(id);
            if (transferenciaExistente != null)
            {
                transferenciaExistente.Estado = transferencia.Estado;
                transferenciaExistente.Descripcion = transferencia.Descripcion;
                _context.SaveChanges();
            }
        }

        // TRANSFERENCIA INTERNA (entre cuentas de BankLink)
        public async Task<Transferencia> RealizarTransferenciaInternaAsync(TransferenciaDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Validar cuenta origen
                var cuentaOrigen = _cuentaService.GetById(dto.IdCuentaOrigen);
                if (cuentaOrigen == null)
                    throw new Exception("Cuenta origen no encontrada");

                if (cuentaOrigen.Estado != "Activa")
                    throw new Exception("La cuenta origen no está activa");

                // 2. Validar saldo suficiente
                if (cuentaOrigen.SaldoActual < dto.Monto)
                    throw new Exception("Saldo insuficiente en cuenta origen");

                // 3. Validar cuenta destino
                var cuentaDestino = _cuentaService.GetByNumeroCuenta(dto.NumeroCuentaDestino);
                if (cuentaDestino == null)
                    throw new Exception("Cuenta destino no encontrada");

                if (cuentaDestino.Estado != "Activa")
                    throw new Exception("La cuenta destino no está activa");

                // 4. Disminuir saldo de cuenta origen
                cuentaOrigen.SaldoActual -= dto.Monto;
                _cuentaService.ActualizarSaldo(cuentaOrigen.Id, cuentaOrigen.SaldoActual);

                // 5. Aumentar saldo de cuenta destino
                cuentaDestino.SaldoActual += dto.Monto;
                _cuentaService.ActualizarSaldo(cuentaDestino.Id, cuentaDestino.SaldoActual);

                // 6. Registrar movimiento en cuenta origen
                var movimientoOrigen = new Movimiento
                {
                    IdCuenta = cuentaOrigen.Id,
                    TipoMovimiento = "Transferencia Enviada",
                    Monto = dto.Monto,
                    FechaHora = DateTime.Now,
                    Descripcion = $"Transferencia a cuenta {dto.NumeroCuentaDestino}. {dto.Descripcion}"
                };
                _movimientoService.Create(movimientoOrigen);

                // 7. Registrar movimiento en cuenta destino
                var movimientoDestino = new Movimiento
                {
                    IdCuenta = cuentaDestino.Id,
                    TipoMovimiento = "Transferencia Recibida",
                    Monto = dto.Monto,
                    FechaHora = DateTime.Now,
                    Descripcion = $"Transferencia desde cuenta {cuentaOrigen.NumeroCuenta}. {dto.Descripcion}"
                };
                _movimientoService.Create(movimientoDestino);

                // 8. Crear registro de transferencia
                var transferencia = new Transferencia
                {
                    IdCuentaOrigen = cuentaOrigen.Id,
                    NumeroCuentaDestino = dto.NumeroCuentaDestino,
                    Monto = dto.Monto,
                    FechaHora = DateTime.Now,
                    Estado = "Completada",
                    Descripcion = dto.Descripcion,
                    TipoTransferencia = "Enviada"
                };
                var transferenciaCreada = Create(transferencia);

                // 9. Commit de la transacción
                await transaction.CommitAsync();

                return transferenciaCreada;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // TRANSFERENCIA EXTERNA (a otro banco)
        public async Task<Transferencia> RealizarTransferenciaExternaAsync(TransferenciaDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Validar cuenta origen
                var cuentaOrigen = _cuentaService.GetById(dto.IdCuentaOrigen);
                if (cuentaOrigen == null)
                    throw new Exception("Cuenta origen no encontrada");

                if (cuentaOrigen.Estado != "Activa")
                    throw new Exception("La cuenta origen no está activa");

                // 2. Validar saldo suficiente
                if (cuentaOrigen.SaldoActual < dto.Monto)
                    throw new Exception("Saldo insuficiente en cuenta origen");

                // 3. Validar banco externo
                if (!dto.IdBancoDestino.HasValue)
                    throw new Exception("Debe especificar el banco destino");

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
