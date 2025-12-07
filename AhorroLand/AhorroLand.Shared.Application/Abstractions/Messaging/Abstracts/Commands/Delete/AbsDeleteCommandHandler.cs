using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Interfaces;
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
    /// 🔥 NUEVO: Permite sobrescribir comportamiento para disparar eventos de dominio.
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
                // 🔥 NUEVO: Permitir que clases derivadas carguen la entidad real si necesitan eventos
                var entity = await LoadEntityForDeletionAsync(command.Id, cancellationToken);

                if (entity == null)
                {
                    return Result.Failure(Error.NotFound($"Entidad {typeof(TEntity).Name} con ID '{command.Id}' no encontrada para eliminación."));
                }

                // Persistencia: Eliminar, SaveChanges y Cache Invalidation (incluye versionado)
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
        /// 🔥 NUEVO: Método virtual que permite a clases derivadas cargar la entidad real
        /// en lugar de usar un stub. Útil cuando se necesita disparar eventos de dominio.
        /// Por defecto, crea un stub optimizado sin acceso a BD.
        /// </summary>
        protected virtual Task<TEntity?> LoadEntityForDeletionAsync(Guid id, CancellationToken cancellationToken)
        {
            // 2. Creamos el stub sincrónicamente
            var entityStub = CreateEntityStub(id);

            // 3. Devolvemos el objeto envuelto en una Task completada
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

            // 🔥 FIX: Convertir Guid a TId (Value Object) usando CreateFromDatabase
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