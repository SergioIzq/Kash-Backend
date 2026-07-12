using SergioIzq.Domain.Kernel.Events;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain.Traspasos.Eventos;

/// <summary>
/// Evento de dominio que se dispara cuando se actualiza un Traspaso.
/// Permite revertir y reaplicar el cambio en los saldos de las cuentas.
/// </summary>
public sealed record TraspasoActualizadoEvent(
    TraspasoId TraspasoId,
    CuentaId CuentaOrigenIdAnterior,
    CuentaId CuentaDestinoIdAnterior,
    Cantidad ImporteAnterior,
    CuentaId CuentaOrigenIdNueva,
    CuentaId CuentaDestinoIdNueva,
    Cantidad ImporteNuevo
) : DomainEventBase;
