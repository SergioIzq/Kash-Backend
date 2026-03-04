using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Interfaces;
using Kash.Shared.Domain.Abstractions;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.Interfaces;
using Kash.Shared.Domain.Interfaces.Repositories;
using MediatR;

namespace Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;

/// <summary>
/// Handler genérico MEJORADO para crear entidades con patrón Template Method.
/// 🔥 REFACTORIZADO: Ahora proporciona hooks para diferentes estrategias de creación.
/// 🎯 OBJETIVO: Reducir el código duplicado en los handlers concretos.
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

    #region Template Method Pattern - Hooks para personalizar

    /// <summary>
    /// 🔥 HOOK 1: Validación pre-creación (opcional).
    /// Override para validar existencia de entidades relacionadas, reglas de negocio, etc.
    /// Por defecto no hace nada (Result.Success).
    /// </summary>
    protected virtual Task<Result> ValidateBeforeCreateAsync(TCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success());
    }

    /// <summary>
    /// 🔥 HOOK 2: Preparación de dependencias (opcional).
    /// Override para buscar/crear entidades relacionadas de forma asíncrona.
    /// Retorna un diccionario con las entidades preparadas.
    /// Por defecto retorna diccionario vacío.
    /// </summary>
    protected virtual Task<Result<Dictionary<string, object>>> PrepareDependenciesAsync(
        TCommand command,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(new Dictionary<string, object>()));
    }

    /// <summary>
    /// 🔥 HOOK 2.5: Persistir dependencias antes de crear la entidad principal (opcional).
    /// Override si las entidades relacionadas deben guardarse ANTES de crear la entidad principal.
    /// Esto evita problemas de concurrencia cuando se auto-crean múltiples entidades relacionadas.
    /// Por defecto no hace nada (false).
    /// </summary>
    protected virtual bool ShouldPersistDependenciesFirst()
    {
        return false; // Por defecto NO persiste las dependencias primero
    }

    /// <summary>
    /// 🔥 HOOK 3: Creación de la entidad (REQUERIDO).
    /// Método abstracto que DEBE implementarse en cada handler concreto.
    /// Recibe el command y opcionalmente las dependencias preparadas.
    /// </summary>
    protected abstract TEntity CreateEntity(TCommand command, Dictionary<string, object>? dependencies = null);

    /// <summary>
    /// 🔥 HOOK 4: Validación y adición al contexto (opcional).
    /// Override para validaciones que requieren la entidad ya creada Y agregar al contexto en un solo paso.
    /// Ejemplo: CreateAsyncWithValidation() que valida unicidad y agrega la entidad.
    /// Si se implementa, NO debe llamarse Add() en el flujo principal.
    /// Retorna true si agregó la entidad al contexto, false si no.
    /// </summary>
    protected virtual async Task<(Result ValidationResult, bool EntityAdded)> ValidateAndAddToContextAsync(
        TEntity entity,
        TCommand command,
        CancellationToken cancellationToken)
    {
        return (Result.Success(), false); // Por defecto no hace nada
    }

    /// <summary>
    /// 🔥 HOOK 5: Acciones post-persistencia (opcional).
    /// Override para ejecutar lógica adicional después de guardar (ej: enviar eventos, invalidar cache).
    /// Por defecto no hace nada.
    /// </summary>
    protected virtual Task OnEntityCreatedAsync(TEntity entity, Guid entityId, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    #endregion

    #region Template Method - Flujo Principal

    /// <summary>
    /// 🎯 FLUJO PRINCIPAL: Template Method que orquesta el proceso de creación.
    /// Este método NO debe ser sobrescrito en la mayoría de casos.
    /// Usa los hooks para personalizar el comportamiento.
    /// </summary>
    public virtual async Task<Result<Guid>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // 1. 🔍 Validación pre-creación
            var validationResult = await ValidateBeforeCreateAsync(command, cancellationToken);
            if (validationResult.IsFailure)
            {
                return Result.Failure<Guid>(validationResult.Error);
            }

            // 2. 🛠️ Preparación de dependencias (buscar/crear entidades relacionadas)
            var dependenciesResult = await PrepareDependenciesAsync(command, cancellationToken);
            if (dependenciesResult.IsFailure)
            {
                return Result.Failure<Guid>(dependenciesResult.Error);
            }

            // 2.5 🔥 NUEVO: Guardar dependencias PRIMERO si es necesario (evita problemas de concurrencia)
            if (ShouldPersistDependenciesFirst())
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // 3. 🏗️ Creación de la entidad
            var entity = CreateEntity(command, dependenciesResult.Value);

            // 4. ✅ Validación post-creación y opcionalmente adición al contexto
            var validationTuple = await ValidateAndAddToContextAsync(entity, command, cancellationToken);
            if (validationTuple.ValidationResult.IsFailure)
            {
                return Result.Failure<Guid>(validationTuple.ValidationResult.Error);
            }

            // 5. 💾 Agregar al contexto si no se hizo en el paso anterior
            if (!validationTuple.EntityAdded)
            {
                _writeRepository.Add(entity);
            }

            // Guardar cambios (solo la entidad principal si ya guardamos las dependencias)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 6. 🎉 Acciones post-persistencia
            await OnEntityCreatedAsync(entity, entity.Id.Value, cancellationToken);

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
                "Error inesperado al crear la entidad",
                ex.Message));
        }
    }

    #endregion
}