using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using System.ComponentModel.DataAnnotations.Schema;

namespace AhorroLand.Domain;

[Table("clientes")]
public sealed class Cliente : AbsEntity<ClienteId>
{
    public Cliente() : base(ClienteId.Create(Guid.Empty).Value)
    {

    }

    private Cliente(ClienteId id, Nombre nombre, UsuarioId usuarioId) : base(id)
    {
        Nombre = nombre;
        UsuarioId = usuarioId;
    }

    public Nombre Nombre { get; private set; }
    public UsuarioId UsuarioId { get; private set; }

    public static Cliente Create(Nombre nombre, UsuarioId usuarioId)
    {
        var cliente = new Cliente(ClienteId.Create(Guid.NewGuid()).Value, nombre, usuarioId);

        return cliente;
    }

    public void Update(Nombre nombre) => Nombre = nombre;
}
