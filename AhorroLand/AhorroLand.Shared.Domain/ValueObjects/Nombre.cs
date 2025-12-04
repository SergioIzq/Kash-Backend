using AhorroLand.Shared.Domain.Abstractions.Results;

namespace AhorroLand.Shared.Domain.ValueObjects;

public readonly record struct Nombre
{
    public const int MaxLength = 50;
    public string Value { get; init; }

    private Nombre(string value)
    {
        Value = value;
    }

    public static Result<Nombre> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Nombre>(Error.Validation("El nombre es obligatorio."));
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<Nombre>(Error.Validation($"El nombre no puede exceder los {MaxLength} caracteres."));
        }

        return Result.Success(new Nombre(trimmedValue));
    }

    public static Nombre CreateFromDatabase(string value) => new Nombre(value);
}