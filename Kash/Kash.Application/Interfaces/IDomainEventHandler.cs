using SergioIzq.Domain.Kernel.Interfaces;

namespace Kash.Application.Interfaces
{
    public interface IDomainEventHandler<TEvent> where TEvent : IDomainEvent
    {
        Task Handle(TEvent notification, CancellationToken cancellationToken);
    }
}
