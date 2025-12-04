using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;

namespace AhorroLand.Shared.Domain.ValueObjects.Ids;

public readonly record struct FormaPagoId : IGuidValueObject
{
    public Guid Value { get; init; }

    private FormaPagoId(Guid value)
    {
        Value = value;
    }

    public static Result<FormaPagoId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<FormaPagoId>(Error.Validation("El ID de la forma de pago no puede estar vacío."));
        }

        return Result.Success(new FormaPagoId(value));
    }

    public static FormaPagoId CreateFromDatabase(Guid value) => new FormaPagoId(value);
}