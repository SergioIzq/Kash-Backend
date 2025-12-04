using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;

namespace AhorroLand.Shared.Domain.ValueObjects.Ids;

public readonly record struct PersonaId : IGuidValueObject
{
    public Guid Value { get; init; }

    private PersonaId(Guid value)
    {
        Value = value;
    }

    public static Result<PersonaId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<PersonaId>(Error.Validation("El ID de la persona no puede estar vacío."));
        }

        return Result.Success(new PersonaId(value));
    }

    public static PersonaId CreateFromDatabase(Guid value) => new PersonaId(value);
}