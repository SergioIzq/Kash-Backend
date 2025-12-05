using AhorroLand.Domain;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AhorroLand.Infrastructure.Persistence.Command.Configurations.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        // --- ID ---
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("binary(16)") // O binary(16) según tu preferencia en MySQL
            .ValueGeneratedNever()     // ✅ Recomendado: La app genera el GUID
            .HasConversion(
                id => id.Value,
                value => UsuarioId.CreateFromDatabase(value) // Usar CreateFromDatabase para evitar validación
            );

        // --- CORREO (Value Object) ---
        builder.Property(e => e.Correo)
            .HasColumnName("correo")
            .HasColumnType("varchar(100)")
            .IsRequired()
            .HasConversion(
                email => email.Value,
                value => Email.CreateFromDatabase(value)
            );

        // --- CONTRASEÑA ---
        builder.Property(e => e.ContrasenaHash)
            .HasColumnName("contrasena")
            .HasColumnType("longtext") // O varchar(255) si el hash es fijo
            .IsRequired()
            .HasConversion(
                password => password.Value,
                value => PasswordHash.CreateFromDatabase(value)
            );

        // --- TOKENS (Nullables) ---
        builder.Property(e => e.TokenConfirmacion)
            .HasColumnName("token_confirmacion")
            .HasColumnType("varchar(32)")
            .IsRequired(false)
            .HasConversion(
                token => token.HasValue ? token.Value.Value : null,
                value => string.IsNullOrEmpty(value) ? null : ConfirmationToken.CreateFromDatabase(value)
            );

        builder.Property(e => e.TokenRecuperacion)
            .HasColumnName("token_recuperacion")
            .HasColumnType("varchar(32)")
            .IsRequired(false)
            .HasConversion(
                token => token.HasValue ? token.Value.Value : null,
                value => string.IsNullOrEmpty(value) ? null : ConfirmationToken.CreateFromDatabase(value)
            );

        builder.Property(e => e.TokenRecuperacionExpiracion)
            .HasColumnName("token_recuperacion_expiracion")
            .IsRequired(false);

        // --- ACTIVO ---
        builder.Property(e => e.Activo)
            .HasColumnName("activo")
            .IsRequired(); // tinyint(1) por defecto en MySQL para bool

        // --- AVATAR (Nullable Struct) ---
        builder.Property(u => u.Avatar)
            .HasColumnName("avatar")
            .HasColumnType("varchar(500)")
            .IsRequired(false)
            .HasConversion(
                v => v.HasValue ? v.Value.Value : null,
                v => string.IsNullOrEmpty(v) ? (AvatarUrl?)null : AvatarUrl.CreateFromDatabase(v)
            );

        // --- NOMBRE Y APELLIDOS ---
        builder.Property(e => e.Nombre)
            .HasColumnName("nombre")
            .HasColumnType("varchar(100)")
            .IsRequired(false)
            .HasConversion(
                token => token.HasValue ? token.Value.Value : null,
                value => string.IsNullOrEmpty(value) ? null : Nombre.CreateFromDatabase(value)
            );

        builder.Property(e => e.Apellidos) // Asegúrate que tu Entidad se llame 'Apellidos' o 'Apellido'
            .HasColumnName("apellidos")
            .HasColumnType("varchar(100)")
            .IsRequired(false)
            .HasConversion(
                token => token.HasValue ? token.Value.Value : null,
                value => string.IsNullOrEmpty(value) ? null : Apellido.CreateFromDatabase(value)
            );

        // --- AUDITORIA ---
        builder.Property(e => e.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .IsRequired()
            .ValueGeneratedOnAdd() // Dejar que la BD ponga CURRENT_TIMESTAMP
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore); // No actualizar nunca

        // --- ÍNDICES ---
        builder.HasIndex(e => e.Correo)
            .IsUnique()
            .HasDatabaseName("idx_usuario_correo");
    }
}