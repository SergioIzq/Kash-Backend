using AhorroLand.Shared.Domain.Interfaces;
using MediatR;

namespace AhorroLand.Infrastructure.Persistence.Command;

public class UnitOfWork : IUnitOfWork
{
    private readonly AhorroLandDbContext _context;
    private readonly IPublisher _publisher; // Inyectamos MediatR

    public UnitOfWork(AhorroLandDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Disparar eventos de dominio ANTES de guardar.
        // Esto permite que los Handlers modifiquen otras entidades (ej: Cuenta)
        // dentro de la misma transacción en memoria.
        await DispatchDomainEventsAsync(cancellationToken);

        // 2. Guardar todo junto en la BBDD (El Gasto nuevo + La Cuenta actualizada)
        return await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        // Buscar entidades que tengan eventos pendientes
        var domainEntities = _context.ChangeTracker
            .Entries<IHasDomainEvents>() // Usamos una interfaz o tu clase base 'AbsEntity'
            .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
            .ToList();

        // Si no hay eventos, salimos
        if (!domainEntities.Any()) return;

        // Recolectar todos los eventos
        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        // IMPORTANTE: Limpiar los eventos de las entidades antes de publicarlos.
        // Esto evita bucles infinitos si SaveChanges se llama de nuevo accidentalmente.
        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        // Publicar los eventos a través de MediatR
        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }
    }
}