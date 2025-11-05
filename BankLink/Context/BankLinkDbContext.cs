// ============================================================================
// BANKLINKDBCONTEXT.CS - Contexto de Entity Framework Core
// ============================================================================
// Este archivo define el DbContext, que es el PUNTO DE ENTRADA de Entity
// Framework para acceder a la base de datos.
//
// RESPONSABILIDADES:
// 1. Definir los DbSets (mapean a tablas de SQL Server)
// 2. Configurar relaciones entre entidades (Foreign Keys)
// 3. Configurar índices únicos
// 4. Configurar precisión de tipos de datos (decimales)
// 5. Configurar comportamiento de eliminación
// ============================================================================

using BankLink.Models;
using Microsoft.EntityFrameworkCore;

namespace BankLink.Context
{
    /// <summary>
    /// DbContext principal de BankLink
    /// Hereda de DbContext (clase base de Entity Framework)
    /// </summary>
    public class BankLinkDbContext : DbContext
    {
        // ====================================================================
        // CONSTRUCTOR
        // ====================================================================
        // Recibe opciones de configuración (como connection string)
        // Estas opciones se configuran en Program.cs con UseSqlServer
        public BankLinkDbContext(DbContextOptions<BankLinkDbContext> options) : base(options)
        {
        }

        // ====================================================================
        // DBSETS - MAPEO A TABLAS DE BASE DE DATOS
        // ====================================================================
        // Cada DbSet<T> representa una tabla en SQL Server
        // Entity Framework usa estos para generar queries SQL
        
        /// <summary>
        /// Tabla de Clientes
        /// SELECT * FROM Clientes → Clientes.ToList()
        /// </summary>
        public DbSet<Cliente> Clientes { get; set; }
        
        /// <summary>
        /// Tabla de Cuentas Bancarias
        /// </summary>
        public DbSet<Cuenta> Cuentas { get; set; }
        
        /// <summary>
        /// Tabla de Movimientos (auditoría de transacciones)
        /// </summary>
        public DbSet<Movimiento> Movimientos { get; set; }
        
        /// <summary>
        /// Tabla de Bancos Externos
        /// </summary>
        public DbSet<BancoExterno> BancosExternos { get; set; }
        
        /// <summary>
        /// Tabla de Transferencias
        /// </summary>
        public DbSet<Transferencia> Transferencias { get; set; }

        // ====================================================================
        // ONMODELCREATING - CONFIGURACIÓN DEL MODELO
        // ====================================================================
        // Este método se ejecuta cuando EF Core crea el modelo de la BD
        // Aquí configuramos relaciones, índices, precisión, etc.
        // Es CRÍTICO para definir cómo se estructura la base de datos
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ================================================================
            // SECCIÓN 1: CONFIGURACIÓN DE RELACIONES (FOREIGN KEYS)
            // ================================================================
            
            // ----------------------------------------------------------------
            // RELACIÓN: Cliente -> Cuentas (1:N)
            // ----------------------------------------------------------------
            // Un cliente puede tener MUCHAS cuentas
            // Una cuenta pertenece a UN SOLO cliente
            modelBuilder.Entity<Cuenta>()
                // Desde el lado de Cuenta: HasOne (tiene UN cliente)
                .HasOne(c => c.ClientePropietario)
                // Desde el lado de Cliente: WithMany (tiene MUCHAS cuentas)
                .WithMany(cl => cl.Cuentas)
                // Foreign Key: IdClientePropietario en tabla Cuentas
                .HasForeignKey(c => c.IdClientePropietario)
                // DeleteBehavior.Restrict: NO permitir eliminar cliente si tiene cuentas
                // Esto previene pérdida accidental de datos
                .OnDelete(DeleteBehavior.Restrict);

            // ----------------------------------------------------------------
            // RELACIÓN: Cuenta -> Movimientos (1:N)
            // ----------------------------------------------------------------
            // Una cuenta puede tener MUCHOS movimientos
            // Un movimiento pertenece a UNA SOLA cuenta
            modelBuilder.Entity<Movimiento>()
                .HasOne(m => m.Cuenta)  // Movimiento tiene UNA cuenta
                .WithMany(c => c.Movimientos)  // Cuenta tiene MUCHOS movimientos
                .HasForeignKey(m => m.IdCuenta)  // FK en tabla Movimientos
                .OnDelete(DeleteBehavior.Restrict);  // NO eliminar cuenta si tiene movimientos

