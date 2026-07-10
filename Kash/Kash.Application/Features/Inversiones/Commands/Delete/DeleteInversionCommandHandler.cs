using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Application.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Inversiones.Commands;

public sealed class DeleteInversionCommandHandler
    : DeleteCommandHandler<Inversion, InversionId, DeleteInversionCommand>
{
    public DeleteInversionCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Inversion, InversionId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }

    /// <summary>
    /// Carga la entidad validando que pertenezca al usuario actual.
    /// Devuelve null si no existe o no pertenece → 404 en el base handler.
    /// </summary>
    protected override async Task<Inversion?> LoadEntityForDeletionAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var entity = await _writeRepository.GetByIdAsync(id, cancellationToken);

        if (entity is null || entity.UsuarioId.Value != _userContext.UserId)
            return null;

        return entity;
    }
}
