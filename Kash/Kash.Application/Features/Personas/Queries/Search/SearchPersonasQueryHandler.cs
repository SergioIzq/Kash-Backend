using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Personas.Queries.Search;

/// <summary>
/// Handler para búsqueda rápida de cuentas (autocomplete).
/// </summary>
public sealed class SearchPersonasQueryHandler
    : SearchForAutocompleteQueryHandler<Persona, PersonaDto, SearchPersonasQuery, PersonaId>
{
    public SearchPersonasQueryHandler(
    IReadRepository<Persona, PersonaDto, PersonaId> repository,
        ICacheService cacheService)
        : base(repository, cacheService)
    {
    }
}
