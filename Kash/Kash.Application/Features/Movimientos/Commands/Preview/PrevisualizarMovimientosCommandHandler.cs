using Kash.Application.Features.Movimientos.Commands.Import;
using Kash.Application.Features.Movimientos.Commands.Import.Models;
using Kash.Application.Features.Movimientos.Commands.Import.Parsers;
using Kash.Application.Interfaces.Repositories;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using MediatR;

namespace Kash.Application.Features.Movimientos.Commands.Preview;

/// <summary>
/// Genera la previsualización: parsea el fichero (CSV o PDF), aplica las reglas de
/// auto-categorización del usuario (o los valores por defecto del mapeo si ninguna coincide)
/// a cada línea, y marca los duplicados (dentro del fichero y en BD). No crea nada.
/// </summary>
public sealed class PrevisualizarMovimientosCommandHandler
    : IRequestHandler<PrevisualizarMovimientosCommand, Result<PreviewMovimientosResult>>
{
    private readonly IUserContext _userContext;
    private readonly GenericBankCsvParser _csvParser;
    private readonly GenericBankPdfParser _pdfParser;
    private readonly IMovimientoDuplicadoChecker _duplicadoChecker;
    private readonly IReglaCategorizacionReadRepository _reglaReadRepository;

    public PrevisualizarMovimientosCommandHandler(
        IUserContext userContext,
        GenericBankCsvParser csvParser,
        GenericBankPdfParser pdfParser,
        IMovimientoDuplicadoChecker duplicadoChecker,
        IReglaCategorizacionReadRepository reglaReadRepository)
    {
        _userContext = userContext;
        _csvParser = csvParser;
        _pdfParser = pdfParser;
        _duplicadoChecker = duplicadoChecker;
        _reglaReadRepository = reglaReadRepository;
    }

    public async Task<Result<PreviewMovimientosResult>> Handle(
        PrevisualizarMovimientosCommand request,
        CancellationToken cancellationToken)
    {
        var usuarioId = _userContext.UserId;
        if (usuarioId is null)
            return Result.Failure<PreviewMovimientosResult>(Error.Unauthorized("Usuario no autenticado."));

        var map = request.Mapping;

        var parse = ImportFileType.EsPdf(request.FileContent)
            ? await _pdfParser.ParseAsync(request.FileContent, map, cancellationToken)
            : await _csvParser.ParseAsync(request.FileContent, map, cancellationToken);

        // Reglas activas del usuario, ya ordenadas por prioridad: se cargan una sola vez por petición.
        var reglas = (await _reglaReadRepository.GetActivasOrdenadasAsync(usuarioId.Value, cancellationToken)).ToList();

        var movimientos = new List<MovimientoPreviewDto>(parse.Filas.Count);
        var vistos = new HashSet<string>();
        var i = 0;

        foreach (var mov in parse.Filas)
        {
            var esGasto = mov.Tipo == TipoMovimiento.Gasto;
            var tipoStr = esGasto ? "gasto" : "ingreso";

            // Duplicado dentro del fichero
            var clave = $"{mov.Fecha:yyyy-MM-dd}|{mov.Tipo}|{mov.Importe}|{mov.Descripcion}";
            var duplicadoEnFichero = !vistos.Add(clave);

            // Duplicado en base de datos
            var duplicadoEnBd = esGasto
                ? await _duplicadoChecker.ExisteGastoAsync(
                    usuarioId.Value, mov.Fecha, mov.Importe, mov.Descripcion, map.CuentaNombre, cancellationToken)
                : await _duplicadoChecker.ExisteIngresoAsync(
                    usuarioId.Value, mov.Fecha, mov.Importe, mov.Descripcion, map.CuentaNombre, cancellationToken);

            // Auto-categorización por reglas: si alguna coincide, sustituye a los valores por defecto.
            var regla = ReglaCategorizacionMatcher.Encontrar(reglas, mov.Descripcion, tipoStr);
            var categoriaDefecto = esGasto ? map.CategoriaGastoNombre : map.CategoriaIngresoNombre;

            movimientos.Add(new MovimientoPreviewDto(
                Id: i.ToString(),
                Fecha: mov.Fecha,
                Descripcion: mov.Descripcion,
                Importe: mov.Importe,
                Tipo: tipoStr,
                CuentaNombre: map.CuentaNombre,
                CategoriaNombre: regla?.CategoriaNombre ?? categoriaDefecto,
                ConceptoNombre: regla?.ConceptoNombre ?? map.ConceptoNombre,
                FormaPagoNombre: regla?.FormaPagoNombre ?? map.FormaPagoNombre,
                ProveedorNombre: esGasto ? regla?.ProveedorNombre : null,
                EsDuplicado: duplicadoEnFichero || duplicadoEnBd,
                ReglaAplicada: regla is not null,
                ReglaPatron: regla?.Patron));

            i++;
        }

        return Result.Success(new PreviewMovimientosResult(movimientos, parse.Errores));
    }
}
