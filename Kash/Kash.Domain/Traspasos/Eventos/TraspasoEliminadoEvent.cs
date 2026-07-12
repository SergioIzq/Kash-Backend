using SergioIzq.Domain.Kernel.Events;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain.Traspasos.Eventos;

/// <summary>
/// Evento de dominio que se dispara cuando se elimina un Traspaso.
/// Permite revertir el efecto en los saldos de las cuentas.
/// </summary>
public sealed record TraspasoEliminadoEvent(
    TraspasoId TraspasoId,
CuentaId CuentaOrigenId,
 CuentaId CuentaDestinoId,
    Cantidad Importe
) : DomainEventBase;
