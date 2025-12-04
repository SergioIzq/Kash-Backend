using System.Text.RegularExpressions;
using AhorroLand.Shared.Domain.Abstractions.Results;

namespace AhorroLand.Shared.Domain.ValueObjects;

public readonly record struct Email
{
    private static readonly Regex EmailRegex =
        new(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    [Obsolete("No usar directamente. Utiliza Email.Create() para validación o Email.CreateFromDatabase() desde infraestructura.", error: true)]
    public Email()
    {
        Value = string.Empty;
    }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string value)
    {
        // 🔑 Regla de Negocio: No puede ser nulo o vacío
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Email>(Error.Validation("El correo electrónico no puede estar vacío."));
        }

        // 🔑 Regla de Negocio: Validar formato del email
        if (!EmailRegex.IsMatch(value))
        {
            return Result.Failure<Email>(Error.Validation($"La dirección de correo '{value}' no tiene un formato válido."));
        }

        // 🔑 Regla de Negocio: Normalizar el correo a minúsculas
        return Result.Success(new Email(value.ToLowerInvariant()));
    }

    public static Email CreateFromDatabase(string value) => new Email(value.ToLowerInvariant());
}