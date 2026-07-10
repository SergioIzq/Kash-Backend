using SergioIzq.Domain.Kernel.Events;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain.IngresosProgramados.Eventos;

/// <summary>
/// Evento de dominio que se dispara cuando se elimina un IngresoProgramado.
/// Permite cancelar el job de Hangfire correspondiente.
/// </summary>
public sealed record IngresoProgramadoEliminadoEvent(
    IngresoProgramadoId IngresoProgramadoId,
    string HangfireJobId,
    CuentaId CuentaId,
    Cantidad Importe
) : DomainEventBase;
