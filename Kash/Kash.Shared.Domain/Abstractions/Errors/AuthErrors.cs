using SergioIzq.Domain.Kernel.Abstractions.Enums;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Shared.Domain.Abstractions.Errors;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = new(
        "Auth.InvalidCredentials",
        "Datos de acceso incorrectos",
        "El correo electrónico o la contraseña no coinciden.",
        ErrorType.Unauthorized
    );

    public static readonly Error TokenExpired = new(
        "Auth.TokenExpired",
        "Sesión caducada",
        "Tu sesión ha expirado, por favor inicia sesión nuevamente.",
        ErrorType.Unauthorized
    );

    public static readonly Error AccountLocked = new(
        "Auth.AccountLocked",
        "Cuenta bloqueada",
        "Has excedido el número de intentos. Intenta en 15 minutos.",
        ErrorType.TooManyRequests
    );

    public static readonly Error InvalidResetToken = new(
        "Auth.InvalidResetToken",
        "Enlace no válido",
        "El enlace de recuperación no es válido, contiene errores o ya fue utilizado.",
        ErrorType.Validation
    );

    public static readonly Error InvalidConfirmationToken = new(
        "Auth.InvalidConfirmationToken",
        "Enlace no válido",
        "El enlace de confirmación no es válido, contiene errores o ya fue utilizado.",
        ErrorType.Validation
    );

    public static readonly Error ResetLinkExpired = new(
        "Auth.ResetLinkExpired",
        "Enlace caducado",
        "El tiempo límite para recuperar la contraseña ha expirado. Por favor, solicita un nuevo enlace.",
        ErrorType.Validation
    );

    public static readonly Error AlreadyConfirmed = new(
        "Auth.AlreadyConfirmed",
        "Cuenta ya confirmada",
        "Ya ha confirmado su cuenta, inicie sesión.",
        ErrorType.Validation
    );
}
