using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.Results;

namespace AhorroLand.Application.Features.Clientes.Queries;

/// <summary>
/// Manejador concreto para la consulta de lista paginada de Clientes.
/// </summary>
public sealed class GetClientesPagedListQueryHandler
    : GetPagedListQueryHandler<Cliente, ClienteDto, GetClientesPagedListQuery>
{
    public GetClientesPagedListQueryHandler(
        IReadRepositoryWithDto<Cliente, ClienteDto> clienteRepository,
        ICacheService cacheService)
        : base(clienteRepository, cacheService)
    {
 }

    /// <summary>
    /// 🚀 OPTIMIZADO: Usa método específico del repositorio que filtra por usuario.
    /// </summary>
    protected override async Task<PagedList<ClienteDto>> ApplyFiltersAsync(
        GetClientesPagedListQuery query,
        CancellationToken cancellationToken)
    {
    // 🔥 Si tenemos UsuarioId, usar el método optimizado con filtro
        if (query.UsuarioId.HasValue)
        {
     return await _dtoRepository.GetPagedReadModelsByUserAsync(
  query.UsuarioId.Value,
                query.Page,
       query.PageSize,
       null, // searchTerm
    null, // sortColumn
   null, // sortOrder
      cancellationToken);
        }

        // Sin UsuarioId, dejamos que el handler base maneje
   return null!;
    }
}