using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Domain.Kernel.Abstractions;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using MediatR;

namespace Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;

/// <summary>
/// Handler genérico MEJORADO para actualizar entidades con patrón Template Method.
/// REFACTORIZADO: Proporciona hooks para diferentes estrategias de actualización.
/// OBJETIVO: Reducir el código duplicado en los handlers concretos.
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

    #region Template Method Pattern - Hooks para personalizar

    /// <summary>
    /// HOOK 1: Validación pre-actualización (opcional).
    /// Override para validar existencia de entidades relacionadas, reglas de negocio, etc.
    /// Se ejecuta ANTES de obtener la entidad de la BD.
    /// Por defecto no hace nada (Result.Success).
    /// </summary>
    protected virtual Task<Result> ValidateBeforeUpdateAsync(TCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success());
    }

    /// <summary>
    /// HOOK 2: Preparación de dependencias (opcional).
    /// Override para buscar/validar entidades relacionadas de forma asíncrona.
    /// Retorna un diccionario con las entidades preparadas o información necesaria.
    /// Por defecto retorna diccionario vacío.
    /// </summary>
    protected virtual Task<Result<Dictionary<string, object>>> PrepareDependenciesAsync(
        TCommand command,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(new Dictionary<string, object>()));
    }

    /// <summary>
    /// HOOK 2.5: Persistir dependencias antes de actualizar la entidad principal (opcional).
    /// Override si las entidades relacionadas deben guardarse ANTES de actualizar la entidad principal.
    /// Esto evita problemas de concurrencia cuando se auto-crean múltiples entidades relacionadas.
    /// Por defecto no hace nada (false).
    /// </summary>
    protected virtual bool ShouldPersistDependenciesFirst()
    {
        return false; // Por defecto NO persiste las dependencias primero
    }

    /// <summary>
    /// HOOK 3: Aplicar cambios a la entidad (REQUERIDO).
    /// Método abstracto que DEBE implementarse en cada handler concreto.
    /// Recibe la entidad cargada de la BD, el command y opcionalmente las dependencias preparadas.
    /// </summary>
    protected abstract void ApplyChanges(TEntity entity, TCommand command, Dictionary<string, object>? dependencies = null);

    /// <summary>
    /// HOOK 4: Validación y actualización con repositorio específico (opcional).
    /// Override para validaciones que requieren la entidad ya modificada (ej: validaciones de unicidad).
    /// Si este método actualiza la entidad en el contexto (usando UpdateAsync con validación),
    /// debe retornar Result con IsSuccess = true y un flag para evitar doble Update.
    /// Por defecto no hace nada (Result.Success, false).
    /// </summary>
    protected virtual async Task<(Result ValidationResult, bool EntityUpdated)> ValidateAndUpdateInContextAsync(
        TEntity entity,
        TCommand command,
        CancellationToken cancellationToken)
    {
        return (Result.Success(), false); // Por defecto no hace nada
    }

    /// <summary>
    /// HOOK 5: Acciones post-actualización (opcional).
    /// Override para ejecutar lógica adicional después de guardar (ej: invalidar cache, enviar eventos).
    /// Por defecto no hace nada.
    /// </summary>
    protected virtual Task OnEntityUpdatedAsync(TEntity entity, Guid entityId, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    #endregion

    #region Template Method - Flujo Principal

    /// <summary>
    /// FLUJO PRINCIPAL: Template Method que orquesta el proceso de actualización.
    /// Este método NO debe ser sobrescrito en la mayoría de casos.
    /// Usa los hooks para personalizar el comportamiento.
    /// </summary>
    public virtual async Task<Result<Guid>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Validación pre-actualización
            var validationResult = await ValidateBeforeUpdateAsync(command, cancellationToken);
            if (validationResult.IsFailure)
            {
                return Result.Failure<Guid>(validationResult.Error);
            }

            // 2. Preparación de dependencias (validar/buscar entidades relacionadas)
            var dependenciesResult = await PrepareDependenciesAsync(command, cancellationToken);
            if (dependenciesResult.IsFailure)
            {
                return Result.Failure<Guid>(dependenciesResult.Error);
            }

            // 2.5 NUEVO: Guardar dependencias PRIMERO si es necesario (evita problemas de concurrencia)
            if (ShouldPersistDependenciesFirst())
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // 3. Obtener la entidad (con tracking para Update)
            var entity = await _writeRepository.GetByIdAsync(command.Id, cancellationToken);
            if (entity is null)
            {
                return Result.Failure<Guid>(Error.NotFound(
                    $"{typeof(TEntity).Name} con ID '{command.Id}' no encontrada."));
            }

            // 4. Aplicar cambios a la entidad
            ApplyChanges(entity, command, dependenciesResult.Value);

            // 5. Validación post-actualización y actualización en contexto
            var validationTuple = await ValidateAndUpdateInContextAsync(entity, command, cancellationToken);
            if (validationTuple.ValidationResult.IsFailure)
            {
                return Result.Failure<Guid>(validationTuple.ValidationResult.Error);
            }

            // 6. Marcar como modificado si no se hizo en el paso anterior
            if (!validationTuple.EntityUpdated)
            {
                _writeRepository.Update(entity);
            }

            // Guardar cambios (solo la entidad principal si ya guardamos las dependencias)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 7. Acciones post-actualización
            await OnEntityUpdatedAsync(entity, entity.Id.Value, cancellationToken);

            return Result.Success(entity.Id.Value);
        }
        catch (ArgumentException ex)
        {
            // Captura de errores de validación de Value Objects
            return Result.Failure<Guid>(Error.Validation(ex.Message));
        }
        catch (Exception ex)
        {
            // Captura de errores inesperados
            return Result.Failure<Guid>(Error.Failure(
                "Error.Unexpected",
                "Error inesperado al actualizar la entidad",
                ex.Message));
        }
    }

    #endregion
}
