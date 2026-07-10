using SergioIzq.Domain.Kernel.Events;
using Kash.Shared.Domain.ValueObjects;

namespace Kash.Domain.IngresosProgramados.Eventos;

/// <summary>
/// Evento de dominio que se dispara cuando se crea un nuevo IngresoProgramado.
/// Este evento es escuchado por la infraestructura para programar el job en Hangfire.
/// </summary>
public sealed record IngresoProgramadoCreadoEvent(
    Guid IngresoProgramadoId,
    Frecuencia Frecuencia,
    DateTime FechaEjecucion
) : DomainEventBase;
