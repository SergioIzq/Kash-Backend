using Kash.Domain;
using Kash.Domain.Errors;
using Kash.Infrastructure.Persistence.Command;
using SergioIzq.Infrastructure.Kernel.Persistence;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Infrastructure.Persistence.Data.Conceptos
{
    public class ConceptoWriteRepository : AbsWriteRepository<Concepto, ConceptoId>, IConceptoWriteRepository
    {
        private readonly IConceptoReadRepository _readRepository;

        public ConceptoWriteRepository(
            KashDbContext context,
            IConceptoReadRepository readRepository) : base(context)
        {
            _readRepository = readRepository;
        }

        public async Task<Result> CreateAsyncWithValidation(Concepto entity, CancellationToken cancellationToken = default)
        {
            // 1. Validar duplicados
            var exists = await _readRepository.ExistsWithSameNameAsync(
                entity.Nombre,
                entity.UsuarioId,
                cancellationToken);

            if (exists)
            {
                return Result.Failure(ConceptoErrors.NombreDuplicado(entity.Nombre.Value));
            }

            // 2. Agregar al contexto
            await base.CreateAsync(entity, cancellationToken);

            return Result.Success();
        }

        public async Task<Result> UpdateAsync(Concepto entity, CancellationToken cancellationToken = default)
        {
            // 1. Validar duplicados (excepto la propia entidad)
            var exists = await _readRepository.ExistsWithSameNameExceptAsync(
                entity.Nombre,
                entity.UsuarioId,
                entity.Id.Value,
                cancellationToken);

            if (exists)
            {
                return Result.Failure(ConceptoErrors.NombreDuplicado(entity.Nombre.Value));
            }

            // 2. Marcar como modificado
            base.Update(entity);

            return Result.Success();
        }
    }
}
