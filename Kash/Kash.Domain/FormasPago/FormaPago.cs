using SergioIzq.Domain.Kernel.Abstractions;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kash.Domain;

[Table("formas_pago")]
public sealed class FormaPago : AbsEntity<FormaPagoId>
{
    // Constructor privado sin parámetros para EF Core
    private FormaPago() : base(FormaPagoId.Create(Guid.NewGuid()).Value)
    {
    }

    private FormaPago(FormaPagoId id, Nombre nombre, UsuarioId usuarioId) : base(id)
    {
        Nombre = nombre;
        UsuarioId = usuarioId;
    }

    public Nombre Nombre { get; private set; }
    public UsuarioId UsuarioId { get; private set; }

    public static FormaPago Create(Nombre nombre, UsuarioId usuarioId)
    {
        var formaPago = new FormaPago(FormaPagoId.Create(Guid.NewGuid()).Value, nombre, usuarioId);

        return formaPago;
    }

    public void Update(Nombre nombre) => Nombre = nombre;
}
