namespace Kash.Shared.Domain.Interfaces;

/// <summary>
/// Patrón Unit of Work con gestión de transacciones.
/// ROLLBACK AUTOMÁTICO: Si SaveChangesAsync falla, la transacción se revierte automáticamente.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Guarda los cambios en la base de datos dentro de una transacción.
    /// Si falla, hace rollback automático.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Hace commit de la transacción actual.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Hace rollback de la transacción actual.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
