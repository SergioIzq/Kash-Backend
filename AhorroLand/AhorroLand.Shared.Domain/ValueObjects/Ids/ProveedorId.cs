using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;

namespace AhorroLand.Shared.Domain.ValueObjects.Ids;

public readonly record struct ProveedorId : IGuidValueObject
{
    public Guid Value { get; init; }

    private ProveedorId(Guid value)
    {
        Value = value;
    }

    public static Result<ProveedorId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<ProveedorId>(Error.Validation("El ID del proveedor no puede estar vacío."));
        }

        return Result.Success(new ProveedorId(value));
    }

    public static ProveedorId CreateFromDatabase(Guid value) => new ProveedorId(value);
}