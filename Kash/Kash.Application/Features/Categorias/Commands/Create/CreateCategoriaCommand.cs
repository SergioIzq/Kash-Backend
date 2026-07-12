using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Categorias.Commands;

/// <summary>
/// Representa la solicitud para crear una nueva Categoría.
/// </summary>
// Hereda de AbsCreateCommand<Entidad, DTO de Respuesta>
public sealed record CreateCategoriaCommand : AbsCreateCommand<Categoria, CategoriaId>
{
    /// <summary>
    /// Nombre de la nueva categoría.
    /// </summary>
    public required string Nombre { get; init; }
    public required Guid UsuarioId { get; init; }

    /// <summary>
    /// Descripción opcional de la categoría.
    /// </summary>
    public string? Descripcion { get; init; }
}
