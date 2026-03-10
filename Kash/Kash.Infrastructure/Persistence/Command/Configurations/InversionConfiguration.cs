using Kash.Domain;
using Kash.Shared.Domain.ValueObjects.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kash.Infrastructure.Persistence.Command.Configurations;

public class InversionConfiguration : IEntityTypeConfiguration<Inversion>
{
    public void Configure(EntityTypeBuilder<Inversion> builder)
    {
        builder.ToTable("inversiones");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd()
            .HasConversion(
                id => id.Value,
                value => InversionId.CreateFromDatabase(value));

        builder.Property(e => e.UsuarioId)
            .HasColumnName("id_usuario")
            .IsRequired()
            .HasConversion(
                usuarioId => usuarioId.Value,
                value => UsuarioId.CreateFromDatabase(value));

        builder.Property(e => e.Nombre)
            .HasColumnName("nombre")
            .HasColumnType("varchar(200)")
            .IsRequired();

        builder.Property(e => e.Ticker)
            .HasColumnName("ticker")
            .HasColumnType("varchar(20)")
            .IsRequired()
            .HasDefaultValue("");

        builder.Property(e => e.Tipo)
            .HasColumnName("tipo")
            .HasColumnType("varchar(30)")
            .IsRequired()
            .HasConversion(
                tipo => TipoInversionConverter.ToDb(tipo),
                value => TipoInversionConverter.FromDb(value));

        builder.Property(e => e.Cantidad)
            .HasColumnName("cantidad")
            .HasColumnType("decimal(18,8)")
            .IsRequired();

        builder.Property(e => e.PrecioCompra)
            .HasColumnName("precio_compra")
            .HasColumnType("decimal(18,8)")
            .IsRequired();

        builder.Property(e => e.Moneda)
            .HasColumnName("moneda")
            .HasColumnType("varchar(3)")
            .IsRequired();

        builder.Property(e => e.FechaCompra)
            .HasColumnName("fecha_compra")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.Descripcion)
            .HasColumnName("descripcion")
            .HasColumnType("varchar(500)")
            .IsRequired(false);

        builder.Property(e => e.Plataforma)
            .HasColumnName("plataforma")
            .HasColumnType("varchar(100)")
            .IsRequired(false);

        builder.Property(e => e.CreadoEn)
            .HasColumnName("creado_en")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(e => e.ActualizadoEn)
            .HasColumnName("actualizado_en")
            .HasColumnType("datetime")
            .IsRequired(false);

        // Índice compuesto para queries paginadas por usuario + fecha
        builder.HasIndex(e => new { e.UsuarioId, e.FechaCompra })
            .HasDatabaseName("IX_inversiones_usuario_fecha");
    }
}
