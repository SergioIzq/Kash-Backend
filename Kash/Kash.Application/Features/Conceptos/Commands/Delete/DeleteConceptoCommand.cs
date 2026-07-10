using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Conceptos.Commands;

/// <summary>
/// Representa la solicitud para eliminar un Concepto por su identificador.
/// </summary>
// Hereda de AbsDeleteCommand<Entidad>
public sealed record DeleteConceptoCommand(Guid Id)
    : AbsDeleteCommand<Concepto, ConceptoId>(Id);
