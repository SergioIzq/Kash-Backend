using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Conceptos.Commands;

/// <summary>
/// Representa la solicitud para crear un nuevo Concepto.
/// </summary>
public sealed record CreateConceptoCommand : AbsCreateCommand<Concepto, ConceptoId>
{
    public required string Nombre { get; init; }
    public required Guid CategoriaId { get; init; }
    public required Guid UsuarioId { get; init; }
}
