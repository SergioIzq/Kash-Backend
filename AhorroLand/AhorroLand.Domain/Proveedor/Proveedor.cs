using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using System.ComponentModel.DataAnnotations.Schema;

namespace AhorroLand.Domain;

[Table("proveedores")]
public sealed class Proveedor : AbsEntity<ProveedorId>
{
    // Constructor privado sin parámetros para EF Core
    private Proveedor() : base(ProveedorId.Create(Guid.NewGuid()).Value)
    {
    }

    private Proveedor(ProveedorId id, Nombre nombre, UsuarioId usuarioId) : base(id)
    {
        Nombre = nombre;
        UsuarioId = usuarioId;
    }

    public Nombre Nombre { get; private set; }
    public UsuarioId UsuarioId { get; private set; }


    public static Proveedor Create(Guid id, Nombre nombre, UsuarioId usuarioId)
    {
        var proveedor = new Proveedor(ProveedorId.Create(id).Value, nombre, usuarioId);

        return proveedor;
    }

    public void Update(Nombre nombre) => Nombre = (nombre);
}