using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.ValueObjects;

namespace AhorroLand.Domain;

public sealed class Traspaso : AbsEntity, IDomainEvent
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> GetDomainEvents() => _domainEvents.ToList();

    public void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    private Traspaso(
        Guid id,
        CuentaId cuentaOrigen,
        CuentaId cuentaDestino,
        Cantidad importe,
        FechaRegistro fecha,
        UsuarioId usuarioId,
        Descripcion? descripcion) : base(id)
    {
        CuentaOrigen = cuentaOrigen;
        CuentaDestino = cuentaDestino;
        Importe = importe;
        Fecha = fecha;
        UsuarioId = usuarioId;
        Descripcion = descripcion;
    }


    public CuentaId CuentaOrigen { get; }
    public CuentaId CuentaDestino { get; }

    public Cantidad Importe { get; }
    public FechaRegistro Fecha { get; }
    public UsuarioId UsuarioId { get; }

    public Descripcion? Descripcion { get; }
    public static Traspaso Create(
        CuentaId cuentaOrigen,
        CuentaId cuentaDestino,
        Cantidad importe,
        FechaRegistro fecha,
        UsuarioId usuarioId,
        Descripcion? descripcion)
    {
        // ⭐ Única validación de dominio intrínseca del Traspaso:
        if (cuentaOrigen.Equals(cuentaDestino))
        {
            throw new InvalidOperationException("La cuenta de origen y destino deben ser diferentes.");
        }

        var traspaso = new Traspaso(
            Guid.NewGuid(),
            cuentaOrigen,
            cuentaDestino,
            importe,
            fecha,
            usuarioId,
            descripcion);

        return traspaso;
    }
}