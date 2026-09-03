using SergioIzq.Application.Kernel.Messaging;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Application.Features.Gastos.Queries.GetPeriodo;

/// <summary>
/// Obtiene los Gastos del usuario cuya fecha de transacción cae dentro de un rango indicado,
/// paginados. No hereda de AbsGetPagedListQuery porque el repositorio genérico del kernel
/// (IReadRepository.GetPagedReadModelsByUserAsync) no acepta rango de fechas.
/// </summary>
public sealed record GetGastosPorPeriodoQuery(
    Guid UsuarioId,
    DateTime FechaInicio,
    DateTime FechaFin,
    int Page,
    int PageSize
) : IQuery<PagedList<GastoDto>>;
