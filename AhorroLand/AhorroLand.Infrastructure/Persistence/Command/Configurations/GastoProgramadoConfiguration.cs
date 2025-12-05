using AhorroLand.Domain;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AhorroLand.Infrastructure.Persistence.Command.Configurations.Configurations
{
    public class GastoProgramadoConfiguration : IEntityTypeConfiguration<GastoProgramado>
    {
        public void Configure(EntityTypeBuilder<GastoProgramado> builder)
        {
            builder.ToTable("gastos_programados");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd().HasConversion(
                id => id.Value,
                value => GastoProgramadoId.CreateFromDatabase(value)
            ); ;

            // ✅ Configurar conversión de Value Object Cantidad
            builder.Property(e => e.Importe)
      .HasColumnName("importe")
      .HasColumnType("decimal(18,2)")
       .IsRequired()
     .HasConversion(
          importe => importe.Valor,
          value => Cantidad.CreateFromDatabase(value));

            // ✅ Configurar conversión de Value Objects de IDs
            builder.Property(e => e.CuentaId)
        .HasColumnName("id_cuenta")
   .IsRequired()
         .HasConversion(
        cuentaId => cuentaId.Value,
           value => CuentaId.CreateFromDatabase(value));

            // ✅ Estos NO son nullable en la entidad, son structs requeridos
            builder.Property(e => e.ProveedorId)
                  .HasColumnName("id_proveedor")
           .IsRequired()
             .HasConversion(
          proveedorId => proveedorId.Value,
                  value =>  ProveedorId.CreateFromDatabase(value));

            builder.Property(e => e.PersonaId)
               .HasColumnName("id_persona")
                        .IsRequired()
                   .HasConversion(
               personaId => personaId.Value,
                  value => PersonaId.CreateFromDatabase(value));

            builder.Property(e => e.FormaPagoId)
                .HasColumnName("id_forma_pago")
       .IsRequired()
        .HasConversion(
           formaPagoId => formaPagoId.Value,
        value => FormaPagoId.CreateFromDatabase(value));

            builder.Property(e => e.UsuarioId)
       .HasColumnName("id_usuario")
         .IsRequired()
         .HasConversion(
         usuarioId => usuarioId.Value,
       value => UsuarioId.CreateFromDatabase(value));

            builder.Property(e => e.ConceptoId)
             .HasColumnName("id_concepto")
          .IsRequired()
                      .HasConversion(
                conceptoId => conceptoId.Value,
                 value => ConceptoId.CreateFromDatabase(value));

            // ✅ FIX CRÍTICO: Configurar Frecuencia como Value Object
            builder.Property(e => e.Frecuencia)
                    .HasColumnName("frecuencia")
                   .HasColumnType("varchar(100)")
            .IsRequired()
         .HasConversion(
                    frecuencia => frecuencia.Value,
                 value => Frecuencia.CreateFromDatabase(value));

            // ✅ Configurar Descripcion nullable
            builder.Property(e => e.Descripcion)
    .HasColumnName("descripcion")
     .HasColumnType("varchar(200)")
            .IsRequired(false)
  .HasConversion(
         descripcion => descripcion.HasValue ? descripcion.Value._Value : null,
       value => string.IsNullOrEmpty(value) ? null : new Descripcion(value));

            // ✅ FechaEjecucion
            builder.Property(e => e.FechaEjecucion)
          .HasColumnName("proximo_pago")
               .IsRequired();

            // ✅ Activo
            builder.Property(e => e.Activo)
            .HasColumnName("activo")
          .IsRequired();

            // ✅ HangfireJobId
            builder.Property(e => e.HangfireJobId)
    .HasColumnName("hangfire_job_id")
  .HasColumnType("varchar(100)")
    .IsRequired();

            builder.Property(e => e.FechaCreacion)
     .HasColumnName("fecha_creacion")
                .IsRequired()
          .ValueGeneratedOnAdd()
      .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
