using AhorroLand.Domain;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AhorroLand.Infrastructure.Persistence.Command.Configurations.Configurations
{
    public class TraspasoProgramadoConfiguration : IEntityTypeConfiguration<TraspasoProgramado>
    {
        public void Configure(EntityTypeBuilder<TraspasoProgramado> builder)
        {
            builder.ToTable("traspasos_programados");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd().HasConversion(
                id => id.Value,
                value => TraspasoProgramadoId.CreateFromDatabase(value)
            ); ;

            // ? Configurar conversión de Value Object Cantidad
            builder.Property(e => e.Importe)
    .HasColumnName("importe")
                .HasColumnType("decimal(18,2)")
  .IsRequired()
    .HasConversion(
         importe => importe.Valor,
      value => Cantidad.CreateFromDatabase(value));

    // ? Configurar conversión de Frecuencia
            builder.Property(e => e.Frecuencia)
.HasColumnName("frecuencia")
           .HasColumnType("varchar(100)")
      .IsRequired()
       .HasConversion(
     frecuencia => frecuencia.Value,
       value => Frecuencia.CreateFromDatabase(value));

            // ? Configurar conversión de Value Objects de IDs
     builder.Property(e => e.CuentaOrigenId)
 .HasColumnName("id_cuenta_origen")
    .IsRequired()
 .HasConversion(
           cuentaId => cuentaId.Value,
            value => CuentaId.CreateFromDatabase(value));

    builder.Property(e => e.CuentaDestinoId)
          .HasColumnName("id_cuenta_destino")
       .IsRequired()
    .HasConversion(
         cuentaId => cuentaId.Value,
    value => CuentaId.CreateFromDatabase(value));

       builder.Property(e => e.UsuarioId)
                .HasColumnName("id_usuario")
      .IsRequired()
                .HasConversion(
         usuarioId => usuarioId.Value,
      value => UsuarioId.CreateFromDatabase(value));

            // ? Configurar Descripcion nullable
       builder.Property(e => e.Descripcion)
 .HasColumnName("descripcion")
        .HasColumnType("varchar(200)")
        .IsRequired(false)
      .HasConversion(
              descripcion => descripcion.HasValue ? descripcion.Value._Value : null,
           value => string.IsNullOrEmpty(value) ? null : new Descripcion(value));

            builder.Property(e => e.Descripcion)
    .HasColumnName("descripcion")
    .HasColumnType("varchar")
    .HasMaxLength(200)
    .IsRequired(false)
    .HasConversion(
        descripcion => descripcion.HasValue ? descripcion.Value._Value : null,
        value => string.IsNullOrEmpty(value) ? null : new Descripcion(value));

            // ? FechaEjecucion
            builder.Property(e => e.FechaEjecucion)
      .HasColumnName("fecha_ejecucion")
        .IsRequired();

// ? Activo
            builder.Property(e => e.Activo)
        .HasColumnName("activo")
    .IsRequired();

            // ? HangfireJobId
      builder.Property(e => e.HangfireJobId)
.HasColumnName("hangfire_job_id")
        .HasColumnType("varchar(100)")
     .IsRequired();

      // ? Ignorar propiedades derivadas (solo para proyecciones)
            builder.Ignore(e => e.SaldoCuentaOrigen);
   builder.Ignore(e => e.SaldoCuentaDestino);

            builder.Property(e => e.FechaCreacion)
   .HasColumnName("fecha_creacion")
  .IsRequired()
    .ValueGeneratedOnAdd()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
