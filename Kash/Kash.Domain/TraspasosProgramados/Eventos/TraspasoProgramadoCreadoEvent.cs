using SergioIzq.Domain.Kernel.Events;
using Kash.Shared.Domain.ValueObjects;

namespace Kash.Domain.TraspasosProgramados.Eventos;

/// <summary>
/// Evento de dominio que se dispara cuando se crea un nuevo TraspasoProgramado.
/// Este evento es escuchado por la infraestructura para programar el job en Hangfire.
/// </summary>
public sealed record TraspasoProgramadoCreadoEvent(
    Guid TraspasoProgramadoId,
    Frecuencia Frecuencia,
    DateTime FechaEjecucion
) : DomainEventBase;
