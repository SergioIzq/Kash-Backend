using SergioIzq.Domain.Kernel.Events;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain.TraspasosProgramados.Eventos;

/// <summary>
/// Evento de dominio que se dispara cuando se actualiza un TraspasoProgramado.
/// Permite cancelar el job anterior y reprogramar uno nuevo si cambiel importe o las cuentas.
/// </summary>
public sealed record TraspasoProgramadoActualizadoEvent(
    TraspasoProgramadoId TraspasoProgramadoId,
    CuentaId CuentaOrigenIdAnterior,
    CuentaId CuentaDestinoIdAnterior,
    Cantidad ImporteAnterior,
    CuentaId CuentaOrigenIdNueva,
    CuentaId CuentaDestinoIdNueva,
    Cantidad ImporteNuevo
) : DomainEventBase;
