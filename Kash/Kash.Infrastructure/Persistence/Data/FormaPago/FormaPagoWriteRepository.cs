using Kash.Domain;
using Kash.Domain.Errors;
using Kash.Infrastructure.Persistence.Command;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.ValueObjects.Ids;
using Microsoft.EntityFrameworkCore;

namespace Kash.Infrastructure.Persistence.Data.FormasPago
{
    public class FormaPagoWriteRepository : AbsWriteRepository<FormaPago, FormaPagoId>, IFormaPagoWriteRepository
    {
        private readonly IFormaPagoReadRepository _readRepository;

        public FormaPagoWriteRepository(
            KashDbContext context,
            IFormaPagoReadRepository readRepository) : base(context)
        {
            _readRepository = readRepository;
        }

        public async Task<Result<FormaPago>> FindOrCreateAsync(FormaPago entity, CancellationToken cancellationToken = default)
        {
            var nombreBuscado = entity.Nombre.Value;

            var usuarioIdObj = entity.UsuarioId;

            var formasPagoDelUsuario = await _context.Set<FormaPago>()
                .Where(fp => fp.UsuarioId == usuarioIdObj)
                .ToListAsync(cancellationToken);

            var existingFormaPago = formasPagoDelUsuario
                .FirstOrDefault(fp => fp.Nombre.Value.Equals(nombreBuscado, StringComparison.OrdinalIgnoreCase));

            if (existingFormaPago != null)
            {
                return Result.Success(existingFormaPago);
            }

            await base.CreateAsync(entity, cancellationToken);

            return Result.Success(entity);
        }

        public async Task<Result> CreateAsyncWithValidation(FormaPago entity, CancellationToken cancellationToken = default)
        {
            // 1. Validar duplicados
            var exists = await _readRepository.ExistsWithSameNameAsync(
                entity.Nombre,
                entity.UsuarioId,
                cancellationToken);

            if (exists)
            {
                return Result.Failure(FormaPagoErrors.NombreDuplicado(entity.Nombre.Value));
            }

            // 2. Agregar al contexto
            await base.CreateAsync(entity, cancellationToken);

            return Result.Success();
        }

        public async Task<Result> UpdateAsync(FormaPago entity, CancellationToken cancellationToken = default)
        {
            // 1. Validar duplicados (excepto la propia entidad)
            var exists = await _readRepository.ExistsWithSameNameExceptAsync(
                entity.Nombre,
                entity.UsuarioId,
                entity.Id.Value,
                cancellationToken);

            if (exists)
            {
                return Result.Failure(FormaPagoErrors.NombreDuplicado(entity.Nombre.Value));
            }

            // 2. Marcar como modificado
            base.Update(entity);

            return Result.Success();
        }
    }
}
