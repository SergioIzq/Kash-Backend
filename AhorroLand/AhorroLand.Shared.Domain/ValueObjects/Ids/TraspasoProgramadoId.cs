using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;

namespace AhorroLand.Shared.Domain.ValueObjects.Ids;

public readonly record struct TraspasoProgramadoId : IGuidValueObject
{
    public Guid Value { get; init; }

    public TraspasoProgramadoId(Guid value)
    {
        Value = value;
    }

    public static Result<TraspasoProgramadoId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<TraspasoProgramadoId>(Error.Validation("El ID del traspaso programado no puede estar vacío."));
        }

        return Result.Success(new TraspasoProgramadoId(value));
    }

    public static TraspasoProgramadoId CreateFromDatabase(Guid value) => new TraspasoProgramadoId(value);
}