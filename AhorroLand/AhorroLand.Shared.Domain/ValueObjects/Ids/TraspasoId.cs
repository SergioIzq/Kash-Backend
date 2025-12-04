using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;

namespace AhorroLand.Shared.Domain.ValueObjects.Ids;

public readonly record struct TraspasoId : IGuidValueObject
{
    public Guid Value { get; init; }

    [Obsolete("No usar directamente. Utiliza TraspasoId.Create() para validación o TraspasoId.CreateFromDatabase() desde infraestructura.", error: true)]
    public TraspasoId()
    {
        Value = Guid.Empty;
    }

    private TraspasoId(Guid value)
    {
        Value = value;
    }

    public static Result<TraspasoId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<TraspasoId>(Error.Validation("El ID del traspaso no puede estar vacío."));
        }

        return Result.Success(new TraspasoId(value));
    }

    public static TraspasoId CreateFromDatabase(Guid value) => new TraspasoId(value);
}