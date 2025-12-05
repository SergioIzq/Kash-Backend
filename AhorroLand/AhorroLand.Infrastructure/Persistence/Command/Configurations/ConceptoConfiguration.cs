using AhorroLand.Domain;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AhorroLand.Infrastructure.Persistence.Command.Configurations.Configurations
{
    public class ConceptoConfiguration : IEntityTypeConfiguration<Concepto>
    {
        public void Configure(EntityTypeBuilder<Concepto> builder)
        {
            builder.ToTable("conceptos");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd().HasConversion(
                id => id.Value,
                value => ConceptoId.CreateFromDatabase(value)   
            );

            // ?? FIX CRÍTICO: Configurar conversiones de Value Objects
            builder.Property(e => e.Nombre)
                .HasColumnName("nombre")
                .HasColumnType("varchar")
                .HasMaxLength(100)
                .IsRequired()
                .HasConversion(
                    nombre => nombre.Value,
                    value => Nombre.CreateFromDatabase(value));

            builder.Property(e => e.CategoriaId)
                .HasColumnName("id_categoria")
                .IsRequired()
                .HasConversion(
                    categoriaId => categoriaId.Value,
                    value => CategoriaId.CreateFromDatabase(value));

            builder.Property(e => e.UsuarioId)
                .HasColumnName("id_usuario") // ?? FIX: Nombre consistente
                .IsRequired()
                .HasConversion(
                    usuarioId => usuarioId.Value,
                    value => UsuarioId.CreateFromDatabase(value));

            builder.Property(e => e.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .IsRequired()
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            builder.HasOne(e => e.Categoria)
                            .WithMany()
                            .HasForeignKey(e => e.CategoriaId)
                            .OnDelete(DeleteBehavior.Restrict);

            // ?? OPTIMIZACIÓN: Índices
            builder.HasIndex(e => new { e.UsuarioId, e.FechaCreacion })
                .HasDatabaseName("idx_conceptos_usuario_fecha");

            builder.HasIndex(e => new { e.UsuarioId, e.CategoriaId })
                .HasDatabaseName("idx_conceptos_usuario_categoria");
        }
    }
}
