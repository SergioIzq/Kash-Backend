using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.ValueObjects;

namespace AhorroLand.Domain;

public sealed class Persona : AbsEntity
{
    // Constructor privado sin parámetros para EF Core
    private Persona() : base(Guid.Empty)
    {
    }

    private Persona(Guid id, Nombre nombre, UsuarioId usuarioId) : base(id)
    {
        Nombre = nombre;
        UsuarioId = usuarioId;
    }

    public Nombre Nombre { get; private set; }
    public UsuarioId UsuarioId { get; private set; }

    public static Persona Create(Guid id, Nombre nombre, UsuarioId usuarioId)
    {
        var persona = new Persona(id, nombre, usuarioId);

        return persona;
    }

    public void Update(Nombre nombre) => Nombre = nombre;
}
