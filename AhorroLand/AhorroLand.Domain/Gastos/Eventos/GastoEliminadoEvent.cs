using AhorroLand.Shared.Domain.Events;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Domain.Gastos.Eventos;

/// <summary>
/// Evento de dominio que se dispara cuando se elimina un Gasto.
/// Permite revertir el efecto en el saldo de la cuenta.
/// </summary>
public sealed record GastoEliminadoEvent(
    GastoId GastoId,
    CuentaId CuentaId,
    Cantidad Importe
) : DomainEventBase;
