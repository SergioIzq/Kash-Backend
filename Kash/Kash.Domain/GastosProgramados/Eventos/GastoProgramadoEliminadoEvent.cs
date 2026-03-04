using Kash.Shared.Domain.Events;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain.GastosProgramados.Eventos;

/// <summary>
/// Evento de dominio que se dispara cuando se elimina un GastoProgramado.
/// Permite cancelar el job de Hangfire correspondiente.
/// </summary>
public sealed record GastoProgramadoEliminadoEvent(
    string HangfireJobId,
    GastoProgramadoId GastoProgramadoId,
    CuentaId CuentaId,
    Cantidad Importe
) : DomainEventBase;
