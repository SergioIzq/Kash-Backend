using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Clientes.Queries;

/// <summary>
/// Manejador concreto para la consulta de lista paginada de Clientes.
/// </summary>
public sealed class GetClientesPagedListQueryHandler
    : GetPagedListQueryHandler<Cliente, ClienteId, ClienteDto, GetClientesPagedListQuery>
{
    public GetClientesPagedListQueryHandler(
        IReadRepository<Cliente, ClienteDto, ClienteId> clienteRepository,
        ICacheService cacheService)
        : base(clienteRepository, cacheService)
    {
    }

    /// <summary>
    /// OPTIMIZADO: Usa método específico del repositorio que filtra por usuario.
    /// </summary>
    protected override async Task<PagedList<ClienteDto>> ApplyFiltersAsync(
        GetClientesPagedListQuery query,
        CancellationToken cancellationToken)
    {
        // Si tenemos UsuarioId, usar el método optimizado con filtro
        if (query.UsuarioId.HasValue)
        {
            return await _dtoRepository.GetPagedReadModelsByUserAsync(
         query.UsuarioId.Value,
                       query.Page,
              query.PageSize,
              query.SearchTerm, // searchTerm
           query.SortColumn, // sortColumn
          query.SortOrder, // sortOrder
             cancellationToken);
        }

        // Sin UsuarioId, dejamos que el handler base maneje
        return null!;
    }
}
