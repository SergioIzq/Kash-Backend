using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;

namespace AhorroLand.Shared.Domain.ValueObjects.Ids;

public readonly record struct UsuarioId : IGuidValueObject
{
    public Guid Value { get; init; }

    [Obsolete("No usar directamente. Utiliza UsuarioId.Create() para validación o UsuarioId.CreateFromDatabase() desde infraestructura.", error: true)]
    public UsuarioId()
    {
        Value = Guid.Empty;
    }

    // ✅ CONSTRUCTOR (Infraestructura):
    // Debe ser permisivo porque EF Core y los serializadores (JSON) 
    // a veces instancian esto con valores por defecto (Guid.Empty) temporalmente.
    public UsuarioId(Guid value)
    {
        Value = value;
    }

    // ✅ FACTORY METHOD (Dominio):
    // Aquí es donde aplicas las reglas de negocio.
    // Tu código de aplicación SIEMPRE debe usar UsuarioId.Create(...)
    public static Result<UsuarioId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<UsuarioId>(Error.Validation("El ID del usuario no puede estar vacío."));
        }

        return Result.Success(new UsuarioId(value));
    }

    public static UsuarioId CreateFromDatabase(Guid value) => new UsuarioId(value);

    public override string ToString() => Value.ToString("D");
}