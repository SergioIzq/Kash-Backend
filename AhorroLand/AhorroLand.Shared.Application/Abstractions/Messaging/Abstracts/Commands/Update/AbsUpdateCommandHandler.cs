using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using MediatR;

namespace AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;

/// <summary>
/// Handler genérico para actualizar entidades.
/// 🔥 MODIFICADO: Ahora devuelve Result<Guid> en lugar de Result<TDto> para ser consistente con Create.
/// </summary>
public abstract class AbsUpdateCommandHandler<TEntity, TId, TDto, TCommand>
    : AbsCommandHandler<TEntity, TId>, IRequestHandler<TCommand, Result<Guid>>
    where TEntity : AbsEntity<TId>
    where TCommand : AbsUpdateCommand<TEntity, TId, TDto>
    where TId : IGuidValueObject
    where TDto : class
{
    protected AbsUpdateCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<TEntity, TId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }

    // Método abstracto para que el hijo aplique los cambios
    protected abstract void ApplyChanges(TEntity entity, TCommand command);

    public async Task<Result<Guid>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        // 1. Obtener la entidad (Tracking activado para Update)
        var entity = await _writeRepository.GetByIdAsync(command.Id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure<Guid>(Error.NotFound($"{typeof(TEntity).Name} con ID '{command.Id}' no encontrada."));
        }

        // 2. Aplicar lógica de dominio (Value Objects)
        // Aquí capturamos errores de validación de negocio (ej. "Nombre vacío", "Precio negativo")
        try
        {
            ApplyChanges(entity, command);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            // Transformamos la excepción del Value Object en un Result.Failure limpio
            return Result.Failure<Guid>(Error.Validation(ex.Message));
        }

        // 3. Persistencia (incluye invalidación de caché con versionado)
        // Si hay error de BD (ej. Nombre duplicado), UpdateAsync dejará que suba al Middleware Global (que devuelve 409)
        var result = await UpdateAsync(entity, cancellationToken);

        // 4. Retornar solo el ID (consistente con CreateAsync)
        return result;
    }
}