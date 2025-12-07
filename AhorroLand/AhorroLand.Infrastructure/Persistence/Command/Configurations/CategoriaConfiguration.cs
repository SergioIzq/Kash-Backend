using AhorroLand.Domain;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AhorroLand.Infrastructure.Persistence.Command.Configurations.Configurations
{
    public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
    {
        public void Configure(EntityTypeBuilder<Categoria> builder)
        {
            builder.ToTable("categorias");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd().HasConversion(
                id => id.Value,
                value => CategoriaId.CreateFromDatabase(value)
            );

            // Configurar conversiones de Value Objects
            builder.Property(e => e.Nombre)
                .HasColumnName("nombre")
                .HasColumnType("varchar")
                .HasMaxLength(100)
                .IsRequired()
                .HasConversion(
                    nombre => nombre.Value,
                    value => Nombre.CreateFromDatabase(value));

            // ? RESTAURADO: Mapeo de Descripcion como Value Object nullable
            builder.Property(e => e.Descripcion)
                .HasColumnName("descripcion")
                .HasColumnType("varchar")
                .HasMaxLength(200)
                .IsRequired(false)
                .HasConversion(
                    descripcion => descripcion.HasValue ? descripcion.Value._Value : null,
                    value => string.IsNullOrEmpty(value) ? null : new Descripcion(value));

            builder.Property(e => e.IdUsuario)
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

            // Índices para mejorar rendimiento
            builder.HasIndex(e => new { e.IdUsuario, e.FechaCreacion })
                .HasDatabaseName("idx_categorias_usuario_fecha");

            builder.HasIndex(e => new { e.IdUsuario, e.Nombre })
                .HasDatabaseName("idx_categorias_usuario_nombre");
        }
    }
}
