using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Application.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Categorias.Commands;

/// <summary>
/// REFACTORIZADO: Handler simplificado usando hooks de la clase base.
/// Reducido de ~80 líneas a ~35 líneas (56% menos código).
/// </summary>
public sealed class UpdateCategoriaCommandHandler
    : AbsUpdateCommandHandler<Categoria, CategoriaId, CategoriaDto, UpdateCategoriaCommand>
{
    private readonly ICategoriaWriteRepository _categoriaWriteRepository;
    
    public UpdateCategoriaCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Categoria, CategoriaId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext,
        ICategoriaWriteRepository categoriaWriteRepository)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _categoriaWriteRepository = categoriaWriteRepository;
    }

    /// <summary>
    /// HOOK: Aplica los cambios del comando a la entidad.
    /// </summary>
    protected override void ApplyChanges(Categoria entity, UpdateCategoriaCommand command, Dictionary<string, object>? dependencies = null)
    {
        var nuevoNombreVO = Nombre.Create(command.Nombre).Value;
        var nuevaDescVO = new Descripcion(command.Descripcion ?? string.Empty);

        entity.Update(nuevoNombreVO, nuevaDescVO);
    }

    /// <summary>
    /// HOOK: Validación y actualización con repositorio específico.
    /// Valida unicidad del nombre y marca la entidad como modificada.
    /// </summary>
    protected override async Task<(Result ValidationResult, bool EntityUpdated)> ValidateAndUpdateInContextAsync(
        Categoria entity,
        UpdateCategoriaCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _categoriaWriteRepository.UpdateAsync(entity, cancellationToken);
        return (result, result.IsSuccess); // Si es exitoso, la entidad fue marcada como modificada
    }
}
