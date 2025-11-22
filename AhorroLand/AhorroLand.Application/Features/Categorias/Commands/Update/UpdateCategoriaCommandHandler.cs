using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;

namespace AhorroLand.Application.Features.Categorias.Commands;

/// <summary>
/// Maneja la creación de una nueva entidad Categoria.
/// </summary>
public sealed class UpdateCategoriaCommandHandler
    : AbsUpdateCommandHandler<Categoria, CategoriaDto, UpdateCategoriaCommand>
{
    public UpdateCategoriaCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Categoria> writeRepository,
        ICacheService cacheService,
        IReadRepositoryWithDto<Categoria, CategoriaDto> readOnlyRepository
        )
        : base(unitOfWork, writeRepository, cacheService)
    {
    }

    protected override void ApplyChanges(Categoria entity, UpdateCategoriaCommand command)
    {
        // 1. Crear el Value Object 'Nombre' a partir del string del comando.
        // Esto automáticamente ejecuta las reglas de validación del nombre.
        var nuevoNombreVO = new Nombre(command.Nombre);
        var nuevADescVO = new Descripcion(command.Descripcion ?? string.Empty);

        // 2. Ejecutar el método de dominio para actualizar la entidad.
        // **La entidad (Categoria) es responsable de su propia actualización.**
        entity.Update(
            nuevoNombreVO,
            nuevADescVO
        );
    }
}
