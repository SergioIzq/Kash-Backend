using AhorroLand.Domain;
using AhorroLand.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AhorroLand.Infrastructure.Persistence.Command.Configurations.Configurations
{
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.ToTable("clientes"); // ✅ Nombre correcto de tabla (plural)
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

            // 🔧 FIX CRÍTICO: Configurar conversiones de Value Objects
            builder.Property(e => e.Nombre)
                .HasColumnName("nombre")
                .HasColumnType("varchar")
                .HasMaxLength(100)
                .IsRequired()
                .HasConversion(
                    nombre => nombre.Value,              // Value Object -> DB
                    value => new Nombre(value));         // DB -> Value Object

            builder.Property(e => e.UsuarioId)
                .HasColumnName("usuario_id") // ✅ Nombre consistente
                .IsRequired()
                .HasConversion(
                    usuarioId => usuarioId.Value,        // Value Object -> DB
                    value => new UsuarioId(value));      // DB -> Value Object

            builder.Property(e => e.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(e => e.FechaCreacion)
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            // 🚀 OPTIMIZACIÓN: Índice compuesto para filtros por usuario
            builder.HasIndex(e => new { e.UsuarioId, e.FechaCreacion })
                .HasDatabaseName("idx_clientes_usuario_fecha");

            // 🚀 OPTIMIZACIÓN: Índice para búsquedas por nombre
            builder.HasIndex(e => new { e.UsuarioId, e.Nombre })
                .HasDatabaseName("idx_clientes_usuario_nombre");
        }
    }
}
