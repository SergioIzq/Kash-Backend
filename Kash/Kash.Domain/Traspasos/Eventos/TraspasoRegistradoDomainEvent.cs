using SergioIzq.Domain.Kernel.Interfaces;
using Kash.Shared.Domain.ValueObjects;

namespace Kash.Domain.Traspasos.Eventos
{
    public sealed record TraspasoRegistradoDomainEvent(Guid TraspasoId, Guid CuentaOrigenId, Guid CuentaDestinoId, Cantidad Importe) : IDomainEvent;
}
