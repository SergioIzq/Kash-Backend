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

            // Configurar conversiones de Value Objects
            builder.Property(e => e.Importe)
                .HasColumnName("importe")
                .HasColumnType("decimal(18,2)")
                .IsRequired()
                .HasConversion(
                    importe => importe.Valor,
                    value => Cantidad.CreateFromDatabase(value));

            builder.Property(e => e.FechaEjecucion)
                .HasColumnName("fecha_ejecucion")
                .IsRequired();

            builder.Property(e => e.Descripcion)
                .HasColumnName("descripcion")
                .HasColumnType("varchar")
                .HasMaxLength(200)
                .IsRequired(false)
                .HasConversion(
                    descripcion => descripcion.HasValue ? descripcion.Value._Value : null,
                    value => string.IsNullOrEmpty(value) ? null : new Descripcion(value));

            builder.Property(e => e.Frecuencia)
        .HasColumnName("frecuencia")
       .HasColumnType("varchar(100)")
.IsRequired()
.HasConversion(
        frecuencia => frecuencia.Value,
     value => Frecuencia.CreateFromDatabase(value));

            // ✅ Activo
            builder.Property(e => e.Activo)
            .HasColumnName("activo")
          .IsRequired();

            builder.Property(e => e.HangfireJobId)
                .HasColumnName("hangfire_job_id")
                .HasColumnType("varchar")
                .HasMaxLength(100)
                .IsRequired();

            // IDs como Value Objects
            builder.Property(e => e.CuentaOrigenId)
                .HasColumnName("id_cuenta_origen")
                .IsRequired()
                .HasConversion(
                    cuentaOrigenId => cuentaOrigenId.Value,
                    value => CuentaId.CreateFromDatabase(value));

            builder.Property(e => e.CuentaDestinoId)
                .HasColumnName("id_cuenta_destino")
                .IsRequired()
                .HasConversion(
                    cuentaDestinoId => cuentaDestinoId.Value,
                    value => CuentaId.CreateFromDatabase(value));

            builder.Property(e => e.UsuarioId)
                .HasColumnName("id_usuario")
                .IsRequired()
                .HasConversion(
                    usuarioId => usuarioId.Value,
                    value => UsuarioId.CreateFromDatabase(value));

            builder.Property(e => e.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .IsRequired()
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            // Relaciones usando SOLO el nombre de columna
            builder.HasOne(e => e.CuentaOrigen)
                .WithMany()
                .HasForeignKey(e => e.CuentaOrigenId) // <--- CAMBIO: Usa la propiedad tipada
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.CuentaDestino)
                .WithMany()
                .HasForeignKey(e => e.CuentaDestinoId) // <--- CAMBIO
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)   // <--- CAMBIO
                .OnDelete(DeleteBehavior.Cascade);

            // Índices críticos para rendimiento
            builder.HasIndex(e => new { e.UsuarioId, e.FechaEjecucion })
                .HasDatabaseName("idx_TraspasoProgramados_usuario_fecha");
        }
    }
}