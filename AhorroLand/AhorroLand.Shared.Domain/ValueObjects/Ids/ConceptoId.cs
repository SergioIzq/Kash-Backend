using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;

namespace AhorroLand.Shared.Domain.ValueObjects.Ids;

public readonly record struct ConceptoId : IGuidValueObject
{
    public Guid Value { get; init; }

    private ConceptoId(Guid value)
    {
        Value = value;
    }

    public static Result<ConceptoId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<ConceptoId>(Error.Validation("El ID del concepto no puede estar vacío."));
        }

        return Result.Success(new ConceptoId(value));
    }

    public static ConceptoId CreateFromDatabase(Guid value) => new ConceptoId(value);
}