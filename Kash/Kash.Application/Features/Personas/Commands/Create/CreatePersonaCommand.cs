using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Personas.Commands;

public sealed record CreatePersonaCommand : AbsCreateCommand<Persona, PersonaId>
{
    public required string Nombre { get; init; }
    public required Guid UsuarioId { get; init; }
}
