using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces.Repositories;

namespace AhorroLand.Application.Features.Clientes.Queries;

/// <summary>
/// Maneja la creación de una nueva entidad Categoria.
/// </summary>
public sealed class GetClienteByIdQueryHandler
    : GetByIdQueryHandler<Cliente, ClienteDto, GetClienteByIdQuery>
{
    public GetClienteByIdQueryHandler(
        ICacheService cacheService,
        IReadRepositoryWithDto<Cliente, ClienteDto> readOnlyRepository
        )
        : base(readOnlyRepository, cacheService)
    {
    }
}