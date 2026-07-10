using SergioIzq.Domain.Kernel.Events;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain.Ingresos.Eventos;

/// <summary>
/// Evento de dominio que se dispara cuando se actualiza un Ingreso.
/// Permite revertir y reaplicar el cambio en el saldo de las cuentas.
/// </summary>
public sealed record IngresoActualizadoEvent(
    IngresoId IngresoId,
    CuentaId CuentaIdAnterior,
    Cantidad ImporteAnterior,
    CuentaId CuentaIdNueva,
    Cantidad ImporteNuevo
) : DomainEventBase;
