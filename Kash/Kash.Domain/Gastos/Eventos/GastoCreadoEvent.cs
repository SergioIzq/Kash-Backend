using SergioIzq.Domain.Kernel.Events;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain.Gastos.Eventos;

/// <summary>
/// Evento de dominio que se dispara cuando se crea un nuevo Gasto.
/// Permite actualizar el saldo de la cuenta asociada.
/// </summary>
public sealed record GastoCreadoEvent(
    GastoId GastoId,
    CuentaId CuentaId,
    Cantidad Importe
) : DomainEventBase;
