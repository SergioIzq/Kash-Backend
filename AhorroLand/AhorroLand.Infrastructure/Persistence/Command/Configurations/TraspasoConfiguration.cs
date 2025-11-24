using AhorroLand.Domain;
using AhorroLand.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AhorroLand.Infrastructure.Persistence.Command.Configurations.Configurations
{
    public class TraspasoConfiguration : IEntityTypeConfiguration<Traspaso>
    {
        public void Configure(EntityTypeBuilder<Traspaso> builder)
        {
            builder.ToTable("traspaso");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

            builder.Property(e => e.Importe)
    .HasColumnName("importe")
                .HasColumnType("decimal(18,2)")
  .IsRequired()
    .HasConversion(
         importe => importe.Valor,
      value => new Cantidad(value));

            builder.Property(e => e.CuentaOrigenId)
        .HasColumnName("id_cuenta_origen")
           .IsRequired()
        .HasConversion(
                  cuentaId => cuentaId.Value,
                   value => new CuentaId(value));

            builder.Property(e => e.CuentaDestinoId)
                  .HasColumnName("id_cuenta_destino")
               .IsRequired()
            .HasConversion(
                 cuentaId => cuentaId.Value,
            value => new CuentaId(value));

            builder.Property(e => e.UsuarioId)
                     .HasColumnName("id_usuario")
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
        }
    }
}
