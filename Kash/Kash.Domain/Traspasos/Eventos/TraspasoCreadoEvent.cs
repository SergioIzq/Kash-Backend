using SergioIzq.Domain.Kernel.Events;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain.Traspasos.Eventos;

/// <summary>
/// Evento de dominio que se dispara cuando se crea un nuevo Traspaso.
/// Permite actualizar los saldos de las cuentas origen y destino.
/// </summary>
public sealed record TraspasoCreadoEvent(
    TraspasoId TraspasoId,
    CuentaId CuentaOrigenId,
    CuentaId CuentaDestinoId,
    Cantidad Importe
) : DomainEventBase;
