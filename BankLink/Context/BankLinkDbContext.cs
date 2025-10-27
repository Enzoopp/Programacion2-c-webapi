using BankLink.Models;
using Microsoft.EntityFrameworkCore;

namespace BankLink.Context
{
    public class BankLinkDbContext : DbContext
    {
        public BankLinkDbContext(DbContextOptions<BankLinkDbContext> options) : base(options)
        {
        }

        // DbSets para todas las entidades
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Cuenta> Cuentas { get; set; }
        public DbSet<Movimiento> Movimientos { get; set; }
        public DbSet<BancoExterno> BancosExternos { get; set; }
        public DbSet<Transferencia> Transferencias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Identificacion).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Identificacion).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Direccion).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Telefono).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            });

            // Configuración de Cuenta
            modelBuilder.Entity<Cuenta>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.NumeroCuenta).IsUnique();
                entity.Property(e => e.NumeroCuenta).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Saldo).HasColumnType("decimal(18,2)");
                
                // Relación con Cliente (Una cuenta pertenece a un cliente)
                entity.HasOne(e => e.Cliente)
                      .WithMany(c => c.Cuentas)
                      .HasForeignKey(e => e.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de Movimiento
            modelBuilder.Entity<Movimiento>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Monto).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                
                // Relación con Cuenta (Un movimiento pertenece a una cuenta)
                entity.HasOne(e => e.Cuenta)
                      .WithMany(c => c.Movimientos)
                      .HasForeignKey(e => e.CuentaId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Relación con Transferencia (Un movimiento puede estar asociado a una transferencia)
                entity.HasOne(e => e.Transferencia)
                      .WithMany(t => t.Movimientos)
                      .HasForeignKey(e => e.TransferenciaId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuración de BancoExterno
            modelBuilder.Entity<BancoExterno>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CodigoIdentificacion).IsUnique();
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CodigoIdentificacion).IsRequired().HasMaxLength(10);
                entity.Property(e => e.UrlBase).IsRequired().HasMaxLength(500);
            });

            // Configuración de Transferencia
            modelBuilder.Entity<Transferencia>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Monto).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.Property(e => e.NumeroCuentaDestino).HasMaxLength(50);
                entity.Property(e => e.ReferenciaExterna).HasMaxLength(100);
                
                // Relación con Cuenta Origen (Una transferencia tiene una cuenta de origen)
                entity.HasOne(e => e.CuentaOrigen)
                      .WithMany()
                      .HasForeignKey(e => e.CuentaOrigenId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Relación con Cuenta Destino (Una transferencia puede tener una cuenta de destino local)
                entity.HasOne(e => e.CuentaDestino)
                      .WithMany()
                      .HasForeignKey(e => e.CuentaDestinoId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Relación con Banco Externo (Una transferencia puede ser hacia un banco externo)
                entity.HasOne(e => e.BancoExterno)
                      .WithMany(b => b.TransferenciasRecibidas)
                      .HasForeignKey(e => e.BancoExternoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Datos semilla para BancosExternos
            modelBuilder.Entity<BancoExterno>().HasData(
                new BancoExterno
                {
                    Id = 1,
                    Nombre = "Banco Nacional",
                    CodigoIdentificacion = "BN001",
                    UrlBase = "https://api.banconacional.com",
                    Activo = true,
                    FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new BancoExterno
                {
                    Id = 2,
                    Nombre = "Banco Internacional",
                    CodigoIdentificacion = "BI002",
                    UrlBase = "https://api.bancointernacional.com",
                    Activo = true,
                    FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}