using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands
{
    /// <summary>
    /// Handler genérico para eliminar entidades.
    /// ✅ OPTIMIZADO: Crea un stub de la entidad con solo el ID para DELETE directo.
    /// No carga la entidad completa ni valida existencia (EF Core lanzará DbUpdateConcurrencyException si no existe).
    /// </summary>
    public abstract class DeleteCommandHandler<TEntity, TCommand>
        : AbsCommandHandler<TEntity>, IRequestHandler<TCommand, Result>
      where TEntity : AbsEntity
  where TCommand : AbsDeleteCommand<TEntity>
    {
        public DeleteCommandHandler(
                IUnitOfWork unitOfWork,
                IWriteRepository<TEntity> writeRepository,
            ICacheService cacheService)
           : base(unitOfWork, writeRepository, cacheService)
        {
        }

        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // 1. ✅ Crear una entidad "stub" solo con el ID para eliminar (sin cargar de BD)
                var entity = CreateEntityStub(command.Id);

                // 2. Persistencia: Eliminar, SaveChanges y Cache Invalidation
                var result = await DeleteAsync(entity, cancellationToken);

                return result;
            }
            catch (DbUpdateConcurrencyException)
            {
                // Si la entidad no existe, EF Core lanza DbUpdateConcurrencyException
                return Result.Failure(Error.NotFound($"Entidad {typeof(TEntity).Name} con ID '{command.Id}' no encontrada para eliminación."));
            }
        }

        /// <summary>
        /// Crea una entidad "stub" solo con el ID para eliminar sin cargar de la BD.
        /// EF Core solo necesita el ID para hacer DELETE.
        /// </summary>
        private TEntity CreateEntityStub(Guid id)
        {
            // Usar Activator para crear instancia sin constructor público
            var entity = (TEntity)Activator.CreateInstance(typeof(TEntity), true)!;

            // Establecer el ID usando reflexión
            var idProperty = typeof(TEntity).GetProperty("Id");
            idProperty?.SetValue(entity, id);

            return entity;
        }
    }
}