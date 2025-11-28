using AhorroLand.Infrastructure.Persistence.Command;
using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

public abstract class AbsWriteRepository<T, TId> : IWriteRepository<T, TId> 
    where T : AbsEntity<TId>
    where TId : IGuidValueObject
{
    protected readonly AhorroLandDbContext _context;

    public AbsWriteRepository(AhorroLandDbContext context)
    {
        _context = context;

        _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    /// <summary>
    /// Obtiene una entidad por ID con tracking habilitado para Commands.
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Buscar la entidad con tracking habilitado para que los cambios se detecten
        return await _context.Set<T>()
            .AsTracking()
            .FirstOrDefaultAsync(e => e.Id.Value == id, cancellationToken);
    }

    public virtual void Add(T entity)
    {
        _context.Set<T>().Add(entity);
    }

    public virtual async Task CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Actualiza una entidad verificando primero que existe en la base de datos.
    /// </summary>
    public virtual void Update(T entity)
    {
        _context.Set<T>().Update(entity);
    }

    /// <summary>
    /// Actualiza una entidad de forma asíncrona verificando primero que existe en la base de datos.
    /// </summary>
    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        // Verificar si la entidad existe en la base de datos
        var exists = await _context.Set<T>().AnyAsync(e => e.Id.Value == entity.Id.Value, cancellationToken);

        if (!exists)
        {
            throw new InvalidOperationException(
                $"No se puede actualizar la entidad {typeof(T).Name} con Id '{entity.Id.Value}' porque no existe en la base de datos.");
        }

        _context.Set<T>().Update(entity);
    }

    public virtual void Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
    }
}