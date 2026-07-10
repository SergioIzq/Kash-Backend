using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Application.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.ReglasCategorizacion.Commands;

public sealed class UpdateReglaCategorizacionCommandHandler
    : AbsUpdateCommandHandler<ReglaCategorizacion, ReglaCategorizacionId, ReglaCategorizacionDto, UpdateReglaCategorizacionCommand>
{
    public UpdateReglaCategorizacionCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<ReglaCategorizacion, ReglaCategorizacionId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }

    /// <summary>
    /// HOOK: Aplica los cambios del comando a la entidad.
    /// </summary>
    protected override void ApplyChanges(
        ReglaCategorizacion entity,
        UpdateReglaCategorizacionCommand command,
        Dictionary<string, object>? dependencies = null)
    {
        var result = entity.Update(
            command.Patron,
            command.Tipo,
            command.CategoriaNombre,
            command.ConceptoNombre,
            command.ProveedorNombre,
            command.FormaPagoNombre,
            command.Prioridad,
            command.Activo);

        if (result.IsFailure)
        {
            throw new ArgumentException(result.Error.Message);
        }
    }
}
