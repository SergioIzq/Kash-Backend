using Kash.Domain;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kash.Infrastructure.Persistence.Command.Configurations.Configurations
{
    public class GastoConfiguration : IEntityTypeConfiguration<Gasto>
    {
        public void Configure(EntityTypeBuilder<Gasto> builder)
        {
            builder.ToTable("gastos");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd().HasConversion(
                id => id.Value,
                value => GastoId.CreateFromDatabase(value)
            ); ;

            // Configurar conversiones de Value Objects
            builder.Property(e => e.Importe)
                .HasColumnName("importe")
                .HasColumnType("decimal(18,2)")
                .IsRequired()
                .HasConversion(
                    importe => importe.Valor,
                    value => Cantidad.CreateFromDatabase(value));

            builder.Property(e => e.Fecha)
                .HasColumnName("fecha")
                .IsRequired()
                .HasConversion(
                    fecha => fecha.Valor,
                    value => FechaRegistro.CreateFromDatabase(value));

            builder.Property(e => e.Descripcion)
                .HasColumnName("descripcion")
                .HasColumnType("varchar")
                .HasMaxLength(200)
                .IsRequired(false)
                .HasConversion(
                    descripcion => descripcion.HasValue ? descripcion.Value._Value : null,
                    value => string.IsNullOrEmpty(value) ? null : new Descripcion(value));

            // IDs como Value Objects
            builder.Property(e => e.ConceptoId)
                .HasColumnName("id_concepto")
                .IsRequired()
                .HasConversion(
                    conceptoId => conceptoId.Value,
                    value => ConceptoId.CreateFromDatabase(value));

            builder.Property(e => e.ProveedorId)
                .HasColumnName("id_proveedor")
                .IsRequired(false) // ✅ NULLABLE: Permite valores NULL en la BD
                .HasConversion(
                    proveedorId => proveedorId.HasValue ? proveedorId.Value.Value : (Guid?)null,
                    value => value.HasValue ? ProveedorId.CreateFromDatabase(value.Value) : null);

            builder.Property(e => e.PersonaId)
                .HasColumnName("id_persona")
                .IsRequired(false) // ✅ NULLABLE: Permite valores NULL en la BD
                .HasConversion(
                    personaId => personaId.HasValue ? personaId.Value.Value : (Guid?)null,
                    value => value.HasValue ? PersonaId.CreateFromDatabase(value.Value) : null);

            builder.Property(e => e.CuentaId)
                .HasColumnName("id_cuenta")
                .IsRequired()
                .HasConversion(
                    cuentaId => cuentaId.Value,
                    value => CuentaId.CreateFromDatabase(value));

            builder.Property(e => e.FormaPagoId)
                .HasColumnName("id_forma_pago")
                .IsRequired()
                .HasConversion(
                    formaPagoId => formaPagoId.Value,
                    value => FormaPagoId.CreateFromDatabase(value));

            builder.Property(e => e.UsuarioId)
                .HasColumnName("id_usuario")
                .IsRequired()
                .HasConversion(
                    usuarioId => usuarioId.Value,
                    value => UsuarioId.CreateFromDatabase(value));

            builder.Property(e => e.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .IsRequired()
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            // Relaciones usando SOLO el nombre de columna
            builder.HasOne(e => e.Concepto)
                .WithMany()
                .HasForeignKey(e => e.ConceptoId) // <--- CAMBIO: Usa la propiedad tipada
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Proveedor)
                .WithMany()
                .HasForeignKey(e => e.ProveedorId) // <--- CAMBIO
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Persona)
                .WithMany()
                .HasForeignKey(e => e.PersonaId)   // <--- CAMBIO
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Cuenta)
                .WithMany()
                .HasForeignKey(e => e.CuentaId)    // <--- CAMBIO
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.FormaPago)
                .WithMany()
                .HasForeignKey(e => e.FormaPagoId) // <--- CAMBIO
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)   // <--- CAMBIO
                .OnDelete(DeleteBehavior.Cascade);

            // Índices críticos para rendimiento
            builder.HasIndex(e => new { e.UsuarioId, e.Fecha })
                .HasDatabaseName("idx_gastos_usuario_fecha");

            builder.HasIndex(e => new { e.CuentaId, e.Fecha })
                .HasDatabaseName("idx_gastos_cuenta_fecha");
        }
    }
}
