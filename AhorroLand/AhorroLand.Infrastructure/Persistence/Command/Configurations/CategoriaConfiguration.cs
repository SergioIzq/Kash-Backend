using AhorroLand.Domain;
using AhorroLand.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AhorroLand.Infrastructure.Persistence.Command.Configurations.Configurations
{
    public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
    {
        public void Configure(EntityTypeBuilder<Categoria> builder)
        {
            builder.ToTable("categorias"); // ?? FIX: Nombre correcto de tabla (plural)
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

            // ?? FIX CRÍTICO: Configurar conversiones de Value Objects
            builder.Property(e => e.Nombre)
                .HasColumnName("nombre")
                .HasColumnType("varchar")
                .HasMaxLength(100)
                .IsRequired()
                .HasConversion(
                    nombre => nombre.Value,              // Value Object -> DB
                    value => new Nombre(value));         // DB -> Value Object

            builder.Property(e => e.Descripcion)
                .HasColumnName("descripcion")
                .HasColumnType("varchar")
                .HasMaxLength(200)
                .IsRequired(false)
                .HasConversion(
                    descripcion => descripcion.HasValue ? descripcion.Value._Value : null,  // Value Object -> DB
                    value => string.IsNullOrEmpty(value) ? (Descripcion?)null : new Descripcion(value)); // DB -> Value Object

            builder.Property(e => e.IdUsuario)
                .HasColumnName("usuario_id") // ?? FIX: Nombre consistente con otras tablas
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

            // ?? OPTIMIZACIÓN: Índices para mejorar rendimiento
            builder.HasIndex(e => new { e.IdUsuario, e.FechaCreacion })
                .HasDatabaseName("idx_categorias_usuario_fecha");

            builder.HasIndex(e => new { e.IdUsuario, e.Nombre })
                .HasDatabaseName("idx_categorias_usuario_nombre");
        }
    }
}
