using SergioIzq.Domain.Kernel.Abstractions.Enums;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Shared.Domain.Abstractions.Errors;

public static class UsuarioErrors
{
    public static readonly Error DuplicateEmail = new(
        "User.DuplicateEmail",
        "Este correo ya está registrado",
        "Ya existe una cuenta asociada a este correo electrónico. Intenta iniciar sesión o recuperar tu contraseña.",
        ErrorType.Conflict
    );

    public static readonly Error DeactivatedAccount = new(
        "User.DeactivatedAccount",
        "Cuenta pendiente de activación",
        "Tu cuenta aún no ha sido verificada. Por favor, revisa tu bandeja de entrada o solicita un nuevo enlace de activación.",
        ErrorType.Validation
    );
}