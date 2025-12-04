using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.Categorias.Commands;

/// <summary>
/// Maneja la creación de una nueva entidad Categoria.
/// </summary>
public sealed class CreateCategoriaCommandHandler
    : AbsCreateCommandHandler<Categoria, CategoriaId, CreateCategoriaCommand>
{
    public CreateCategoriaCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Categoria, CategoriaId> writeRepository,
        ICacheService cacheService)
        : base(unitOfWork, writeRepository, cacheService)
    {
    }

    /// <summary>
    /// **Implementación de la lógica de negocio**: Crea la entidad Categoria.
    /// Este es el único método que tienes que implementar y donde se aplica el DDD.
    /// </summary>
    /// <param name="command">El comando con los datos de creación.</param>
    /// <returns>La nueva entidad Categoria creada.</returns>
    protected override Categoria CreateEntity(CreateCategoriaCommand command)
    {
        var nombreVO = Nombre.Create(command.Nombre).Value;
        var descripcionVO = new Descripcion(command.Descripcion ?? string.Empty);
        var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

        var newCategoria = Categoria.Create(
            nombreVO,
            usuarioId,
            descripcionVO
        );

        return newCategoria;
    }
}
