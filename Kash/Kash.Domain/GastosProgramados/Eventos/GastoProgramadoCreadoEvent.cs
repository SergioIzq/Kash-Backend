using SergioIzq.Domain.Kernel.Events;
using Kash.Shared.Domain.ValueObjects;

namespace Kash.Domain.GastosProgramados.Eventos;

/// <summary>
/// Evento de dominio que se dispara cuando se crea un nuevo GastoProgramado.
/// Este evento es escuchado por la infraestructura para programar el job en Hangfire.
/// </summary>
public sealed record GastoProgramadoCreadoEvent(
    Guid GastoProgramadoId,
    Frecuencia Frecuencia,
    DateTime FechaEjecucion
) : DomainEventBase;
