using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Categorias.Commands;

/// <summary>
/// REFACTORIZADO: Handler simplificado usando hooks de la clase base.
/// Reducido de ~70 líneas a ~30 líneas (60% menos código).
/// </summary>
public sealed class CreateCategoriaCommandHandler
    : AbsCreateCommandHandler<Categoria, CategoriaId, CreateCategoriaCommand>
{
    private readonly ICategoriaWriteRepository _categoriaWriteRepository;
    
    public CreateCategoriaCommandHandler(
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
    /// HOOK: Crea la entidad de dominio.
    /// Solo necesita implementar la lógica de creación, el resto lo maneja la clase base.
    /// </summary>
    protected override Categoria CreateEntity(CreateCategoriaCommand command, Dictionary<string, object>? dependencies = null)
    {
        var nombreVO = Nombre.Create(command.Nombre).Value;
        var descripcionVO = new Descripcion(command.Descripcion ?? string.Empty);
        var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

        return Categoria.Create(nombreVO, usuarioId, descripcionVO);
    }

    /// <summary>
    /// HOOK: Validación y adición al contexto.
    /// Usa CreateAsyncWithValidation que valida unicidad Y agrega la entidad.
    /// </summary>
    protected override async Task<(Result ValidationResult, bool EntityAdded)> ValidateAndAddToContextAsync(
        Categoria entity,
        CreateCategoriaCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _categoriaWriteRepository.CreateAsyncWithValidation(entity, cancellationToken);
        return (result, result.IsSuccess); // Si es exitoso, la entidad fue agregada
    }
}
