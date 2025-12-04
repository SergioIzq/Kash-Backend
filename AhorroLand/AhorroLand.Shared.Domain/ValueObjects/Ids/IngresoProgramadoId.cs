using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;

namespace AhorroLand.Shared.Domain.ValueObjects.Ids;

public readonly record struct IngresoProgramadoId : IGuidValueObject
{
    public Guid Value { get; init; }

    public IngresoProgramadoId(Guid value)
    {
        Value = value;
    }

    public static Result<IngresoProgramadoId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<IngresoProgramadoId>(Error.Validation("El ID del ingreso programado no puede estar vacío."));
        }

        return Result.Success(new IngresoProgramadoId(value));
    }

    public static IngresoProgramadoId CreateFromDatabase(Guid value) => new IngresoProgramadoId(value);
}