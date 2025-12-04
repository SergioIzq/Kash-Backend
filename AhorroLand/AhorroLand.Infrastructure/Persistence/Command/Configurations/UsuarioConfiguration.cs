using AhorroLand.Domain;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AhorroLand.Infrastructure.Persistence.Command.Configurations.Configurations
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("usuarios");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd().HasConversion(
                id => id.Value,
                value => UsuarioId.Create(value).Value
            ); ;

            builder.Property(e => e.Correo)
                .HasColumnName("correo")
            .HasColumnType("varchar")
    .HasMaxLength(100)
         .IsRequired()
     .HasConversion(
           email => email.Value,
         value => Email.Create(value).Value);

            // ? Configurar PasswordHash como Value Object
            builder.Property(e => e.ContrasenaHash)
                      .HasColumnName("contrasena")
         .HasColumnType("longtext")
        .IsRequired()
                   .HasConversion(
              password => password.Value,
                      value => PasswordHash.Create(value).Value);

            // ? Configurar TokenConfirmacion como Value Object nullable
            builder.Property(e => e.TokenConfirmacion)
        .HasColumnName("token_confirmacion")
             .HasColumnType("varchar")
                 .HasMaxLength(32)
          .IsRequired(false)
            .HasConversion(
            token => token.HasValue ? token.Value.Value : null,
           value => string.IsNullOrEmpty(value) ? null : ConfirmationToken.Create(value).Value);

            builder.Property(e => e.TokenRecuperacion)
                .HasColumnName("token_recuperacion")
                .HasColumnType("varchar")
                .HasMaxLength(32)
                .IsRequired(false)
                .HasConversion(
                token => token.HasValue ? token.Value.Value : null,
                value => string.IsNullOrEmpty(value) ? null : ConfirmationToken.Create(value).Value);

            builder.Property(e => e.TokenRecuperacionExpiracion)
    .HasColumnName("token_recuperacion_expiracion")
.IsRequired(false);

            // ? Configurar Activo
            builder.Property(e => e.Activo)
                   .HasColumnName("activo")
                   .IsRequired();

            // ? Configurar FechaCreacion
            builder.Property(e => e.FechaCreacion)
               .HasColumnName("fecha_creacion")
      .IsRequired()
                  .ValueGeneratedOnAdd()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            builder.Property(e => e.Nombre)
                    .HasColumnName("nombre")
                     .HasColumnType("varchar")
                         .HasMaxLength(100)
                    .IsRequired(false)
                    .HasConversion(
                    token => token.HasValue ? token.Value.Value : null,
                    value => string.IsNullOrEmpty(value) ? null : Nombre.Create(value).Value);

            builder.Property(e => e.Apellidos)
                    .HasColumnName("apellidos")
                    .HasColumnType("varchar")
                    .HasMaxLength(100)
                    .IsRequired(false)
                    .HasConversion(
                    token => token.HasValue ? token.Value.Value : null,
                    value => string.IsNullOrEmpty(value) ? null : Apellido.Create(value).Value);

            // ? Índice único en el correo
            builder.HasIndex(e => e.Correo)
                 .IsUnique()
            .HasDatabaseName("idx_usuario_correo");
        }
    }
}
