using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;

namespace Kash.Shared.Domain.ValueObjects.Ids;

public readonly record struct IngresoProgramadoId : IGuidValueObject
{
    public Guid Value { get; init; }

    [Obsolete("No usar directamente. Utiliza IngresoProgramadoId.Create() para validación o IngresoProgramadoId.CreateFromDatabase() desde infraestructura.", error: true)]
    public IngresoProgramadoId()
    {
        Value = Guid.Empty;
    }

    private IngresoProgramadoId(Guid value)
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