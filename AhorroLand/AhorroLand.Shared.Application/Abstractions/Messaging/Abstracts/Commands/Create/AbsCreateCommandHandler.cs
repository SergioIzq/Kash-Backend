using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using MediatR;

namespace AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;

/// <summary>
/// Handler genérico para crear entidades. 
/// Maneja la creación del objeto de dominio, persistencia y retorno del ID.
/// </summary>
public abstract class AbsCreateCommandHandler<TEntity, TId, TCommand>
    : AbsCommandHandler<TEntity, TId>, IRequestHandler<TCommand, Result<Guid>>
    where TEntity : AbsEntity<TId>
    where TCommand : AbsCreateCommand<TEntity, TId>
    where TId : IGuidValueObject
{
    protected AbsCreateCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<TEntity, TId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }

    /// <summary>
    /// Método abstracto: El hijo debe saber cómo instanciar la entidad (new TEntity(...))
    /// </summary>
    protected abstract TEntity CreateEntity(TCommand command);

    public virtual async Task<Result<Guid>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        TEntity entity;

        // 1. Lógica de Dominio: Instanciar la entidad
        // Capturamos errores de validación de Value Objects (ej. ArgumentException)
        try
        {
            entity = CreateEntity(command);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            return Result.Failure<Guid>(
                Error.Validation(ex.Message) // Devuelve 400 Bad Request
            );
        }

        // 2. Persistencia: Usar el método base (incluye invalidación de caché con versionado)
        // Si hay error de BD (Unique Constraint), el Middleware devuelve 409 Conflict
        var result = await CreateAsync(entity, cancellationToken);

        // 3. Retorno
        // CreateAsync ya devuelve Result<Guid>, así que lo pasamos directamente.
        return result;
    }
}