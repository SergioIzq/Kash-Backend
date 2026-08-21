using Kash.Application.Interfaces;
using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Conceptos.Queries;

/// <summary>
/// Manejador concreto para la consulta de lista paginada de Conceptos.
/// </summary>
public sealed class GetConceptosPagedListQueryHandler
    : GetPagedListQueryHandler<Concepto, ConceptoId, ConceptoDto, GetConceptosPagedListQuery>
{
    private readonly IConceptoPaginadoRepository _conceptoPaginadoRepository;

    public GetConceptosPagedListQueryHandler(
        IReadRepository<Concepto, ConceptoDto, ConceptoId> repository,
        ICacheService cacheService,
        IConceptoPaginadoRepository conceptoPaginadoRepository)
    : base(repository, cacheService)
    {
        _conceptoPaginadoRepository = conceptoPaginadoRepository;
    }

    protected override async Task<PagedList<ConceptoDto>> ApplyFiltersAsync(
        GetConceptosPagedListQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.UsuarioId.HasValue)
        {
            return null!;
        }

        // Sin categoriaId: mismo comportamiento que antes de este cambio (comportamiento genérico del kernel).
        if (string.IsNullOrEmpty(query.CategoriaId))
        {
            return await _dtoRepository.GetPagedReadModelsByUserAsync(
                query.UsuarioId.Value,
                query.Page,
                query.PageSize,
                query.SearchTerm,
                query.SortColumn,
                query.SortOrder,
                cancellationToken);
        }

        // Con categoriaId: el kernel no acepta filtros extra en el paginado, así que se usa
        // el repositorio Dapper propio (ver IConceptoPaginadoRepository).
        return await _conceptoPaginadoRepository.GetPagedByCategoriaAsync(
            query.UsuarioId.Value,
            Guid.Parse(query.CategoriaId),
            query.Page,
            query.PageSize,
            query.SearchTerm,
            query.SortColumn,
            query.SortOrder,
            cancellationToken);
    }
}
