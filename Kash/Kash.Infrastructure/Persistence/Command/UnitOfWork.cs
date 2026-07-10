using SergioIzq.Domain.Kernel.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Kash.Infrastructure.Persistence.Command;

/// <summary>
/// Unit of Work con gestión de transacciones y rollback automático.
/// COMPATIBLE CON MYSQL: Compatible con MySqlRetryingExecutionStrategy.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly KashDbContext _context;
    private readonly IPublisher _publisher;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(KashDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    /// <summary>
    /// Inicia una nueva transacción.
    /// NOTA: Cuando usas transacciones manuales, MySQL Retry Strategy se desactiva automáticamente.
    /// </summary>
    private async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            return; // Ya hay una transacción activa
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Guarda los cambios con gestión automática de transacciones y rollback.
    /// FIX: Compatible con MySQL Retry Strategy.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Obtenemos la estrategia de ejecución actual (la de reintentos de MySQL)
        var strategy = _context.Database.CreateExecutionStrategy();

        // Ejecutamos todo el bloque dentro de la estrategia
        return await strategy.ExecuteAsync(async () =>
        {
            // 1. Iniciar transacción si no existe
            // IMPORTANTE: Al estar dentro de strategy.ExecuteAsync, 
            // EF Core ahora sí nos permite llamar a BeginTransactionAsync manualmente.
            var shouldCommitTransaction = false;

            if (_currentTransaction == null)
            {
                _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                shouldCommitTransaction = true;
            }

            try
            {
                // 2. Disparar eventos de dominio ANTES de guardar
                await DispatchDomainEventsAsync(cancellationToken);

                // 3. Guardar todo junto en la BD
                var result = await _context.SaveChangesAsync(cancellationToken);

                // 4. COMMIT: Solo si creamos la transacción aquí (es decir, es "local")
                if (shouldCommitTransaction && _currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync(cancellationToken);

                    // Limpieza tras el commit exitoso
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }

                return result;
            }
            catch (Exception)
            {
                // ROLLBACK AUTOMÁTICO
                // Nota: En muchas estrategias de reintento, el rollback es implícito si la conexión cae,
                // pero mantenerlo explícito aquí asegura la limpieza en errores lógicos.
                if (_currentTransaction != null && shouldCommitTransaction)
                {
                    await _currentTransaction.RollbackAsync(cancellationToken);
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }

                // Re-lanzar la excepción para que el handler la maneje o EF intente el reintento
                throw;
            }
        });
    }

    /// <summary>
    /// Hace commit de la transacción actual si existe.
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No hay una transacción activa para hacer commit.");
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    /// <summary>
    /// Hace rollback de la transacción actual.
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    /// <summary>
    /// Dispara los eventos de dominio pendientes.
    /// </summary>
    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        // Buscar entidades que tengan eventos pendientes
        var domainEntities = _context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
            .ToList();

        // Si no hay eventos, salir
        if (!domainEntities.Any()) return;

        // Recolectar todos los eventos
        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        // IMPORTANTE: Limpiar los eventos de las entidades antes de publicarlos
        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        // Publicar los eventos a través de MediatR
        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }
    }

    /// <summary>
    /// Libera los recursos.
    /// </summary>
    public void Dispose()
    {
        _currentTransaction?.Dispose();
    }

    /// <summary>
    /// Libera los recursos de forma asíncrona.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
        }
    }
}
