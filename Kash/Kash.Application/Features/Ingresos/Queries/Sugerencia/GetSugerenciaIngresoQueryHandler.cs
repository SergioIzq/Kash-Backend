using Kash.Application.Interfaces;
using Kash.Shared.Application.Dtos;
using MediatR;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Application.Features.Ingresos.Queries.Sugerencia;

/// <summary>
/// Equivalente de <see cref="Gastos.Queries.Sugerencia.GetSugerenciaGastoQueryHandler"/> para
/// ingresos. Sin caché a propósito (pedido explícitamente): tras crear un ingreso se quiere ver
/// reflejado de inmediato en la siguiente sugerencia. Ver <see cref="IIngresoSugerenciaRepository"/>.
/// </summary>
public sealed class GetSugerenciaIngresoQueryHandler : IRequestHandler<GetSugerenciaIngresoQuery, Result<IReadOnlyList<IngresoDto>>>
{
    private readonly IIngresoSugerenciaRepository _repository;

    public GetSugerenciaIngresoQueryHandler(IIngresoSugerenciaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<IngresoDto>>> Handle(GetSugerenciaIngresoQuery request, CancellationToken cancellationToken)
    {
        var ultimo = await _repository.GetUltimoUsoAsync(request.UsuarioId, request.ConceptoId, cancellationToken);
        IReadOnlyList<IngresoDto> resultado = ultimo is null ? Array.Empty<IngresoDto>() : [ultimo];
        return Result.Success(resultado);
    }
}
