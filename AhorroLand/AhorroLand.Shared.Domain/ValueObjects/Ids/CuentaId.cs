using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;

namespace AhorroLand.Shared.Domain.ValueObjects.Ids;

public readonly record struct CuentaId : IGuidValueObject
{
    public Guid Value { get; init; }

    [Obsolete("No usar directamente. Utiliza CuentaId.Create() para validación o CuentaId.CreateFromDatabase() desde infraestructura.", error: true)]
    public CuentaId()
    {
        Value = Guid.Empty;
    }

    private CuentaId(Guid value)
    {
        Value = value;
    }

    public static Result<CuentaId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<CuentaId>(Error.Validation("El ID de la cuenta no puede estar vacío."));
        }

        return Result.Success(new CuentaId(value));
    }

    public static CuentaId CreateFromDatabase(Guid value) => new CuentaId(value);
}