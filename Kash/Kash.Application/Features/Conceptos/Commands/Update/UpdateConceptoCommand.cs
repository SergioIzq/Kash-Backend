using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Conceptos.Commands;

/// <summary>
/// Representa la solicitud para actualizar un nuevo concepto.
/// </summary>
// Hereda de AbsUpadteCommand<Entidad, DTO de Respuesta>
public sealed record UpdateConceptoCommand : AbsUpdateCommand<Concepto, ConceptoId, ConceptoDto>
{
    /// <summary>
    /// Nombre del nuevo concepto.
    /// </summary>
    public required string Nombre { get; init; }

    public required Guid CategoriaId { get; init; }
}
