using SergioIzq.Domain.Kernel.Events;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain.Ingresos.Eventos;

/// <summary>
/// Evento de dominio que se dispara cuando se crea un nuevo Ingreso.
/// Permite actualizar el saldo de la cuenta asociada.
/// </summary>
public sealed record IngresoCreadoEvent(
    IngresoId IngresoId,
    CuentaId CuentaId,
    Cantidad Importe
) : DomainEventBase;
