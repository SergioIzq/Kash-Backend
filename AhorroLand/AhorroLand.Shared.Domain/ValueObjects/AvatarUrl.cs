using AhorroLand.Shared.Domain.Abstractions.Results;

namespace AhorroLand.Shared.Domain.ValueObjects;

public readonly record struct AvatarUrl
{
    public const int MaxLength = 500;
    public string Value { get; init; }

    [Obsolete("No usar directamente. Utiliza AvatarUrl.Create() para validación o AvatarUrl.CreateFromDatabase() desde infraestructura.", error: true)]
    public AvatarUrl()
    {
        Value = string.Empty;
    }

    private AvatarUrl(string value)
    {
        Value = value;
    }

    public static Result<AvatarUrl> Create(string value)
    {
        var trimmedValue = value.Trim();

        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<AvatarUrl>(Error.Validation($"El avatar no puede exceder los {MaxLength} caracteres."));
        }

        return Result.Success(new AvatarUrl(trimmedValue));
    }

    public static AvatarUrl CreateFromDatabase(string value) => new(value);
}