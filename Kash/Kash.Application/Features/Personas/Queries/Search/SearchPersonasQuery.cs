using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Personas.Queries;

/// <summary>
/// Query para búsqueda rápida de personas (autocomplete).
/// </summary>
public sealed record SearchPersonasQuery : SearchForAutocompleteQuery<Persona, PersonaDto, PersonaId>
{
    public SearchPersonasQuery(string searchTerm, int limit = 10)
        : base(searchTerm, limit)
    {
    }
}
