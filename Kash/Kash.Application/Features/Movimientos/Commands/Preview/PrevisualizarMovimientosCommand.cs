using Kash.Application.Features.Movimientos.Commands.Import.Models;
using Kash.Shared.Domain.Abstractions.Results;
using MediatR;

namespace Kash.Application.Features.Movimientos.Commands.Preview;

/// <summary>
/// Parsea el extracto y devuelve los movimientos que se CREARÍAN (sin crearlos),
/// marcando los duplicados, para que el usuario los revise y edite antes de confirmar.
/// </summary>
public sealed record PrevisualizarMovimientosCommand(
    byte[] FileContent,
    ColumnMapping Mapping) : IRequest<Result<PreviewMovimientosResult>>;
