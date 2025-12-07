using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.Categorias.Commands;

/// <summary>
/// Maneja la creación de una nueva entidad Categoria.
/// </summary>
public sealed class UpdateCategoriaCommandHandler
    : AbsUpdateCommandHandler<Categoria, CategoriaId, CategoriaDto, UpdateCategoriaCommand>
{
    public UpdateCategoriaCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Categoria, CategoriaId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext
        )
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }

    protected override void ApplyChanges(Categoria entity, UpdateCategoriaCommand command)
    {
        // 1. Crear el Value Object 'Nombre' a partir del string del comando.
        // Esto automáticamente ejecuta las reglas de validación del nombre.
        var nuevoNombreVO = Nombre.Create(command.Nombre).Value;
        var nuevADescVO = new Descripcion(command.Descripcion ?? string.Empty);

        // 2. Ejecutar el método de dominio para actualizar la entidad.
        // **La entidad (Categoria) es responsable de su propia actualización.**
        entity.Update(
            nuevoNombreVO,
            nuevADescVO
        );
    }
}
