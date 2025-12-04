using AhorroLand.Shared.Domain.Abstractions.Results;

namespace AhorroLand.Shared.Domain.ValueObjects;

public readonly record struct Apellido
{
    public const int MaxLength = 150;
    public string Value { get; init; }

    private Apellido(string value)
    {
        Value = value;
    }

    public static Result<Apellido> Create(string value)
    {

        var trimmedValue = value.Trim();

        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<Apellido>(Error.Validation($"El apellido no puede exceder los {MaxLength} caracteres."));
        }

        return Result.Success(new Apellido(trimmedValue));
    }

    public static Apellido CreateFromDatabase(string value) => new Apellido(value);
}