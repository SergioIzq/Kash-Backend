using MediatR;

namespace AhorroLand.Shared.Domain.Interfaces;

public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}