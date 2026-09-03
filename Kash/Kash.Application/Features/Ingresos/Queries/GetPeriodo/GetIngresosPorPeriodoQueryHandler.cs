using Kash.Application.Interfaces;
using SergioIzq.Application.Kernel.Messaging;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Application.Features.Ingresos.Queries.GetPeriodo;

/// <summary>
/// Obtiene los Ingresos del usuario que cumplen el rango de fechas indicado, paginados.
/// </summary>
public sealed class GetIngresosPorPeriodoQueryHandler : IQueryHandler<GetIngresosPorPeriodoQuery, PagedList<IngresoDto>>
{
    private readonly IIngresoPeriodoRepository _periodoRepository;

    public GetIngresosPorPeriodoQueryHandler(IIngresoPeriodoRepository periodoRepository)
    {
        _periodoRepository = periodoRepository;
    }

    public async Task<Result<PagedList<IngresoDto>>> Handle(
        GetIngresosPorPeriodoQuery request,
        CancellationToken cancellationToken)
    {
        if (request.FechaInicio > request.FechaFin)
        {
            return Result.Failure<PagedList<IngresoDto>>(
                Error.Validation("La fecha de inicio no puede ser posterior a la fecha de fin."));
        }

        var resultado = await _periodoRepository.GetPagedByPeriodoAsync(
            request.UsuarioId,
            request.FechaInicio,
            request.FechaFin,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result.Success(resultado);
    }
}
