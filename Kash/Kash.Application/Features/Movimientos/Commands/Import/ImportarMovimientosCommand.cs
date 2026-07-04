using Kash.Application.Features.Movimientos.Commands.Import.Models;
using Kash.Shared.Domain.Abstractions.Results;
using MediatR;

namespace Kash.Application.Features.Movimientos.Commands.Import;

/// <summary>
/// Importa gastos e ingresos desde un extracto bancario CSV de cualquier banco.
/// </summary>
public sealed record ImportarMovimientosCommand(
    byte[] FileContent,
    ColumnMapping Mapping) : IRequest<Result<ImportarMovimientosResult>>;
