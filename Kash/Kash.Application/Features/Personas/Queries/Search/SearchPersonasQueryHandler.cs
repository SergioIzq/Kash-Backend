using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
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