            // ----------------------------------------------------------------
            // RELACIÓN: Transferencia -> Cuenta Origen (N:1)
            // ----------------------------------------------------------------
            // Una transferencia tiene UNA cuenta origen
            // Una cuenta puede tener MUCHAS transferencias
            modelBuilder.Entity<Transferencia>()
                .HasOne(t => t.CuentaOrigen)  // Transferencia tiene UNA cuenta origen
                .WithMany()  // No definimos colección en Cuenta (opcional)
                .HasForeignKey(t => t.IdCuentaOrigen)  // FK en tabla Transferencias
                .OnDelete(DeleteBehavior.Restrict);  // NO eliminar cuenta con transferencias

            // ----------------------------------------------------------------
            // RELACIÓN: Transferencia -> Banco Externo (N:1 OPCIONAL)
            // ----------------------------------------------------------------
            // Una transferencia PUEDE tener un banco destino (nullable)
            // Si es transferencia interna, IdBancoDestino es NULL
            modelBuilder.Entity<Transferencia>()
                .HasOne(t => t.BancoDestino)  // Transferencia tiene UN banco (opcional)
                .WithMany()  // Banco puede tener MUCHAS transferencias
                .HasForeignKey(t => t.IdBancoDestino)  // FK nullable
                .OnDelete(DeleteBehavior.Restrict);  // NO eliminar banco con transferencias

            // ================================================================
            // SECCIÓN 2: CONFIGURACIÓN DE PRECISIÓN DECIMAL
            // ================================================================
            // Los valores monetarios SIEMPRE deben usar decimal, nunca float/double
            // Precisión (18, 2) significa: 18 dígitos totales, 2 decimales
            // Ejemplo: 9999999999999999.99 es el máximo valor
            
            // Saldo de cuentas: decimal(18,2)
            modelBuilder.Entity<Cuenta>()
                .Property(c => c.SaldoActual)
                .HasPrecision(18, 2);  // Ej: 1500000.50

            // Monto de movimientos: decimal(18,2)
            modelBuilder.Entity<Movimiento>()
                .Property(m => m.Monto)
                .HasPrecision(18, 2);  // Ej: 5000.75

            // Monto de transferencias: decimal(18,2)
            modelBuilder.Entity<Transferencia>()
                .Property(t => t.Monto)
                .HasPrecision(18, 2);  // Ej: 10000.00

            // ================================================================
            // SECCIÓN 3: CONFIGURACIÓN DE ÍNDICES ÚNICOS
            // ================================================================
            // Los índices únicos GARANTIZAN que no haya valores duplicados
            // Mejoran el rendimiento de búsquedas y previenen datos inconsistentes
            
            // ----------------------------------------------------------------
            // DNI único: No puede haber dos clientes con el mismo DNI
            // ----------------------------------------------------------------
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Dni)  // Crear índice en columna Dni
                .IsUnique();  // El índice debe ser único

            // ----------------------------------------------------------------
            // NombreUsuario único: No puede haber usuarios duplicados
            // ----------------------------------------------------------------
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.NombreUsuario)
                .IsUnique();  // Previene registro con username existente

            // ----------------------------------------------------------------
            // NumeroCuenta único: Cada cuenta tiene número único
            // ----------------------------------------------------------------
            modelBuilder.Entity<Cuenta>()
                .HasIndex(c => c.NumeroCuenta)
                .IsUnique();  // Garantiza que el número de cuenta es único

            // ----------------------------------------------------------------
            // CodigoIdentificacion único: Cada banco tiene código único
            // ----------------------------------------------------------------
            modelBuilder.Entity<BancoExterno>()
                .HasIndex(b => b.CodigoIdentificacion)
                .IsUnique();  // Previene registrar el mismo banco dos veces
        }
    }
}

// ============================================================================
// NOTAS PARA LA PRESENTACIÓN:
// ============================================================================
// 1. OnModelCreating es donde se configura TODO el modelo de datos
// 2. Las relaciones HasOne/WithMany definen Foreign Keys automáticamente
// 3. DeleteBehavior.Restrict previene eliminaciones accidentales
// 4. HasPrecision(18,2) es CRÍTICO para valores monetarios (evita errores de redondeo)
// 5. Los índices únicos mejoran performance y garantizan integridad
// 6. Entity Framework usa esto para generar la migración inicial
// ============================================================================
