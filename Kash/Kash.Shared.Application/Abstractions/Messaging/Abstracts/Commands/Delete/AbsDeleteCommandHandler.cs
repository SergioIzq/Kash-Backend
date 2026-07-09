using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Interfaces;
using Kash.Shared.Domain.Abstractions;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.Interfaces;
using Kash.Shared.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands
{
    /// <summary>
    /// Handler genérico para eliminar entidades.
    /// OPTIMIZADO: Crea un stub de la entidad con solo el ID para DELETE directo.
    /// ROLLBACK AUTOMÁTICO: Si la eliminación falla, se hace rollback de la transacción.
    /// </summary>
    public abstract class DeleteCommandHandler<TEntity, TId, TCommand>
        : AbsCommandHandler<TEntity, TId>, IRequestHandler<TCommand, Result>
        where TEntity : AbsEntity<TId>
        where TCommand : AbsDeleteCommand<TEntity, TId>
        where TId : IGuidValueObject
    {
        public DeleteCommandHandler(
            IUnitOfWork unitOfWork,
            IWriteRepository<TEntity, TId> writeRepository,
            ICacheService cacheService,
            IUserContext userContext)
            : base(unitOfWork, writeRepository, cacheService, userContext)
        {
        }

        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Permitir que clases derivadas carguen la entidad real si necesitan eventos
                var entity = await LoadEntityForDeletionAsync(command.Id, cancellationToken);

                if (entity == null)
                {
                    return Result.Failure(Error.NotFound(
                        $"Entidad {typeof(TEntity).Name} con ID '{command.Id}' no encontrada para eliminación."));
                }

                // NUEVO: Validar si la entidad puede ser eliminada (lógica de dominio)
                var canDeleteResult = CanDelete(entity);

                if (canDeleteResult.IsFailure)
                {
                    // Si no se puede eliminar (ej: tiene registros dependientes), retornar error
                    return canDeleteResult;
                }

                // Persistencia: Eliminar, SaveChanges y Cache Invalidation
                var result = await DeleteAsync(entity, cancellationToken);

                if (result.IsFailure)
                {
                    // El UnitOfWork hará rollback automáticamente
                    return result;
                }

                return Result.Success();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Si la entidad no existe, EF Core lanza DbUpdateConcurrencyException
                return Result.Failure(Error.NotFound(
                    $"Entidad {typeof(TEntity).Name} con ID '{command.Id}' no encontrada para eliminación."));
            }
            catch (DbUpdateException ex)
            {
                // Capturar violaciones de foreign key u otros errores de BD
                // El UnitOfWork hará rollback automáticamente
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return Result.Failure(Error.Conflict(
                    $"No se puede eliminar porque tiene registros relacionados. Detalle: {errorMessage}"));
            }
            catch (Exception ex)
            {
                // Capturar cualquier otro error inesperado
                // El UnitOfWork hará rollback automáticamente
                return Result.Failure(Error.Failure(
                    "Database.Error",
                    "Error de base de datos",
                    ex.Message));
            }
        }

        /// <summary>
        /// NUEVO: Método virtual para validar si la entidad puede ser eliminada.
        /// Override para implementar validaciones de negocio antes de eliminar.
        /// </summary>
        protected virtual Result CanDelete(TEntity entity)
        {
            // Por defecto, siempre se puede eliminar
            return Result.Success();
        }

        /// <summary>
        /// Método virtual que permite a clases derivadas cargar la entidad real
        /// en lugar de usar un stub. Útil cuando se necesita disparar eventos de dominio.
        /// Por defecto, crea un stub optimizado sin acceso a BD.
        /// </summary>
        protected virtual Task<TEntity?> LoadEntityForDeletionAsync(Guid id, CancellationToken cancellationToken)
        {
            var entityStub = CreateEntityStub(id);
            return Task.FromResult<TEntity?>(entityStub);
        }

        /// <summary>
        /// Crea una entidad "stub" solo con el ID para eliminar sin cargar de la BD.
        /// EF Core solo necesita el ID para hacer DELETE.
        /// </summary>
        private TEntity CreateEntityStub(Guid id)
        {
            // Usar Activator para crear instancia sin constructor público
            var entity = (TEntity)Activator.CreateInstance(typeof(TEntity), true)!;

            // Convertir Guid a TId (Value Object) usando CreateFromDatabase
            var idType = typeof(TId);
            var createFromDatabaseMethod = idType.GetMethod("CreateFromDatabase",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            if (createFromDatabaseMethod == null)
            {
                throw new InvalidOperationException(
                    $"El tipo {idType.Name} debe tener un método estático 'CreateFromDatabase(Guid value)'");
            }

            // Invocar CreateFromDatabase(id) para obtener el Value Object
            var valueObjectId = createFromDatabaseMethod.Invoke(null, new object[] { id });

            // Establecer el ID en la entidad
            var idProperty = typeof(TEntity).GetProperty("Id");
            idProperty?.SetValue(entity, valueObjectId);

            return entity;
        }
    }
}
