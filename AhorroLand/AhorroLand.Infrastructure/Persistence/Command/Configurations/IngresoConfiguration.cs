using AhorroLand.Domain;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AhorroLand.Infrastructure.Persistence.Command.Configurations.Configurations
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
            ); ;

            // --- VALUE OBJECTS (Importe, Fecha, Descripción) ---
            // (Esta parte estaba bien, la dejo igual por brevedad)
            builder.Property(e => e.Importe)
                   .HasColumnName("importe")
                   .HasColumnType("decimal(18,2)")
                   .IsRequired()
                   .HasConversion(i => i.Valor, v => Cantidad.CreateFromDatabase(v));

            builder.Property(e => e.Fecha)
                   .HasColumnName("fecha")
                   .IsRequired()
                   .HasConversion(f => f.Valor, v => FechaRegistro.CreateFromDatabase(v));

            builder.Property(e => e.Descripcion)
                   .HasColumnName("descripcion")
                   .HasColumnType("varchar")
                   .HasMaxLength(200)
                   .IsRequired(false)
                   .HasConversion(
                       d => d.HasValue ? d.Value._Value : null,
                       v => string.IsNullOrEmpty(v) ? null : new Descripcion(v));

            // --- CONFIGURACIÓN DE PROPIEDADES FK (Esto está BIEN) ---
            // EF Core usará estas configuraciones (nombre de columna y conversión)
            // cuando las vinculemos abajo en las relaciones.

            builder.Property(e => e.ConceptoId)
                   .HasColumnName("id_concepto")
                   .IsRequired();

            builder.Property(e => e.ClienteId)
                   .HasColumnName("id_cliente")
                   .IsRequired();

            builder.Property(e => e.PersonaId)
                   .HasColumnName("id_persona")
                   .IsRequired();

            builder.Property(e => e.CuentaId)
                   .HasColumnName("id_cuenta")
                   .IsRequired();

            builder.Property(e => e.FormaPagoId)
                   .HasColumnName("id_forma_pago")
                   .IsRequired();

            builder.Property(e => e.UsuarioId)
                   .HasColumnName("id_usuario")
                   .IsRequired();

            builder.Property(e => e.FechaCreacion)
                   .HasColumnName("fecha_creacion")
                   .IsRequired()
                   .ValueGeneratedOnAdd()
                   .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            // --- RELACIONES (AQUÍ ESTABA EL ERROR) ---
            // CORRECCIÓN: Usar la expresión lambda (e => e.PropiedadId) en lugar del string.
            // Al hacer esto, EF Core reutiliza la configuración de columna y conversión definida arriba.

            builder.HasOne(e => e.Concepto)
                   .WithMany()
                   .HasForeignKey(e => e.ConceptoId) // <--- CORREGIDO (Antes "id_concepto")
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Cliente)
                   .WithMany()
                   .HasForeignKey(e => e.ClienteId) // <--- CORREGIDO (Antes "id_cliente")
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Persona)
                   .WithMany()
                   .HasForeignKey(e => e.PersonaId) // <--- CORREGIDO
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Cuenta)
                   .WithMany()
                   .HasForeignKey(e => e.CuentaId) // <--- CORREGIDO
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.FormaPago)
                   .WithMany()
                   .HasForeignKey(e => e.FormaPagoId) // <--- CORREGIDO
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Usuario)
                   .WithMany()
                   .HasForeignKey(e => e.UsuarioId) // <--- CORREGIDO
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}