using Kash.Application.Interfaces;
using SergioIzq.Application.Kernel.Messaging;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Application.Features.Gastos.Queries.GetPeriodo;

/// <summary>
/// Obtiene los Gastos del usuario que cumplen el rango de fechas indicado, paginados.
/// </summary>
public sealed class GetGastosPorPeriodoQueryHandler : IQueryHandler<GetGastosPorPeriodoQuery, PagedList<GastoDto>>
{
    private readonly IGastoPeriodoRepository _periodoRepository;

    public GetGastosPorPeriodoQueryHandler(IGastoPeriodoRepository periodoRepository)
    {
        _periodoRepository = periodoRepository;
    }

    public async Task<Result<PagedList<GastoDto>>> Handle(
        GetGastosPorPeriodoQuery request,
        CancellationToken cancellationToken)
    {
        if (request.FechaInicio > request.FechaFin)
        {
            return Result.Failure<PagedList<GastoDto>>(
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
