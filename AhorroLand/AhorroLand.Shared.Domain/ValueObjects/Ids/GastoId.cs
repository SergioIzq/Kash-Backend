using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;

namespace AhorroLand.Shared.Domain.ValueObjects.Ids;

public readonly record struct GastoId : IGuidValueObject
{
    public Guid Value { get; init; }

    [Obsolete("No usar directamente. Utiliza GastoId.Create() para validación o GastoId.CreateFromDatabase() desde infraestructura.", error: true)]
    public GastoId()
    {
        Value = Guid.Empty;
    }

    private GastoId(Guid value)
    {
        Value = value;
    }

    public static Result<GastoId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<GastoId>(Error.Validation("El ID del gasto no puede estar vacío."));
        }

        return Result.Success(new GastoId(value));
    }

    public static GastoId CreateFromDatabase(Guid value) => new GastoId(value);
}