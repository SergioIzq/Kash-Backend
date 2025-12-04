using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;

namespace AhorroLand.Shared.Domain.ValueObjects.Ids;

public readonly record struct GastoProgramadoId : IGuidValueObject
{
    public Guid Value { get; init; }

    public GastoProgramadoId(Guid value)
    {
        Value = value;
    }

    public static Result<GastoProgramadoId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<GastoProgramadoId>(Error.Validation("El ID del gasto programado no puede estar vacío."));
        }

        return Result.Success(new GastoProgramadoId(value));
    }

    public static GastoProgramadoId CreateFromDatabase(Guid value) => new GastoProgramadoId(value);
}