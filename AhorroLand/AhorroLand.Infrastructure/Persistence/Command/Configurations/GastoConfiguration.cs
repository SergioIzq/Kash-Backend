using AhorroLand.Domain;
using AhorroLand.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AhorroLand.Infrastructure.Persistence.Command.Configurations.Configurations
{
    public class GastoConfiguration : IEntityTypeConfiguration<Gasto>
    {
        public void Configure(EntityTypeBuilder<Gasto> builder)
        {
            builder.ToTable("gastos"); // ?? FIX: Nombre correcto de tabla (plural)
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

            // ?? FIX CRÍTICO: Configurar conversiones de Value Objects
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
                    value => string.IsNullOrEmpty(value) ? (Descripcion?)null : new Descripcion(value));

            // ?? FIX: IDs como Value Objects
            builder.Property(e => e.ConceptoId)
                .HasColumnName("concepto_id")
                .IsRequired()
                .HasConversion(
                    conceptoId => conceptoId.Value,
                    value => new ConceptoId(value));

            builder.Property(e => e.CategoriaId)
                .HasColumnName("categoria_id")
                .IsRequired()
                .HasConversion(
                    categoriaId => categoriaId.Value,
                    value => new CategoriaId(value));

            builder.Property(e => e.ProveedorId)
                .HasColumnName("proveedor_id")
                .IsRequired(false)
                .HasConversion(
                    proveedorId => proveedorId.Value,
                    value => new ProveedorId(value));

            builder.Property(e => e.PersonaId)
                .HasColumnName("persona_id")
                .IsRequired(false)
                .HasConversion(
                    personaId => personaId.Value,
                    value => new PersonaId(value));

            builder.Property(e => e.CuentaId)
                .HasColumnName("cuenta_id")
                .IsRequired()
                .HasConversion(
                    cuentaId => cuentaId.Value,
                    value => new CuentaId(value));

            builder.Property(e => e.FormaPagoId)
                .HasColumnName("forma_pago_id")
                .IsRequired(false)
                .HasConversion(
                    formaPagoId => formaPagoId.Value,
                    value => new FormaPagoId(value));

            builder.Property(e => e.UsuarioId)
                .HasColumnName("usuario_id") // ?? FIX: Nombre consistente
                .IsRequired()
                .HasConversion(
                    usuarioId => usuarioId.Value,
                    value => new UsuarioId(value));

            builder.Property(e => e.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(e => e.FechaCreacion)
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            // ?? Relaciones (para queries/proyecciones)
            builder.HasOne(e => e.Concepto)
                .WithMany()
                .HasForeignKey(e => e.ConceptoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Categoria)
                .WithMany()
                .HasForeignKey(e => e.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Proveedor)
                .WithMany()
                .HasForeignKey(e => e.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Persona)
                .WithMany()
                .HasForeignKey(e => e.PersonaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Cuenta)
                .WithMany()
                .HasForeignKey(e => e.CuentaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.FormaPago)
                .WithMany()
                .HasForeignKey(e => e.FormaPagoId)
                .OnDelete(DeleteBehavior.Restrict);

            // ?? OPTIMIZACIÓN: Índices críticos para rendimiento
            builder.HasIndex(e => new { e.UsuarioId, e.Fecha })
                .HasDatabaseName("idx_gastos_usuario_fecha");

            builder.HasIndex(e => new { e.UsuarioId, e.CategoriaId, e.Fecha })
                .HasDatabaseName("idx_gastos_usuario_categoria_fecha");

            builder.HasIndex(e => new { e.CuentaId, e.Fecha })
                .HasDatabaseName("idx_gastos_cuenta_fecha");
        }
    }
}
