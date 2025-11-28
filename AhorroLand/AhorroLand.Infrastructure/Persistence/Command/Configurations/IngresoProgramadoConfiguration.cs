using AhorroLand.Domain;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AhorroLand.Infrastructure.Persistence.Command.Configurations.Configurations
{
    public class IngresoProgramadoConfiguration : IEntityTypeConfiguration<IngresoProgramado>
    {
        public void Configure(EntityTypeBuilder<IngresoProgramado> builder)
        {
            builder.ToTable("ingresos_programados");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd().HasConversion(
                id => id.Value,
                value => new IngresoProgramadoId(value)
            ); ;

            // ? Configurar conversión de Value Object Cantidad
            builder.Property(e => e.Importe)
            .HasColumnName("importe")
              .HasColumnType("decimal(18,2)")
        .IsRequired()
        .HasConversion(
               importe => importe.Valor,
              value => new Cantidad(value));

            builder.Property(e => e.Descripcion)
                .HasColumnName("descripcion")
                .HasColumnType("varchar")
                .HasMaxLength(200)
                .IsRequired(false)
                .HasConversion(
                    descripcion => descripcion.HasValue ? descripcion.Value._Value : null,
                    value => string.IsNullOrEmpty(value) ? null : new Descripcion(value));

            // ? Configurar conversión de Value Objects de IDs
            builder.Property(e => e.CuentaId)
          .HasColumnName("id_cuenta")
       .IsRequired()
       .HasConversion(
            cuentaId => cuentaId.Value,
          value => new CuentaId(value));

            builder.Property(e => e.FormaPagoId)
.HasColumnName("id_forma_pago")
.IsRequired()
.HasConversion(
formaPagoId => formaPagoId.Value,
value => new FormaPagoId(value));

            builder.Property(e => e.PersonaId)
.HasColumnName("id_persona")
.IsRequired()
.HasConversion(
personaId => personaId.Value,
value => new PersonaId(value));

            builder.Property(e => e.ConceptoId)
.HasColumnName("id_concepto")
.IsRequired()
.HasConversion(
conceptoId => conceptoId.Value,
value => new ConceptoId(value));

            builder.Property(e => e.ClienteId)
       .HasColumnName("id_cliente")
       .IsRequired()
    .HasConversion(
    clienteId => clienteId.Value,
            value => new ClienteId(value));

            builder.Property(e => e.UsuarioId)
       .HasColumnName("id_usuario")
          .IsRequired()
     .HasConversion(
    usuarioId => usuarioId.Value,
       value => new UsuarioId(value));

            // ? Configurar Frecuencia
            builder.Property(e => e.Frecuencia)
            .HasColumnName("frecuencia")
          .HasColumnType("varchar(100)")
            .IsRequired()
     .HasConversion(
      frecuencia => frecuencia.Value,
           value => new Frecuencia(value));

            // ? Configurar FechaEjecucion
            builder.Property(e => e.FechaEjecucion)
            .HasColumnName("fecha_ejecucion")
             .IsRequired();

            builder.Property(e => e.FechaCreacion)
                 .HasColumnName("fecha_creacion")
              .IsRequired()
                   .ValueGeneratedOnAdd()
              .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
