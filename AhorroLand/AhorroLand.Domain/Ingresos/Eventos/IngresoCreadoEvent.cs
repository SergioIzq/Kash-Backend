using AhorroLand.Shared.Domain.Events;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Domain.Ingresos.Eventos;

/// <summary>
/// Evento de dominio que se dispara cuando se crea un nuevo Ingreso.
/// Permite actualizar el saldo de la cuenta asociada.
/// </summary>
public sealed record IngresoCreadoEvent(
    IngresoId IngresoId,
    CuentaId CuentaId,
    Cantidad Importe
) : DomainEventBase;
