using AhorroLand.Domain;
using AhorroLand.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AhorroLand.Infrastructure.Persistence.Command.Configurations.Configurations
{
    public class ConceptoConfiguration : IEntityTypeConfiguration<Concepto>
    {
        public void Configure(EntityTypeBuilder<Concepto> builder)
        {
            builder.ToTable("conceptos"); // ?? FIX: Nombre correcto de tabla (plural)
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

            // ?? FIX CRÍTICO: Configurar conversiones de Value Objects
            builder.Property(e => e.Nombre)
                .HasColumnName("nombre")
                .HasColumnType("varchar")
                .HasMaxLength(100)
                .IsRequired()
                .HasConversion(
                    nombre => nombre.Value,
                    value => new Nombre(value));

            builder.Property(e => e.CategoriaId)
                .HasColumnName("categoria_id")
                .IsRequired()
                .HasConversion(
                    categoriaId => categoriaId.Value,
                    value => new CategoriaId(value));

            builder.Property(e => e.UsuarioId)
                .HasColumnName("usuario_id") // ?? FIX: Nombre consistente
                .IsRequired()
                .HasConversion(
                    usuarioId => usuarioId.Value,
                    value => new UsuarioId(value));

            builder.Property(e => e.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .IsRequired()
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            // ? FIX: Relación con Categoria usando el nombre de columna en lugar de la propiedad
            builder.HasOne(e => e.Categoria)
                .WithMany()
                .HasForeignKey("categoria_id")  // ? Usar nombre de columna, no propiedad
                .OnDelete(DeleteBehavior.Restrict);

            // ?? OPTIMIZACIÓN: Índices
            builder.HasIndex(e => new { e.UsuarioId, e.FechaCreacion })
                .HasDatabaseName("idx_conceptos_usuario_fecha");

            builder.HasIndex(e => new { e.UsuarioId, e.CategoriaId })
                .HasDatabaseName("idx_conceptos_usuario_categoria");
        }
    }
}
