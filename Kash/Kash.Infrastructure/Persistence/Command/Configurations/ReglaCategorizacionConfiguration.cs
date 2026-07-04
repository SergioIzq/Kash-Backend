using Kash.Domain;
using Kash.Shared.Domain.ValueObjects.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kash.Infrastructure.Persistence.Command.Configurations.Configurations
{
    public class ReglaCategorizacionConfiguration : IEntityTypeConfiguration<ReglaCategorizacion>
    {
        public void Configure(EntityTypeBuilder<ReglaCategorizacion> builder)
        {
            builder.ToTable("reglas_categorizacion");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd().HasConversion(
                id => id.Value,
                value => ReglaCategorizacionId.CreateFromDatabase(value));

            builder.Property(e => e.Patron)
                .HasColumnName("patron")
                .HasColumnType("varchar")
                .HasMaxLength(ReglaCategorizacion.PatronMaxLength)
                .IsRequired();

            builder.Property(e => e.Tipo)
                .HasColumnName("tipo")
                .HasColumnType("varchar")
                .HasMaxLength(10)
                .IsRequired(false);

            builder.Property(e => e.CategoriaNombre)
                .HasColumnName("categoria_nombre")
                .HasColumnType("varchar")
                .HasMaxLength(ReglaCategorizacion.NombreMaxLength)
                .IsRequired();

            builder.Property(e => e.ConceptoNombre)
                .HasColumnName("concepto_nombre")
                .HasColumnType("varchar")
                .HasMaxLength(ReglaCategorizacion.NombreMaxLength)
                .IsRequired(false);

            builder.Property(e => e.ProveedorNombre)
                .HasColumnName("proveedor_nombre")
                .HasColumnType("varchar")
                .HasMaxLength(ReglaCategorizacion.NombreMaxLength)
                .IsRequired(false);

            builder.Property(e => e.FormaPagoNombre)
                .HasColumnName("forma_pago_nombre")
                .HasColumnType("varchar")
                .HasMaxLength(ReglaCategorizacion.NombreMaxLength)
                .IsRequired(false);

            builder.Property(e => e.Prioridad)
                .HasColumnName("prioridad")
                .IsRequired();

            builder.Property(e => e.Activo)
                .HasColumnName("activo")
                .IsRequired();

            builder.Property(e => e.UsuarioId)
                .HasColumnName("id_usuario")
                .IsRequired()
                .HasConversion(
                    usuarioId => usuarioId.Value,
                    value => UsuarioId.CreateFromDatabase(value));

            builder.Property(e => e.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(e => e.FechaCreacion)
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            builder.HasIndex(e => new { e.UsuarioId, e.Activo, e.Prioridad })
                .HasDatabaseName("idx_reglas_categorizacion_usuario_activo_prioridad");
        }
    }
}
