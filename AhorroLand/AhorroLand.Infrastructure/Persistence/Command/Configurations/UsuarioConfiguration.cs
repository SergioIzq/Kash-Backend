using AhorroLand.Domain;
using AhorroLand.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AhorroLand.Infrastructure.Persistence.Command.Configurations.Configurations
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("usuario");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

            // ? Configurar Email como Value Object
            builder.Property(e => e.Correo)
                .HasColumnName("username")
            .HasColumnType("varchar")
    .HasMaxLength(100)
         .IsRequired()
     .HasConversion(
           email => email.Value,
         value => new Email(value));

      // ? Configurar PasswordHash como Value Object
      builder.Property(e => e.ContrasenaHash)
                .HasColumnName("contrasena")
   .HasColumnType("longtext")
  .IsRequired()
             .HasConversion(
        password => password.Value,
                value => new PasswordHash(value));

 // ? Configurar TokenConfirmacion como Value Object nullable
        builder.Property(e => e.TokenConfirmacion)
    .HasColumnName("token_confirmacion")
         .HasColumnType("longtext")
      .IsRequired(false)
        .HasConversion(
        token => token.HasValue ? token.Value.Value : null,
       value => string.IsNullOrEmpty(value) ? null : new ConfirmationToken(value));

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

       // ? Índice único en el correo
    builder.HasIndex(e => e.Correo)
         .IsUnique()
    .HasDatabaseName("idx_usuario_correo");
    }
    }
}
