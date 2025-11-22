using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using AhorroLand.Shared.Application.Dtos;

namespace AhorroLand.Application.Features.Gastos.Queries;

/// <summary>
/// Representa la consulta para obtener una lista paginada de Gastos con búsqueda y ordenamiento.
/// </summary>
public sealed record GetGastosPagedListQuery : AbsGetPagedListQuery<Gasto, GastoDto>
{
    public string? SearchTerm { get; init; }
    public string? SortColumn { get; init; }
    public string? SortOrder { get; init; }

    public GetGastosPagedListQuery(
        int page,
        int pageSize,
        string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null)
        : base(page, pageSize, null)
    {
        Page = page;
        PageSize = pageSize;
        SearchTerm = searchTerm;
        SortColumn = sortColumn ?? "Fecha"; // Por defecto ordenar por fecha
        SortOrder = sortOrder ?? "desc"; // Por defecto descendente
    }
}