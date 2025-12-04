using AhorroLand.Shared.Domain.Abstractions.Results;

namespace AhorroLand.Shared.Domain.ValueObjects;

public readonly record struct PasswordHash
{
    public string Value { get; }

    private PasswordHash(string value)
    {
        Value = value;
    }

    public static Result<PasswordHash> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 10)
        {
            return Result.Failure<PasswordHash>(Error.Validation("El hash de la contraseña proporcionado no es válido o está vacío."));
        }

        return Result.Success(new PasswordHash(value));
    }

    public static PasswordHash CreateFromDatabase(string value) => new PasswordHash(value);
}