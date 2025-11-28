using AhorroLand.Domain;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AhorroLand.Infrastructure.Persistence.Command.Configurations.Configurations
{
    public class GastoConfiguration : IEntityTypeConfiguration<Gasto>
    {
        public void Configure(EntityTypeBuilder<Gasto> builder)
        {
            builder.ToTable("gastos");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd().HasConversion(
                id => id.Value,
                value => new GastoId(value)
            ); ;

            // Configurar conversiones de Value Objects
            builder.Property(e => e.Importe)
                .HasColumnName("importe")
                .HasColumnType("decimal(18,2)")
                .IsRequired()
                .HasConversion(
                    importe => importe.Valor,
                    value => new Cantidad(value));

            builder.Property(e => e.Fecha)
                .HasColumnName("fecha")
                .IsRequired()
                .HasConversion(
                    fecha => fecha.Valor,
                    value => new FechaRegistro(value));

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
                    value => new ConceptoId(value));

            builder.Property(e => e.ProveedorId)
                .HasColumnName("id_proveedor")
                .IsRequired()
                .HasConversion(
                    proveedorId => proveedorId.Value,
                    value => new ProveedorId(value));

            builder.Property(e => e.PersonaId)
                .HasColumnName("id_persona")
                .IsRequired()
                .HasConversion(
                    personaId => personaId.Value,
                    value => new PersonaId(value));

            builder.Property(e => e.CuentaId)
                .HasColumnName("id_cuenta")
                .IsRequired()
                .HasConversion(
                    cuentaId => cuentaId.Value,
                    value => new CuentaId(value));

            builder.Property(e => e.FormaPagoId)
                .HasColumnName("id_forma_pago")
                .IsRequired()
                .HasConversion(
                    formaPagoId => formaPagoId.Value,
                    value => new FormaPagoId(value));

            builder.Property(e => e.UsuarioId)
                .HasColumnName("id_usuario")
                .IsRequired()
                .HasConversion(
                    usuarioId => usuarioId.Value,
                    value => new UsuarioId(value));

            builder.Property(e => e.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .IsRequired()
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            // Relaciones usando SOLO el nombre de columna
            builder.HasOne(e => e.Concepto)
                .WithMany()
                .HasForeignKey("id_concepto")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Proveedor)
                .WithMany()
                .HasForeignKey("id_proveedor")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Persona)
                .WithMany()
                .HasForeignKey("id_persona")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Cuenta)
                .WithMany()
                .HasForeignKey("id_cuenta")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.FormaPago)
                .WithMany()
                .HasForeignKey("id_forma_pago")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey("id_usuario")
                .OnDelete(DeleteBehavior.Restrict);

            // Índices críticos para rendimiento
            builder.HasIndex(e => new { e.UsuarioId, e.Fecha })
                .HasDatabaseName("idx_gastos_usuario_fecha");

            builder.HasIndex(e => new { e.CuentaId, e.Fecha })
                .HasDatabaseName("idx_gastos_cuenta_fecha");
        }
    }
}
