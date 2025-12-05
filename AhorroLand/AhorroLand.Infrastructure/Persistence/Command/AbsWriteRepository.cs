using AhorroLand.Infrastructure.Persistence.Command;
using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

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
        if (id == Guid.Empty) return null;

        // 🔥 CORRECCIÓN: Usamos Activator con BindingFlags para encontrar constructores PRIVADOS
        var idValueObject = (TId)Activator.CreateInstance(
            typeof(TId),
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, // Busca privados y públicos
            null,
            [id],
            null)!;

        return await _context.Set<T>()
            .FindAsync([idValueObject], cancellationToken);
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

    public virtual void Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
    }
}