using SergioIzq.Domain.Kernel.Abstractions;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;

namespace Kash.Shared.Application.Abstractions.Messaging.Abstracts.Interfaces;

/// <summary>
/// Interfaz base para todos los Command Handlers.
/// Define los contratos para las operaciones de escritura CUD (Create, Update, Delete).
/// </summary>
/// <typeparam name="TEntity">El tipo de entidad que el command handler manipula, debe heredar de AbsEntity.</typeparam>
public interface IAbsCommandHandlerBase<TEntity, TId>
    where TEntity : AbsEntity<TId>
    where TId : IGuidValueObject
{
    /// <summary>
    /// Crea una nueva entidad y persiste los cambios.
    /// </summary>
    Task<Result<Guid>> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca la entidad como modificada y persiste los cambios.
    /// </summary>
    Task<Result<Guid>> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca la entidad para su eliminación y persiste los cambios.
    /// </summary>
    Task<Result> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
}
