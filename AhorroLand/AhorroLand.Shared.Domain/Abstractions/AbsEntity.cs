using AhorroLand.Shared.Domain.Interfaces;

namespace AhorroLand.Shared.Domain.Abstractions;

public abstract class AbsEntity<TId> : IHasDomainEvents
    where TId : IGuidValueObject
{
    // 🔥 OPTIMIZACIÓN: Usar List con capacidad inicial para evitar resizes
    private List<IDomainEvent>? _domainEvents;

    protected AbsEntity(TId id)
    {
        Id = id;
        FechaCreacion = DateTime.Now;
    }

    public virtual TId Id { get; init; }
    public virtual DateTime FechaCreacion { get; init; }

    // --- Gestión de Eventos de Dominio ---
    public IReadOnlyCollection<IDomainEvent> DomainEvents
        => _domainEvents?.AsReadOnly() ?? (IReadOnlyCollection<IDomainEvent>)Array.Empty<IDomainEvent>();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        // 🔥 OPTIMIZACIÓN: Lazy initialization - solo crear lista cuando sea necesario
        _domainEvents ??= new List<IDomainEvent>(capacity: 2); // La mayoría de entidades tendrán 1-2 eventos
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        // 🔥 OPTIMIZACIÓN: Reusar la lista en lugar de crear nueva
        _domainEvents?.Clear();
    }
}
