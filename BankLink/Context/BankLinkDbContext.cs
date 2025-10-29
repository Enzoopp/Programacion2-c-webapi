using BankLink.Models;
using Microsoft.EntityFrameworkCore;

namespace BankLink.Context
{
    public class BankLinkDbContext : DbContext
    {
        public BankLinkDbContext(DbContextOptions<BankLinkDbContext> options) : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Cuenta> Cuentas { get; set; }
        public DbSet<Movimiento> Movimientos { get; set; }
        public DbSet<BancoExterno> BancosExternos { get; set; }
        public DbSet<Transferencia> Transferencias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de relación Cliente -> Cuentas (1:N)
            modelBuilder.Entity<Cuenta>()
                .HasOne(c => c.ClientePropietario)
                .WithMany(cl => cl.Cuentas)
                .HasForeignKey(c => c.IdClientePropietario)
                .OnDelete(DeleteBehavior.Restrict); // Evitar eliminación en cascada

            // Configuración de relación Cuenta -> Movimientos (1:N)
            modelBuilder.Entity<Movimiento>()
                .HasOne(m => m.Cuenta)
                .WithMany(c => c.Movimientos)
                .HasForeignKey(m => m.IdCuenta)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de relación Transferencia -> Cuenta Origen
            modelBuilder.Entity<Transferencia>()
                .HasOne(t => t.CuentaOrigen)
                .WithMany()
                .HasForeignKey(t => t.IdCuentaOrigen)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de relación Transferencia -> Banco Externo (opcional)
            modelBuilder.Entity<Transferencia>()
                .HasOne(t => t.BancoDestino)
                .WithMany()
                .HasForeignKey(t => t.IdBancoDestino)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de precisión para campos decimales
            modelBuilder.Entity<Cuenta>()
                .Property(c => c.SaldoActual)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Movimiento>()
                .Property(m => m.Monto)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transferencia>()
                .Property(t => t.Monto)
                .HasPrecision(18, 2);

            // Índices únicos
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Dni)
                .IsUnique();

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.NombreUsuario)
                .IsUnique();

            modelBuilder.Entity<Cuenta>()
                .HasIndex(c => c.NumeroCuenta)
                .IsUnique();

            modelBuilder.Entity<BancoExterno>()
                .HasIndex(b => b.CodigoIdentificacion)
                .IsUnique();
        }
    }
}
