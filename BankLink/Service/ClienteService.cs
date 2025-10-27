using BankLink.Models;
using BankLink.Context;
using BankLink.interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankLink.Service
{
    public class ClienteService : IClienteService
    {
        private readonly BankLinkDbContext _context;

        public ClienteService(BankLinkDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Cliente>> GetAllAsync()
        {
            return await _context.Clientes
                .Include(c => c.Cuentas)
                .ToListAsync();
        }

        public async Task<Cliente?> GetByIdAsync(int id)
        {
            return await _context.Clientes
                .Include(c => c.Cuentas)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Cliente?> GetByIdentificacionAsync(string identificacion)
        {
            return await _context.Clientes
                .Include(c => c.Cuentas)
                .FirstOrDefaultAsync(c => c.Identificacion == identificacion);
        }

        public async Task<Cliente?> GetByEmailAsync(string email)
        {
            return await _context.Clientes
                .Include(c => c.Cuentas)
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<Cliente> CreateAsync(Cliente cliente)
        {
            // Validar que no exista la identificación
            var existeIdentificacion = await _context.Clientes
                .AnyAsync(c => c.Identificacion == cliente.Identificacion);
            
            if (existeIdentificacion)
            {
                throw new InvalidOperationException("Ya existe un cliente con esta identificación");
            }

            // Validar que no exista el email
            var existeEmail = await _context.Clientes
                .AnyAsync(c => c.Email == cliente.Email);
            
            if (existeEmail)
            {
                throw new InvalidOperationException("Ya existe un cliente con este email");
            }

            cliente.FechaCreacion = DateTime.UtcNow;
            
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            
            return cliente;
        }

        public async Task<Cliente> UpdateAsync(Cliente cliente)
        {
            var existeCliente = await _context.Clientes.FindAsync(cliente.Id);
            if (existeCliente == null)
            {
                throw new InvalidOperationException("Cliente no encontrado");
            }

            // Validar que no exista la identificación en otro cliente
            var existeIdentificacion = await _context.Clientes
                .AnyAsync(c => c.Identificacion == cliente.Identificacion && c.Id != cliente.Id);
            
            if (existeIdentificacion)
            {
                throw new InvalidOperationException("Ya existe otro cliente con esta identificación");
            }

            // Validar que no exista el email en otro cliente
            var existeEmail = await _context.Clientes
                .AnyAsync(c => c.Email == cliente.Email && c.Id != cliente.Id);
            
            if (existeEmail)
            {
                throw new InvalidOperationException("Ya existe otro cliente con este email");
            }

            existeCliente.Nombre = cliente.Nombre;
            existeCliente.Apellido = cliente.Apellido;
            existeCliente.Identificacion = cliente.Identificacion;
            existeCliente.Direccion = cliente.Direccion;
            existeCliente.Telefono = cliente.Telefono;
            existeCliente.Email = cliente.Email;

            await _context.SaveChangesAsync();
            return existeCliente;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Cuentas)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (cliente == null)
            {
                return false;
            }

            // Verificar que no tenga cuentas activas
            if (cliente.Cuentas.Any(c => c.Estado == EstadoCuenta.Activa))
            {
                throw new InvalidOperationException("No se puede eliminar un cliente con cuentas activas");
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Clientes.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> ExistsByIdentificacionAsync(string identificacion)
        {
            return await _context.Clientes.AnyAsync(c => c.Identificacion == identificacion);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Clientes.AnyAsync(c => c.Email == email);
        }

        public async Task<IEnumerable<Cliente>> SearchAsync(string searchTerm)
        {
            return await _context.Clientes
                .Include(c => c.Cuentas)
                .Where(c => c.Nombre.Contains(searchTerm) || 
                           c.Apellido.Contains(searchTerm) ||
                           c.Identificacion.Contains(searchTerm) ||
                           c.Email.Contains(searchTerm))
                .ToListAsync();
        }
    }
}