using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Application.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Categorias.Commands;

/// <summary>
/// Manejador concreto para eliminar una Categoría.
/// Hereda toda la lógica de la clase base genérica.
/// </summary>
public sealed class DeleteCategoriaCommandHandler
    : DeleteCommandHandler<Categoria, CategoriaId, DeleteCategoriaCommand>
{
    public DeleteCategoriaCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Categoria, CategoriaId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }
}


