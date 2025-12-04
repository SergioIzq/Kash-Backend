using AhorroLand.Domain;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AhorroLand.Infrastructure.Persistence.Command.Configurations.Configurations
{
    public class FormaPagoConfiguration : IEntityTypeConfiguration<FormaPago>
    {
        public void Configure(EntityTypeBuilder<FormaPago> builder)
        {
            builder.ToTable("formas_pago"); // ?? FIX: Nombre correcto de tabla (plural)
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd().HasConversion(
                id => id.Value,
                value => FormaPagoId.Create(value).Value
            ); ;

            // ?? FIX CRÍTICO: Configurar conversiones de Value Objects
            builder.Property(e => e.Nombre)
                .HasColumnName("nombre")
                .HasColumnType("varchar")
                .HasMaxLength(100)
                .IsRequired()
                .HasConversion(
                    nombre => nombre.Value,
                    value => Nombre.Create(value).Value);

            builder.Property(e => e.UsuarioId)
                .HasColumnName("usuario_id") // ?? FIX: Nombre consistente
                .IsRequired()
                .HasConversion(
                    usuarioId => usuarioId.Value,
                    value => UsuarioId.Create(value).Value);

            builder.Property(e => e.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(e => e.FechaCreacion)
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            // ?? OPTIMIZACIÓN: Índices
            builder.HasIndex(e => new { e.UsuarioId, e.FechaCreacion })
                .HasDatabaseName("idx_formas_pago_usuario_fecha");

            builder.HasIndex(e => new { e.UsuarioId, e.Nombre })
                .HasDatabaseName("idx_formas_pago_usuario_nombre");
        }
    }
}
