using Kash.Domain;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kash.Infrastructure.Persistence.Command.Configurations.Configurations
{
    public class IngresoConfiguration : IEntityTypeConfiguration<Ingreso>
    {
        public void Configure(EntityTypeBuilder<Ingreso> builder)
        {
            builder.ToTable("ingresos");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd().HasConversion(
                id => id.Value,
                value => IngresoId.CreateFromDatabase(value)
            );

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

            // IDs como Value Objects obligatorios
            builder.Property(e => e.ConceptoId)
                .HasColumnName("id_concepto")
                .IsRequired()
                .HasConversion(
                    conceptoId => conceptoId.Value,
                    value => ConceptoId.CreateFromDatabase(value));

            // 🔥 NULLABLE: ClienteId es opcional
            builder.Property(e => e.ClienteId)
                .HasColumnName("id_cliente")
                .IsRequired(false);

            builder.Property(e => e.ClienteId)
                .HasConversion<Guid?>(
                    clienteId => clienteId.HasValue ? clienteId.Value.Value : null,
                    value => value.HasValue ? ClienteId.CreateFromDatabase(value.Value) : null);

            // 🔥 NULLABLE: PersonaId es opcional
            builder.Property(e => e.PersonaId)
                .HasColumnName("id_persona")
                .IsRequired(false);

            builder.Property(e => e.PersonaId)
                .HasConversion<Guid?>(
                    personaId => personaId.HasValue ? personaId.Value.Value : null,
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

            // Relaciones usando las propiedades tipadas
            builder.HasOne(e => e.Concepto)
                .WithMany()
                .HasForeignKey(e => e.ConceptoId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔥 NULLABLE: Cliente es opcional - relación nullable
            builder.HasOne(e => e.Cliente)
                .WithMany()
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.SetNull) // ✅ SetNull en lugar de Cascade para FK nullable
                .IsRequired(false); // ✅ Relación opcional

            // 🔥 NULLABLE: Persona es opcional - relación nullable
            builder.HasOne(e => e.Persona)
                .WithMany()
                .HasForeignKey(e => e.PersonaId)
                .OnDelete(DeleteBehavior.SetNull) // ✅ SetNull en lugar de Cascade para FK nullable
                .IsRequired(false); // ✅ Relación opcional

            builder.HasOne(e => e.Cuenta)
                .WithMany()
                .HasForeignKey(e => e.CuentaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.FormaPago)
                .WithMany()
                .HasForeignKey(e => e.FormaPagoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices críticos para rendimiento
            builder.HasIndex(e => new { e.UsuarioId, e.Fecha })
                .HasDatabaseName("idx_ingresos_usuario_fecha");

            builder.HasIndex(e => new { e.CuentaId, e.Fecha })
                .HasDatabaseName("idx_ingresos_cuenta_fecha");
        }
    }
}