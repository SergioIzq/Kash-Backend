using Kash.Shared.Domain.Abstractions;
using Kash.Shared.Domain.Abstractions.Errors;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kash.Domain;

[Table("usuarios")]
public sealed class Usuario : AbsEntity<UsuarioId>
{
    // Constructor privado para EF Core - usa el constructor directo sin validación
    private Usuario() : base(UsuarioId.Create(Guid.NewGuid()).Value)
    {

    }

    private Usuario(
        UsuarioId id,
        Email correo,
        PasswordHash contrasenaHash,
        ConfirmationToken? tokenConfirmacion,
        bool activo,
        ConfirmationToken? tokenRecuperacion = null,
        DateTime? tokenRecuperacionExpiracion = null,
        Nombre? nombre = null,
        Apellido? apellidos = null
        ) : base(id)
    {
        Correo = correo;
        ContrasenaHash = contrasenaHash;
        TokenConfirmacion = tokenConfirmacion;
        Activo = activo;
        TokenRecuperacion = tokenRecuperacion;
        TokenRecuperacionExpiracion = tokenRecuperacionExpiracion;
        Nombre = nombre;
        Apellidos = apellidos;
    }

    public Email Correo { get; private set; }
    public Nombre? Nombre { get; private set; }
    public Apellido? Apellidos { get; private set; }
    public PasswordHash ContrasenaHash { get; private set; }
    public ConfirmationToken? TokenConfirmacion { get; private set; }
    public ConfirmationToken? TokenRecuperacion { get; private set; }
    public DateTime? TokenRecuperacionExpiracion { get; private set; }
    public bool Activo { get; private set; }
    public AvatarUrl? Avatar { get; private set; }


    /// <summary>
    /// Crea un nuevo Usuario. El HASHING DEBE OCURRIR FUERA del Dominio.
    /// </summary>
    public static Usuario Create(
        Email correo,
        Nombre nombre,
        Apellido? apellidos,
        PasswordHash contrasenaHash)
    {
        // 1. Generar elementos iniciales de seguridad
        var tokenVO = ConfirmationToken.GenerateNew();

        // 2. Crear la entidad - usa el constructor directo en lugar de Create
        var usuario = new Usuario(
            UsuarioId.Create(Guid.NewGuid()).Value,
            correo,
            contrasenaHash,
            tokenVO,
            activo: false,
            null,
            null,
            nombre,
            apellidos
        );

        return usuario;
    }

    public void Update(
    Nombre nombre,
    Apellido apellidos)
    {
        Nombre = nombre;
        Apellidos = apellidos;
    }

    public void UpdateAvatar(
        AvatarUrl avatar)
    {
        Avatar = avatar;
    }


    /// <summary>
    /// Activa la cuenta del usuario si el token coincide.
    /// </summary>
    public Result Confirmar(string tokenSuministrado)
    {
        if (Activo) return Result.Success();

        if (TokenConfirmacion is null || !TokenConfirmacion.Value.Equals(tokenSuministrado))
        {
            return Result.Failure(AuthErrors.InvalidConfirmationToken);
        }

        Activo = true;
        TokenConfirmacion = null;

        return Result.Success();
    }

    public void SetTokenConfirmacion(ConfirmationToken token)
    {
        TokenConfirmacion = token;
    }

    /// <summary>
    /// Genera un token para restablecer la contraseña y establece una expiración de 1 hora.
    /// Este método debe llamarse desde el ForgotPasswordCommandHandler.
    /// </summary>
    public void GenerarTokenRecuperacion()
    {
        // Generamos un nuevo token usando la lógica existente en tu Value Object
        TokenRecuperacion = ConfirmationToken.GenerateNew();

        // Establecemos que el token expira en 1 hora (UTC para consistencia)
        TokenRecuperacionExpiracion = DateTime.UtcNow.AddHours(1);
    }

    /// <summary>
    /// Intenta cambiar la contraseña validando el token y su expiración.
    /// Este método debe llamarse desde el ResetPasswordCommandHandler.
    /// </summary>
    public Result RestablecerContrasena(string tokenSuministrado, PasswordHash nuevaContrasena)
    {
        // 1. Validar que exista un token activo en la entidad
        if (TokenRecuperacion is null || TokenRecuperacionExpiracion is null)
        {
            return Result.Failure(AuthErrors.InvalidResetToken);
        }

        // 2. Validar que el token coincida
        if (!TokenRecuperacion.Value.Equals(tokenSuministrado))
        {
            return Result.Failure(AuthErrors.InvalidResetToken);
        }

        // 3. Validar expiración (Usar UTC)
        if (DateTime.UtcNow > TokenRecuperacionExpiracion)
        {
            // Limpiamos el token expirado por seguridad
            TokenRecuperacion = null;
            TokenRecuperacionExpiracion = null;
            return Result.Failure(AuthErrors.InvalidResetToken);
        }

        // 4. Éxito: Actualizar contraseña y limpiar tokens
        ContrasenaHash = nuevaContrasena;

        // Es buena práctica activar al usuario si recupera contraseña (por si acaso no confirmó email antes)
        if (!Activo) Activo = true;

        TokenRecuperacion = null;
        TokenRecuperacionExpiracion = null;

        return Result.Success();
    }
}