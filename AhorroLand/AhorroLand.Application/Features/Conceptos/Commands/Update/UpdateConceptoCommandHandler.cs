using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.Conceptos.Commands;

/// <summary>
/// Maneja la creación de una nueva entidad Concepto.
/// </summary>
public sealed class UpdateConceptoCommandHandler
    : AbsUpdateCommandHandler<Concepto, ConceptoId, ConceptoDto, UpdateConceptoCommand>
{
    public UpdateConceptoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Concepto, ConceptoId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext
        )
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }

    protected override void ApplyChanges(Concepto entity, UpdateConceptoCommand command)
    {
        // 1. Crear el Value Object 'Nombre' a partir del string del comando.
        // Esto automáticamente ejecuta las reglas de validación del nombre.
        var nuevoNombreVO = Nombre.Create(command.Nombre).Value;
        var categoriaIdVO = CategoriaId.Create(command.CategoriaId).Value;

        entity.Update(
            nuevoNombreVO,
            categoriaIdVO
        );
    }
}
