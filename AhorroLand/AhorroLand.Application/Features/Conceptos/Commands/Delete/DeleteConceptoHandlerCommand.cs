using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.Conceptos.Commands;

/// <summary>
/// Manejador concreto para eliminar un Concepto.
/// Hereda toda la lógica de la clase base genérica.
/// </summary>
public sealed class DeleteConceptoCommandHandler
    : DeleteCommandHandler<Concepto, ConceptoId, DeleteConceptoCommand>
{
    public DeleteConceptoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Concepto, ConceptoId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }
}