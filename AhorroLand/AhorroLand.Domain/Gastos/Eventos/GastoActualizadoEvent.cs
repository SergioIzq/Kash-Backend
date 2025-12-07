using AhorroLand.Shared.Domain.Events;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Domain.Gastos.Eventos;

/// <summary>
/// Evento de dominio que se dispara cuando se actualiza un Gasto.
/// Permite revertir y reaplicar el cambio en el saldo de las cuentas.
/// </summary>
public sealed record GastoActualizadoEvent(
    GastoId GastoId,
    CuentaId CuentaIdAnterior,
    Cantidad ImporteAnterior,
    CuentaId CuentaIdNueva,
    Cantidad ImporteNuevo
) : DomainEventBase;
