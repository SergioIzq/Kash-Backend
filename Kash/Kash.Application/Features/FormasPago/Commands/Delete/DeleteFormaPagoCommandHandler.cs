using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Application.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.FormasPago.Commands;

/// <summary>
/// Manejador concreto para eliminar una FormaPago.
/// Hereda toda la lógica de la clase base genérica.
/// </summary>
public sealed class DeleteFormaPagoCommandHandler
    : DeleteCommandHandler<FormaPago, FormaPagoId, DeleteFormaPagoCommand>
{
    public DeleteFormaPagoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<FormaPago, FormaPagoId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }
}


