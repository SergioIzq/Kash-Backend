using SergioIzq.Domain.Kernel.Abstractions;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kash.Domain;

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


    public static Proveedor Create(Nombre nombre, UsuarioId usuarioId)
    {
        var proveedor = new Proveedor(ProveedorId.Create(Guid.NewGuid()).Value, nombre, usuarioId);

        return proveedor;
    }

    public void Update(Nombre nombre) => Nombre = (nombre);
}