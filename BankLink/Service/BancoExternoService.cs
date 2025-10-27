using BankLink.Models;
using BankLink.Context;
using BankLink.interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BankLink.Service
{
    public class BancoExternoService : IBancoExternoService
    {
        private readonly BankLinkDbContext _context;
        private readonly HttpClient _httpClient;

        public BancoExternoService(BankLinkDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<BancoExterno>> GetAllAsync()
        {
            return await _context.BancosExternos
                .Include(b => b.TransferenciasRecibidas)
                .ToListAsync();
        }

        public async Task<BancoExterno?> GetByIdAsync(int id)
        {
            return await _context.BancosExternos
                .Include(b => b.TransferenciasRecibidas)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<BancoExterno?> GetByCodigoAsync(string codigo)
        {
            return await _context.BancosExternos
                .Include(b => b.TransferenciasRecibidas)
                .FirstOrDefaultAsync(b => b.CodigoIdentificacion == codigo);
        }

        public async Task<IEnumerable<BancoExterno>> GetActivosAsync()
        {
            return await _context.BancosExternos
                .Where(b => b.Activo)
                .ToListAsync();
        }

        public async Task<BancoExterno> CreateAsync(BancoExterno banco)
        {
            // Validar que no exista el código
            var existeCodigo = await _context.BancosExternos
                .AnyAsync(b => b.CodigoIdentificacion == banco.CodigoIdentificacion);
            
            if (existeCodigo)
            {
                throw new InvalidOperationException("Ya existe un banco con este código de identificación");
            }

            banco.FechaCreacion = DateTime.UtcNow;
            banco.Activo = true;
            
            _context.BancosExternos.Add(banco);
            await _context.SaveChangesAsync();
            
            return banco;
        }

        public async Task<BancoExterno> UpdateAsync(BancoExterno banco)
        {
            var existeBanco = await _context.BancosExternos.FindAsync(banco.Id);
            if (existeBanco == null)
            {
                throw new InvalidOperationException("Banco externo no encontrado");
            }

            // Validar que no exista el código en otro banco
            var existeCodigo = await _context.BancosExternos
                .AnyAsync(b => b.CodigoIdentificacion == banco.CodigoIdentificacion && b.Id != banco.Id);
            
            if (existeCodigo)
            {
                throw new InvalidOperationException("Ya existe otro banco con este código de identificación");
            }

            existeBanco.Nombre = banco.Nombre;
            existeBanco.CodigoIdentificacion = banco.CodigoIdentificacion;
            existeBanco.UrlBase = banco.UrlBase;
            existeBanco.Activo = banco.Activo;

            await _context.SaveChangesAsync();
            return existeBanco;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var banco = await _context.BancosExternos
                .Include(b => b.TransferenciasRecibidas)
                .FirstOrDefaultAsync(b => b.Id == id);
            
            if (banco == null)
            {
                return false;
            }

            // Verificar que no tenga transferencias activas
            if (banco.TransferenciasRecibidas.Any(t => t.Estado == EstadoTransferencia.Pendiente))
            {
                throw new InvalidOperationException("No se puede eliminar un banco con transferencias pendientes");
            }

            _context.BancosExternos.Remove(banco);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivarAsync(int id)
        {
            var banco = await _context.BancosExternos.FindAsync(id);
            if (banco == null)
            {
                return false;
            }

            banco.Activo = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DesactivarAsync(int id)
        {
            var banco = await _context.BancosExternos.FindAsync(id);
            if (banco == null)
            {
                return false;
            }

            banco.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidarConexionAsync(int bancoId)
        {
            var banco = await _context.BancosExternos.FindAsync(bancoId);
            if (banco == null || !banco.Activo)
            {
                return false;
            }

            try
            {
                var response = await _httpClient.GetAsync($"{banco.UrlBase}/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EnviarTransferenciaAsync(int bancoId, string cuentaDestino, decimal monto, string descripcion)
        {
            var banco = await _context.BancosExternos.FindAsync(bancoId);
            if (banco == null || !banco.Activo)
            {
                return false;
            }

            try
            {
                var transferenciaData = new
                {
                    CuentaDestino = cuentaDestino,
                    Monto = monto,
                    Descripcion = descripcion,
                    FechaHora = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(transferenciaData);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{banco.UrlBase}/transferencias", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ValidarCuentaExternaAsync(int bancoId, string numeroCuenta)
        {
            var banco = await _context.BancosExternos.FindAsync(bancoId);
            if (banco == null || !banco.Activo)
            {
                return false;
            }

            try
            {
                var response = await _httpClient.GetAsync($"{banco.UrlBase}/cuentas/validar/{numeroCuenta}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.BancosExternos.AnyAsync(b => b.Id == id);
        }

        public async Task<bool> ExistsByCodigoAsync(string codigo)
        {
            return await _context.BancosExternos.AnyAsync(b => b.CodigoIdentificacion == codigo);
        }
    }
}