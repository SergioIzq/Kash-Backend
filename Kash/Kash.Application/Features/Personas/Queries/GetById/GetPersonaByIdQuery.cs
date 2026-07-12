using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Personas.Queries;

/// <summary>
/// Representa la solicitud para crear un nuevo Persona.
/// </summary>
// Hereda de AbsCreateCommand<Entidad, DTO de Respuesta>
public sealed record GetPersonaByIdQuery(Guid Id) : AbsGetByIdQuery<Persona, PersonaId, PersonaDto>(Id)
{
}
