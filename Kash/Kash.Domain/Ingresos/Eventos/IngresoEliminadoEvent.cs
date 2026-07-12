using SergioIzq.Domain.Kernel.Events;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain.Ingresos.Eventos;

/// <summary>
/// Evento de dominio que se dispara cuando se elimina un Ingreso.
/// Permite revertir el efecto en el saldo de la cuenta.
/// </summary>
public sealed record IngresoEliminadoEvent(
    IngresoId IngresoId,
    CuentaId CuentaId,
    Cantidad Importe
) : DomainEventBase;
