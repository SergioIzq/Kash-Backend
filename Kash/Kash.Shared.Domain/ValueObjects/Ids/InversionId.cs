using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;

namespace Kash.Shared.Domain.ValueObjects.Ids;

public readonly record struct InversionId : IGuidValueObject
{
    public Guid Value { get; init; }

    [Obsolete("No usar directamente. Utiliza InversionId.Create() para validación o InversionId.CreateFromDatabase() desde infraestructura.", error: true)]
    public InversionId()
    {
        Value = Guid.Empty;
    }

    private InversionId(Guid value)
    {
        Value = value;
    }

    public static Result<InversionId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<InversionId>(Error.Validation("El ID de la inversión no puede estar vacío."));
        }

        return Result.Success(new InversionId(value));
    }

    public static InversionId CreateFromDatabase(Guid value) => new(value);
}
