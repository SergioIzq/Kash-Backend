using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Application.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Conceptos.Commands;

/// <summary>
/// REFACTORIZADO: Handler simplificado usando hooks de la clase base.
/// Reducido de ~70 líneas a ~30 líneas (60% menos código).
/// </summary>
public sealed class CreateConceptoCommandHandler
    : AbsCreateCommandHandler<Concepto, ConceptoId, CreateConceptoCommand>
{
    private readonly IConceptoWriteRepository _ConceptoWriteRepository;

    public CreateConceptoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Concepto, ConceptoId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext,
        IConceptoWriteRepository ConceptoWriteRepository)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _ConceptoWriteRepository = ConceptoWriteRepository;
    }

    /// <summary>
    /// HOOK: Crea la entidad de dominio.
    /// Solo necesita implementar la lógica de creación, el resto lo maneja la clase base.
    /// </summary>
    protected override Concepto CreateEntity(CreateConceptoCommand command, Dictionary<string, object>? dependencies = null)
    {
        var nombreVO = Nombre.Create(command.Nombre).Value;
        var categoriaId = CategoriaId.Create(command.CategoriaId).Value;
        var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

        return Concepto.Create(nombreVO, categoriaId, usuarioId);
    }

    /// <summary>
    /// HOOK: Validación y adición al contexto.
    /// Usa CreateAsyncWithValidation que valida unicidad Y agrega la entidad.
    /// </summary>
    protected override async Task<(Result ValidationResult, bool EntityAdded)> ValidateAndAddToContextAsync(
        Concepto entity,
        CreateConceptoCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _ConceptoWriteRepository.CreateAsyncWithValidation(entity, cancellationToken);
        return (result, result.IsSuccess); // Si es exitoso, la entidad fue agregada
    }
}
