using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;

namespace Kash.Shared.Domain.ValueObjects.Ids;

public readonly record struct ReglaCategorizacionId : IGuidValueObject
{
    public Guid Value { get; init; }

    [Obsolete("No usar directamente. Utiliza ReglaCategorizacionId.Create() para validación o ReglaCategorizacionId.CreateFromDatabase() desde infraestructura.", error: true)]
    public ReglaCategorizacionId()
    {
        Value = Guid.Empty;
    }

    public ReglaCategorizacionId(Guid value)
    {
        Value = value;
    }

    public static Result<ReglaCategorizacionId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<ReglaCategorizacionId>(Error.Validation("El ID de la regla de categorización no puede estar vacío."));
        }

        return Result.Success(new ReglaCategorizacionId(value));
    }

    public static ReglaCategorizacionId CreateFromDatabase(Guid value) => new(value);
}
