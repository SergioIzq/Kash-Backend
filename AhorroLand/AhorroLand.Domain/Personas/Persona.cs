using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using System.ComponentModel.DataAnnotations.Schema;

namespace AhorroLand.Domain;

[Table("personas")]
public sealed class Persona : AbsEntity<PersonaId>
{
    // Constructor privado sin parámetros para EF Core
    private Persona() : base(PersonaId.Create(Guid.Empty).Value)
    {
    }

    private Persona(PersonaId id, Nombre nombre, UsuarioId usuarioId) : base(id)
    {
        Nombre = nombre;
        UsuarioId = usuarioId;
    }

    public Nombre Nombre { get; private set; }
    public UsuarioId UsuarioId { get; private set; }

    public static Persona Create(Guid id, Nombre nombre, UsuarioId usuarioId)
    {
        var persona = new Persona(PersonaId.Create(id).Value, nombre, usuarioId);

        return persona;
    }

    public void Update(Nombre nombre) => Nombre = nombre;
}
