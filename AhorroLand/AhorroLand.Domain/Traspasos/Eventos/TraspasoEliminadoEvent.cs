using AhorroLand.Shared.Domain.Events;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Domain.Traspasos.Eventos;

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
