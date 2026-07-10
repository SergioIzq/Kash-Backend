using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Conceptos.Commands;

/// <summary>
/// REFACTORIZADO: Handler simplificado usando hooks de la clase base.
/// Reducido de ~80 líneas a ~35 líneas (56% menos código).
/// </summary>
public sealed class UpdateConceptoCommandHandler
    : AbsUpdateCommandHandler<Concepto, ConceptoId, ConceptoDto, UpdateConceptoCommand>
{
    private readonly IConceptoWriteRepository _conceptoWriteRepository;

    public UpdateConceptoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Concepto, ConceptoId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext,
        IConceptoWriteRepository conceptoWriteRepository)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _conceptoWriteRepository = conceptoWriteRepository;
    }

    /// <summary>
    /// HOOK: Aplica los cambios del comando a la entidad.
    /// </summary>
    protected override void ApplyChanges(Concepto entity, UpdateConceptoCommand command, Dictionary<string, object>? dependencies = null)
    {
        var nuevoNombreVO = Nombre.Create(command.Nombre).Value;
        var nuevaCategoriaIdVO = CategoriaId.Create(command.CategoriaId).Value;

        entity.Update(nuevoNombreVO, nuevaCategoriaIdVO);
    }

    /// <summary>
    /// HOOK: Validación y actualización con repositorio específico.
    /// Valida unicidad del nombre y marca la entidad como modificada.
    /// </summary>
    protected override async Task<(Result ValidationResult, bool EntityUpdated)> ValidateAndUpdateInContextAsync(
        Concepto entity,
        UpdateConceptoCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _conceptoWriteRepository.UpdateAsync(entity, cancellationToken);
        return (result, result.IsSuccess); // Si es exitoso, la entidad fue marcada como modificada
    }
}
