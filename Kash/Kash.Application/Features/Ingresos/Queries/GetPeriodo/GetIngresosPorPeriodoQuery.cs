using SergioIzq.Application.Kernel.Messaging;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Application.Features.Ingresos.Queries.GetPeriodo;

/// <summary>
/// Obtiene los Ingresos del usuario cuya fecha de transacción cae dentro de un rango indicado,
/// paginados. No hereda de AbsGetPagedListQuery porque el repositorio genérico del kernel
/// (IReadRepository.GetPagedReadModelsByUserAsync) no acepta rango de fechas.
/// </summary>
public sealed record GetIngresosPorPeriodoQuery(
    Guid UsuarioId,
    DateTime FechaInicio,
    DateTime FechaFin,
    int Page,
    int PageSize
) : IQuery<PeriodoResult<IngresoDto>>;
