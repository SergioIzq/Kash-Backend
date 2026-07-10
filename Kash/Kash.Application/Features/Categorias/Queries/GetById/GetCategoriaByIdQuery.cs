using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Categorias.Queries;

/// <summary>
/// Representa la solicitud para crear una nueva Categoría.
/// </summary>
// Hereda de AbsCreateCommand<Entidad, DTO de Respuesta>
public sealed record GetCategoriaByIdQuery(Guid id) : AbsGetByIdQuery<Categoria, CategoriaId, CategoriaDto>(id)
{
}
