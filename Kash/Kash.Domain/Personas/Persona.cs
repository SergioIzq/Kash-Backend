using SergioIzq.Domain.Kernel.Abstractions;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kash.Domain;

[Table("personas")]
public sealed class Persona : AbsEntity<PersonaId>
{
    // Constructor privado sin parámetros para EF Core
    private Persona() : base(PersonaId.Create(Guid.NewGuid()).Value)
    {
    }

    private Persona(PersonaId id, Nombre nombre, UsuarioId usuarioId) : base(id)
    {
        Nombre = nombre;
        UsuarioId = usuarioId;
    }

    public Nombre Nombre { get; private set; }
    public UsuarioId UsuarioId { get; private set; }

    public static Persona Create(Nombre nombre, UsuarioId usuarioId)
    {
        var persona = new Persona(PersonaId.Create(Guid.NewGuid()).Value, nombre, usuarioId);

        return persona;
    }

    public void Update(Nombre nombre) => Nombre = nombre;
}
