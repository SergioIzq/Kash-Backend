using Kash.Domain;
using Kash.Domain.Errors;
using Kash.Infrastructure.Persistence.Command;
using SergioIzq.Infrastructure.Kernel.Persistence;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Infrastructure.Persistence.Data.Clientes
{
    public class ClienteWriteRepository : AbsWriteRepository<Cliente, ClienteId>, IClienteWriteRepository
    {
        private readonly IClienteReadRepository _readRepository;

        public ClienteWriteRepository(
            KashDbContext context,
            IClienteReadRepository readRepository) : base(context)
        {
            _readRepository = readRepository;
        }

        public async Task<Result> CreateAsyncWithValidation(Cliente entity, CancellationToken cancellationToken = default)
        {
            // 1. Validar duplicados
            var exists = await _readRepository.ExistsWithSameNameAsync(
                entity.Nombre,
                entity.UsuarioId,
                cancellationToken);

            if (exists)
            {
                // RETORNAMOS Failure en lugar de lanzar excepción
                return Result.Failure(ClienteErrors.NombreDuplicado(entity.Nombre.Value));
            }

            // 2. Llamar al base (Asumiendo que base.CreateAsync devuelve Task o Task<Result>)
            // Si tu base devuelve void/Task simple, hacemos esto:
            await base.CreateAsync(entity, cancellationToken);

            return Result.Success();
        }

        // IMPORTANTE: Cambiamos 'async void' -> 'async Task<Result>'
        // 'async void' es peligroso porque las excepciones rompen la aplicación sin control.
        public async Task<Result> UpdateAsync(Cliente entity, CancellationToken cancellationToken = default)
        {
            // 1. Validar PRIMERO (antes de llamar al base update)
            var exists = await _readRepository.ExistsWithSameNameExceptAsync(
                entity.Nombre,
                entity.UsuarioId,
                entity.Id.Value); // Asumiendo que entity.Id es ClienteId y .Value es Guid

            if (exists)
            {
                return Result.Failure(ClienteErrors.NombreDuplicado(entity.Nombre.Value));
            }

            // 2. Llamar al base para marcar como modificado
            // Nota: He renombrado el método a UpdateAsync para seguir convenciones
            base.Update(entity);

            // Como Update de EF Core suele ser síncrono (solo cambia estado), 
            // devolvemos Success directamente.
            return Result.Success();
        }
    }
}