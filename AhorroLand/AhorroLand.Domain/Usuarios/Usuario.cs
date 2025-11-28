using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using System.ComponentModel.DataAnnotations.Schema;

namespace AhorroLand.Domain;

[Table("usuarios")]
public sealed class Usuario : AbsEntity<UsuarioId>
{

    private Usuario() : base(new UsuarioId(Guid.Empty))
    {

    }
    private Usuario(
        UsuarioId id,
        Email correo,
        PasswordHash contrasenaHash,
        ConfirmationToken? tokenConfirmacion,
        bool activo) : base(id)
    {
        Correo = correo;
        ContrasenaHash = contrasenaHash;
        TokenConfirmacion = tokenConfirmacion;
        Activo = activo;
    }

    public Email Correo { get; private set; }
    public PasswordHash ContrasenaHash { get; private set; }
    public ConfirmationToken? TokenConfirmacion { get; private set; }
    public bool Activo { get; private set; }

    /// <summary>
    /// Crea un nuevo Usuario. El HASHING DEBE OCURRIR FUERA del Dominio.
    /// </summary>
    public static Usuario Create(
        Email correo,
        PasswordHash contrasenaHash)
    {
        // 1. Generar elementos iniciales de seguridad
        var tokenVO = ConfirmationToken.GenerateNew();

        // 2. Crear la entidad
        var usuario = new Usuario(
            new UsuarioId(Guid.NewGuid()),
            correo,
            contrasenaHash,
            tokenVO,
            activo: false
        );

        return usuario;
    }

    /// <summary>
    /// Activa la cuenta del usuario si el token coincide.
    /// </summary>
    public void Confirmar(string tokenSuministrado)
    {
        if (Activo) return;

        if (TokenConfirmacion is null || !TokenConfirmacion.Value.Equals(tokenSuministrado))
        {
            throw new InvalidOperationException("El token de confirmación no es válido.");
        }

        Activo = true;
        TokenConfirmacion = null;
    }

    public void SetTokenConfirmacion(ConfirmationToken token)
    {
        TokenConfirmacion = token;
    }
}