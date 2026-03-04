using Kash.Shared.Domain.Events;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain.TraspasosProgramados.Eventos;

/// <summary>
/// Evento de dominio que se dispara cuando se elimina un TraspasoProgramado.
/// Permite cancelar el job de Hangfire correspondiente.
/// </summary>
public sealed record TraspasoProgramadoEliminadoEvent(
    string HangfireJobId,
    TraspasoProgramadoId TraspasoProgramadoId,
    CuentaId CuentaOrigenId,
    CuentaId CuentaDestinoId,
    Cantidad Importe
) : DomainEventBase;
