using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Categorias.Commands;

/// <summary>
/// Representa la solicitud para eliminar una Categoría por su identificador.
/// </summary>
// Hereda de AbsDeleteCommand<Entidad>
public sealed record DeleteCategoriaCommand(Guid Id)
    : AbsDeleteCommand<Categoria, CategoriaId>(Id);
