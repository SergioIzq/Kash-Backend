using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;

namespace AhorroLand.Shared.Domain.ValueObjects.Ids;

public readonly record struct CategoriaId : IGuidValueObject
{
    public Guid Value { get; init; }

    private CategoriaId(Guid value)
    {
        Value = value;
    }

    public static Result<CategoriaId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<CategoriaId>(Error.Validation("El ID de la categoría no puede estar vacío."));
        }

        return Result.Success(new CategoriaId(value));
    }

    public static CategoriaId CreateFromDatabase(Guid value) => new CategoriaId(value);
}