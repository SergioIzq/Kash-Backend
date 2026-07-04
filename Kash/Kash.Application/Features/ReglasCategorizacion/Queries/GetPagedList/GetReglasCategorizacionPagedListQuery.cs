using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.ReglasCategorizacion.Queries;

public sealed record GetReglasCategorizacionPagedListQuery
    : AbsGetPagedListQuery<ReglaCategorizacion, ReglaCategorizacionId, ReglaCategorizacionDto>
{
    public GetReglasCategorizacionPagedListQuery(
        int page,
        int pageSize,
        string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null)
        : base(page, pageSize, searchTerm ?? "", sortColumn ?? "", sortOrder ?? "")
    {
    }
}
