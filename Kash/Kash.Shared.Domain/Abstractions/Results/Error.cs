namespace Kash.Shared.Domain.Abstractions.Results;

using Kash.Shared.Domain.Abstractions.Enums;

public record Error(string Code, string Name, string Message, ErrorType Type)
{
    // Un error vacío es de tipo Failure por defecto
    public static readonly Error None = new(string.Empty, string.Empty, string.Empty, ErrorType.Failure);

    public static readonly Error NullValue = new("Error.NullValue", "Un valor null fue ingresado", "El valor no puede ser nulo", ErrorType.Validation);

    // --- Métodos Parametrizables (Actualizados) ---

    public static Error NotFound(string detail = "El recurso solicitado no fue encontrado.") =>
        new("Error.NotFound", "Recurso no encontrado", detail, ErrorType.NotFound);

    public static Error Conflict(string detail = "Ya existe un recurso con una o más propiedades únicas.") =>
        new("Error.Conflict", "Conflicto de recurso", detail, ErrorType.Conflict);

    public static Error Validation(string detail = "Uno o más campos de entrada son inválidos.") =>
        new("Error.Validation", "Error de validación", detail, ErrorType.Validation);

    public static Error Unauthorized(string detail = "Credenciales inválidas.") =>
        new("Error.Unauthorized", "No autorizado", detail, ErrorType.Unauthorized);

    public static Error Forbidden(string detail = "No tienes permisos.") =>
        new("Error.Forbidden", "Acceso denegado", detail, ErrorType.Forbidden);

    public static Error Failure(string code, string name, string message) =>
        new(code, name, message, ErrorType.Failure);

    // Constructor auxiliar para cuando creas errores custom sin pasar el enum explícitamente (fallback a Failure)
    public Error(string Code, string Name, string Message) : this(Code, Name, Message, ErrorType.Failure) { }
}
