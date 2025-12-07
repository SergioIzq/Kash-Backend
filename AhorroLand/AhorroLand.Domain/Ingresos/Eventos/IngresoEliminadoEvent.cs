using AhorroLand.Shared.Domain.Events;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Domain.Ingresos.Eventos;

/// <summary>
/// Evento de dominio que se dispara cuando se elimina un Ingreso.
/// Permite revertir el efecto en el saldo de la cuenta.
/// </summary>
public sealed record IngresoEliminadoEvent(
    IngresoId IngresoId,
    CuentaId CuentaId,
    Cantidad Importe
) : DomainEventBase;
