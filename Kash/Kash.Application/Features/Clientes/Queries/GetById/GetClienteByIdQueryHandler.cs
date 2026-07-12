using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Clientes.Queries;

/// <summary>
/// Maneja la creación de una nueva entidad Categoria.
/// </summary>
public sealed class GetClienteByIdQueryHandler
    : GetByIdQueryHandler<Cliente, ClienteId, ClienteDto, GetClienteByIdQuery>
{
    public GetClienteByIdQueryHandler(
        ICacheService cacheService,
        IReadRepository<Cliente, ClienteDto, ClienteId> readOnlyRepository
        )
        : base(readOnlyRepository, cacheService)
    {
    }
}
