using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.ValueObjects;

namespace AhorroLand.Domain;

public sealed class FormaPago : AbsEntity
{
    // Constructor privado sin parámetros para EF Core
    private FormaPago() : base(Guid.Empty)
    {
    }

    private FormaPago(Guid id, Nombre nombre, UsuarioId usuarioId) : base(id)
    {
        Nombre = nombre;
        UsuarioId = usuarioId;
    }

    public Nombre Nombre { get; private set; }
    public UsuarioId UsuarioId { get; private set; }

    public static FormaPago Create(Nombre nombre, UsuarioId usuarioId)
    {
        var formaPago = new FormaPago(Guid.NewGuid(), nombre, usuarioId);

        return formaPago;
    }

    public void Update(Nombre nombre) => Nombre = nombre;
}
