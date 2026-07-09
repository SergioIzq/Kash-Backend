using Kash.Infrastructure.Persistence.Command;
using Kash.Shared.Domain.Abstractions;
using Kash.Shared.Domain.Interfaces;
using Kash.Shared.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

public abstract class AbsWriteRepository<T, TId> : IWriteRepository<T, TId>
    where T : AbsEntity<TId>
    where TId : IGuidValueObject
{
    protected readonly KashDbContext _context;

    // Compilado una única vez por tipo TId cerrado, en lugar de reflexión en cada llamada.
    private static readonly Func<Guid, TId> _idFactory = BuildIdFactory();

    private static Func<Guid, TId> BuildIdFactory()
    {
        var method = typeof(TId).GetMethod(
            "CreateFromDatabase",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: [typeof(Guid)],
            modifiers: null)
            ?? throw new InvalidOperationException(
                $"{typeof(TId).Name} debe exponer un método estático 'CreateFromDatabase(Guid)' para usarse como TId en {nameof(AbsWriteRepository<T, TId>)}.");

        var parameter = Expression.Parameter(typeof(Guid), "id");
        var call = Expression.Call(method, parameter);
        return Expression.Lambda<Func<Guid, TId>>(call, parameter).Compile();
    }

    public AbsWriteRepository(KashDbContext context)
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

        var idValueObject = _idFactory(id);

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