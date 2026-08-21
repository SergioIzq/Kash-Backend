using Kash.Application.Interfaces;
using Kash.Shared.Application.Dtos;
using MediatR;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Application.Features.Gastos.Queries.Sugerencia;

/// <summary>
/// Devuelve el gasto más reciente del usuario para un concepto dado. Sin caché a propósito
/// (pedido explícitamente): tras crear un gasto se quiere ver reflejado de inmediato en la
/// siguiente sugerencia, sin esperar a que expire ningún TTL. No hereda de
/// <c>GetRecentQueryHandler</c> porque ese mecanismo genérico ordena según el
/// <c>DefaultOrderBy</c> de <c>GastoReadRepository</c> (<c>fecha DESC, id DESC</c>): con varios
/// gastos en la misma fecha, el desempate por <c>id</c> (GUID) no refleja cuál se creó
/// realmente el último. Ver <see cref="IGastoSugerenciaRepository"/>.
/// </summary>
public sealed class GetSugerenciaGastoQueryHandler : IRequestHandler<GetSugerenciaGastoQuery, Result<IReadOnlyList<GastoDto>>>
{
    private readonly IGastoSugerenciaRepository _repository;

    public GetSugerenciaGastoQueryHandler(IGastoSugerenciaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<GastoDto>>> Handle(GetSugerenciaGastoQuery request, CancellationToken cancellationToken)
    {
        var ultimo = await _repository.GetUltimoUsoAsync(request.UsuarioId, request.ConceptoId, cancellationToken);
        IReadOnlyList<GastoDto> resultado = ultimo is null ? Array.Empty<GastoDto>() : [ultimo];
        return Result.Success(resultado);
    }
}
