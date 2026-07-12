using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

public sealed record GetTraspasosProgramadosPagedListQuery : AbsGetPagedListQuery<TraspasoProgramado, TraspasoProgramadoId, TraspasoProgramadoDto>
{
    public GetTraspasosProgramadosPagedListQuery(
        int page,
        int pageSize,
        string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null)
        // FIX: Si es null, enviamos "" (cadena vacía)
        : base(page, pageSize, searchTerm ?? "", sortColumn ?? "", sortOrder ?? "")
    {
    }
}
