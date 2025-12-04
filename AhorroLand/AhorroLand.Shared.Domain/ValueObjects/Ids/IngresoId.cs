using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;

namespace AhorroLand.Shared.Domain.ValueObjects.Ids;

public readonly record struct IngresoId : IGuidValueObject
{
    public Guid Value { get; init; }

    [Obsolete("No usar directamente. Utiliza IngresoId.Create() para validación o IngresoId.CreateFromDatabase() desde infraestructura.", error: true)]
    public IngresoId()
    {
        Value = Guid.Empty;
    }

    private IngresoId(Guid value)
    {
        Value = value;
    }

    public static Result<IngresoId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<IngresoId>(Error.Validation("El ID del ingreso no puede estar vacío."));
        }

        return Result.Success(new IngresoId(value));
    }

    public static IngresoId CreateFromDatabase(Guid value) => new IngresoId(value);
}