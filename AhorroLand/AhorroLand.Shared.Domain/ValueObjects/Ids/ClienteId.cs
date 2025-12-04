using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;

namespace AhorroLand.Shared.Domain.ValueObjects.Ids;

public readonly record struct ClienteId : IGuidValueObject
{
    public Guid Value { get; init; }

    [Obsolete("No usar directamente. Utiliza ClienteId.Create() para validación o ClienteId.CreateFromDatabase() desde infraestructura.", error: true)]
    public ClienteId()
    {
        Value = Guid.Empty;
    }

    private ClienteId(Guid value)
    {
        Value = value;
    }

    public static Result<ClienteId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<ClienteId>(Error.Validation("El ID del cliente no puede estar vacío."));
        }

        return Result.Success(new ClienteId(value));
    }

    public static ClienteId CreateFromDatabase(Guid value) => new ClienteId(value);
}