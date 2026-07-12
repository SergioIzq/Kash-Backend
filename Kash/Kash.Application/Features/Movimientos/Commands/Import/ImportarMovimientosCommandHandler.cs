using Kash.Application.Features.Movimientos.Commands.Import.Models;
using Kash.Application.Features.Movimientos.Commands.Import.Parsers;
using Kash.Application.Interfaces;
using Kash.Application.Interfaces.Repositories;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Application.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using MediatR;

namespace Kash.Application.Features.Movimientos.Commands.Import;

/// <summary>
/// Parsea el extracto y crea los movimientos en bloque (vía <see cref="IMovimientoBulkCreator"/>),
/// auto-creando por nombre la cuenta, categoría, concepto y forma de pago. Aplica las reglas de
/// auto-categorización del usuario antes de los valores por defecto del mapeo. Deduplica dentro
/// del propio fichero y contra la base de datos, cargando las claves existentes en una sola query.
/// </summary>
public sealed class ImportarMovimientosCommandHandler
    : IRequestHandler<ImportarMovimientosCommand, Result<ImportarMovimientosResult>>
{
    private readonly IUserContext _userContext;
    private readonly GenericBankCsvParser _csvParser;
    private readonly GenericBankPdfParser _pdfParser;
    private readonly IMovimientoDuplicadoChecker _duplicadoChecker;
    private readonly IReglaCategorizacionReadRepository _reglaReadRepository;
    private readonly IMovimientoBulkCreator _bulkCreator;

    public ImportarMovimientosCommandHandler(
        IUserContext userContext,
        GenericBankCsvParser csvParser,
        GenericBankPdfParser pdfParser,
        IMovimientoDuplicadoChecker duplicadoChecker,
        IReglaCategorizacionReadRepository reglaReadRepository,
        IMovimientoBulkCreator bulkCreator)
    {
        _userContext = userContext;
        _csvParser = csvParser;
        _pdfParser = pdfParser;
        _duplicadoChecker = duplicadoChecker;
        _reglaReadRepository = reglaReadRepository;
        _bulkCreator = bulkCreator;
    }

    public async Task<Result<ImportarMovimientosResult>> Handle(
        ImportarMovimientosCommand request,
        CancellationToken cancellationToken)
    {
        var usuarioId = _userContext.UserId;
        if (usuarioId is null)
            return Result.Failure<ImportarMovimientosResult>(Error.Unauthorized("Usuario no autenticado."));

        var map = request.Mapping;

        // Selección automática de parser según el tipo de fichero (PDF empieza por "%PDF").
        var parse = ImportFileType.EsPdf(request.FileContent)
            ? await _pdfParser.ParseAsync(request.FileContent, map, cancellationToken)
            : await _csvParser.ParseAsync(request.FileContent, map, cancellationToken);

        var errores = parse.Errores;

        if (parse.Filas.Count == 0)
            return Result.Success(new ImportarMovimientosResult(0, 0, 0, 0, errores));

        var reglas = (await _reglaReadRepository.GetActivasOrdenadasAsync(usuarioId.Value, cancellationToken)).ToList();

        // Deduplicación contra BD: una sola query con todas las claves del rango del fichero.
        var desde = parse.Filas.Min(f => f.Fecha);
        var hasta = parse.Filas.Max(f => f.Fecha);
        var clavesExistentes = await _duplicadoChecker.CargarClavesExistentesAsync(
            usuarioId.Value, map.CuentaNombre, desde, hasta, cancellationToken);

        var vistos = new HashSet<string>();
        var aCrear = new List<MovimientoACrear>(parse.Filas.Count);
        var duplicados = 0;

        foreach (var mov in parse.Filas)
        {
            var esGasto = mov.Tipo == TipoMovimiento.Gasto;
            var clave = MovimientoDedupKey.Construir(esGasto, mov.Fecha, mov.Importe, mov.Descripcion);

            // Duplicado dentro del propio fichero o ya existente en base de datos.
            if (!vistos.Add(clave) || clavesExistentes.Contains(clave))
            {
                duplicados++;
                continue;
            }

            var tipoStr = esGasto ? "gasto" : "ingreso";
            var regla = ReglaCategorizacionMatcher.Encontrar(reglas, mov.Descripcion, tipoStr);

            aCrear.Add(new MovimientoACrear(
                EsGasto: esGasto,
                Importe: mov.Importe,
                Fecha: mov.Fecha,
                Descripcion: mov.Descripcion,
                CategoriaNombre: regla?.CategoriaNombre ?? (esGasto ? map.CategoriaGastoNombre : map.CategoriaIngresoNombre),
                ConceptoNombre: regla?.ConceptoNombre ?? map.ConceptoNombre,
                CuentaNombre: map.CuentaNombre,
                FormaPagoNombre: regla?.FormaPagoNombre ?? map.FormaPagoNombre,
                ProveedorNombre: regla?.ProveedorNombre));
        }

        var resultado = await _bulkCreator.CrearAsync(usuarioId.Value, aCrear, cancellationToken);

        errores.AddRange(resultado.Errores);

        return Result.Success(new ImportarMovimientosResult(
            resultado.Gastos, resultado.Ingresos, duplicados, resultado.Fallidos, errores));
    }
}
