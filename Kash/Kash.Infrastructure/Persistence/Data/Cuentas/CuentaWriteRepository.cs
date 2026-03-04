using Kash.Domain;
using Kash.Domain.Errors;
using Kash.Infrastructure.Persistence.Command;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.ValueObjects.Ids;
using Microsoft.EntityFrameworkCore;

namespace Kash.Infrastructure.Persistence.Data.Cuentas
{
    public class CuentaWriteRepository : AbsWriteRepository<Cuenta, CuentaId>, ICuentaWriteRepository
    {
        private readonly ICuentaReadRepository _readRepository;

        public CuentaWriteRepository(
            KashDbContext context,
            ICuentaReadRepository readRepository) : base(context)
        {
            _readRepository = readRepository;
        }

        public async Task<Result<Cuenta>> FindOrCreateAsync(Cuenta entity, CancellationToken cancellationToken = default)
        {
            var nombreBuscado = entity.Nombre.Value;

            var usuarioIdObj = entity.UsuarioId;

            var cuentasDelUsuario = await _context.Set<Cuenta>()
                .Where(fp => fp.UsuarioId == usuarioIdObj)
                .ToListAsync(cancellationToken);

            var existingCuenta = cuentasDelUsuario
                .FirstOrDefault(fp => fp.Nombre.Value.Equals(nombreBuscado, StringComparison.OrdinalIgnoreCase));

            if (existingCuenta != null)
            {
                return Result.Success(existingCuenta);
            }

            await base.CreateAsync(entity, cancellationToken);

            return Result.Success(entity);
        }

        public async Task<Result> CreateAsyncWithValidation(Cuenta entity, CancellationToken cancellationToken = default)
        {
            // 1. Validar duplicados
            var exists = await _readRepository.ExistsWithSameNameAsync(
                entity.Nombre,
                entity.UsuarioId,
                cancellationToken);

            if (exists)
            {
                return Result.Failure(CuentaErrors.NombreDuplicado(entity.Nombre.Value));
            }

            // 2. Agregar al contexto
            await base.CreateAsync(entity, cancellationToken);

            return Result.Success();
        }

        public async Task<Result> UpdateAsync(Cuenta entity, CancellationToken cancellationToken = default)
        {
            // 1. Validar duplicados (excepto la propia entidad)
            var exists = await _readRepository.ExistsWithSameNameExceptAsync(
                entity.Nombre,
                entity.UsuarioId,
                entity.Id.Value,
                cancellationToken);

            if (exists)
            {
                return Result.Failure(CuentaErrors.NombreDuplicado(entity.Nombre.Value));
            }

            // 2. Marcar como modificado
            base.Update(entity);

            return Result.Success();
        }
    }
}
