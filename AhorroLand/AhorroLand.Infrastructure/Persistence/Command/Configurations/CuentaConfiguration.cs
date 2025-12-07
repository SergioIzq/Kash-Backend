using AhorroLand.Domain;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AhorroLand.Infrastructure.Persistence.Command.Configurations.Configurations
{
    public class CuentaConfiguration : IEntityTypeConfiguration<Cuenta>
    {
        public void Configure(EntityTypeBuilder<Cuenta> builder)
        {
            builder.ToTable("cuentas"); // ?? FIX: Nombre correcto de tabla (plural)
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd().HasConversion(
                id => id.Value,
                value => CuentaId.CreateFromDatabase(value)
            ); ;

            // ?? FIX CRÍTICO: Configurar conversiones de Value Objects
            builder.Property(e => e.Nombre)
                .HasColumnName("nombre")
                .HasColumnType("varchar")
                .HasMaxLength(100)
                .IsRequired()
                .HasConversion(
                    nombre => nombre.Value,
                    value => Nombre.CreateFromDatabase(value));

            builder.Property(e => e.Saldo)
                .HasColumnName("saldo")
                .HasColumnType("decimal(18,2)")
                .IsRequired()
                .HasConversion(
                    saldo => saldo.Valor,
                    value => Cantidad.CreateFromDatabase(value));

            builder.Property(e => e.UsuarioId)
                .HasColumnName("id_usuario") // ?? FIX: Nombre consistente
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

            // ?? OPTIMIZACIÓN: Índices
            builder.HasIndex(e => new { e.UsuarioId, e.FechaCreacion })
                .HasDatabaseName("idx_cuentas_usuario_fecha");

            builder.HasIndex(e => new { e.UsuarioId, e.Nombre })
                .HasDatabaseName("idx_cuentas_usuario_nombre");
        }
    }
}
