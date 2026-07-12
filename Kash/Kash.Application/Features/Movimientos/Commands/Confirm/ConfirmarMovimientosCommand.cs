using Kash.Application.Features.Movimientos.Commands.Import.Models;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using MediatR;

namespace Kash.Application.Features.Movimientos.Commands.Confirm;

/// <summary>
/// Crea los movimientos ya revisados y editados por el usuario, tal cual llegan.
/// </summary>
public sealed record ConfirmarMovimientosCommand(
    IReadOnlyList<MovimientoConfirmarDto> Movimientos) : IRequest<Result<ImportarMovimientosResult>>;
