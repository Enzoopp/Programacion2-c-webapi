using BankLink.Context;
using BankLink.interfaces;
using BankLink.Models;

namespace BankLink.Service
{
    public class ClienteDbService : IClienteService
    {
        private readonly BankLinkDbContext _context;

        public ClienteDbService(BankLinkDbContext context)
        {
            _context = context;
        }

        public Cliente Create(Cliente cliente)
        {
            _context.Clientes.Add(cliente);
            _context.SaveChanges();
            return cliente;
        }

        public void Delete(int id)
        {
            var cliente = _context.Clientes.Find(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
                _context.SaveChanges();
            }
        }

        public List<Cliente> GetAll()
        {
            return _context.Clientes.ToList();
        }

        public Cliente GetById(int id)
        {
            return _context.Clientes.Find(id);
        }

        public Cliente GetByNombreUsuario(string nombreUsuario)
        {
            return _context.Clientes.FirstOrDefault(c => c.NombreUsuario == nombreUsuario);
        }

        public Cliente GetByDni(string dni)
        {
            return _context.Clientes.FirstOrDefault(c => c.Dni == dni);
        }

        public void Update(int id, Cliente cliente)
        {
            var clienteExistente = _context.Clientes.Find(id);
            if (clienteExistente != null)
            {
                clienteExistente.Nombre = cliente.Nombre;
                clienteExistente.Apellido = cliente.Apellido;
                clienteExistente.Dni = cliente.Dni;
                clienteExistente.Direccion = cliente.Direccion;
                clienteExistente.Telefono = cliente.Telefono;
                clienteExistente.Email = cliente.Email;
                clienteExistente.NombreUsuario = cliente.NombreUsuario;
                clienteExistente.PassHash = cliente.PassHash;
                clienteExistente.Rol = cliente.Rol;
                _context.SaveChanges();
            }
        }
    }
}
