using Kash.Application.Features.Gastos.Commands;
using Kash.Application.Features.Ingresos.Commands;
using Kash.Application.Features.Movimientos.Commands.Import.Models;
using Kash.Application.Features.Movimientos.Commands.Import.Parsers;
using Kash.Application.Interfaces.Repositories;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kash.Application.Features.Movimientos.Commands.Import;

/// <summary>
/// Parsea el extracto y crea cada movimiento reutilizando <see cref="CreateGastoCommand"/> y
/// <see cref="CreateIngresoCommand"/>, que auto-crean por nombre la cuenta, categoría, concepto
/// y forma de pago. Aplica las reglas de auto-categorización del usuario antes de los valores
/// por defecto del mapeo. Evita duplicados dentro del propio fichero y contra la base de datos.
/// </summary>
public sealed class ImportarMovimientosCommandHandler
    : IRequestHandler<ImportarMovimientosCommand, Result<ImportarMovimientosResult>>
{
    private readonly IMediator _mediator;
    private readonly IUserContext _userContext;
    private readonly GenericBankCsvParser _csvParser;
    private readonly GenericBankPdfParser _pdfParser;
    private readonly IMovimientoDuplicadoChecker _duplicadoChecker;
    private readonly IReglaCategorizacionReadRepository _reglaReadRepository;
    private readonly ILogger<ImportarMovimientosCommandHandler> _logger;

    public ImportarMovimientosCommandHandler(
        IMediator mediator,
        IUserContext userContext,
        GenericBankCsvParser csvParser,
        GenericBankPdfParser pdfParser,
        IMovimientoDuplicadoChecker duplicadoChecker,
        IReglaCategorizacionReadRepository reglaReadRepository,
        ILogger<ImportarMovimientosCommandHandler> logger)
    {
        _mediator = mediator;
        _userContext = userContext;
        _csvParser = csvParser;
        _pdfParser = pdfParser;
        _duplicadoChecker = duplicadoChecker;
        _reglaReadRepository = reglaReadRepository;
        _logger = logger;
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

        var reglas = (await _reglaReadRepository.GetActivasOrdenadasAsync(usuarioId.Value, cancellationToken)).ToList();

        var errores = parse.Errores;
        var vistos = new HashSet<string>();
        int gastos = 0, ingresos = 0, duplicados = 0, fallidos = 0;

        foreach (var mov in parse.Filas)
        {
            // 1. Deduplicación dentro del propio fichero
            var clave = $"{mov.Fecha:yyyy-MM-dd}|{mov.Tipo}|{mov.Importe}|{mov.Descripcion}";
            if (!vistos.Add(clave))
            {
                duplicados++;
                continue;
            }

            // 2. Deduplicación contra la base de datos (reimportar extractos solapados)
            var yaExiste = mov.Tipo == TipoMovimiento.Gasto
                ? await _duplicadoChecker.ExisteGastoAsync(
                    usuarioId.Value, mov.Fecha, mov.Importe, mov.Descripcion, map.CuentaNombre, cancellationToken)
                : await _duplicadoChecker.ExisteIngresoAsync(
                    usuarioId.Value, mov.Fecha, mov.Importe, mov.Descripcion, map.CuentaNombre, cancellationToken);

            if (yaExiste)
            {
                duplicados++;
                continue;
            }

            var tipoStr = mov.Tipo == TipoMovimiento.Gasto ? "gasto" : "ingreso";
            var regla = ReglaCategorizacionMatcher.Encontrar(reglas, mov.Descripcion, tipoStr);

            var result = mov.Tipo == TipoMovimiento.Gasto
                ? await _mediator.Send(CrearGasto(mov, map, regla, usuarioId.Value), cancellationToken)
                : await _mediator.Send(CrearIngreso(mov, map, regla, usuarioId.Value), cancellationToken);

            if (result.IsSuccess)
            {
                if (mov.Tipo == TipoMovimiento.Gasto) gastos++;
                else ingresos++;
            }
            else
            {
                fallidos++;
                errores.Add(new MovimientoImportError(0, mov.Descripcion, result.Error.Message));
                _logger.LogWarning("No se pudo importar el movimiento '{Desc}': {Error}",
                    mov.Descripcion, result.Error.Message);
            }
        }

        return Result.Success(new ImportarMovimientosResult(gastos, ingresos, duplicados, fallidos, errores));
    }

    private static CreateGastoCommand CrearGasto(
        MovimientoImportDto mov, ColumnMapping map, ReglaCategorizacionDto? regla, Guid usuarioId) => new()
    {
        Importe = mov.Importe,
        Fecha = mov.Fecha,
        Descripcion = mov.Descripcion,
        CategoriaId = Guid.Empty,
        ConceptoId = Guid.Empty,
        ProveedorId = null,
        PersonaId = null,
        CuentaId = Guid.Empty,
        FormaPagoId = Guid.Empty,
        UsuarioId = usuarioId,
        CategoriaNombre = regla?.CategoriaNombre ?? map.CategoriaGastoNombre,
        ConceptoNombre = regla?.ConceptoNombre ?? map.ConceptoNombre,
        CuentaNombre = map.CuentaNombre,
        FormaPagoNombre = regla?.FormaPagoNombre ?? map.FormaPagoNombre,
        ProveedorNombre = regla?.ProveedorNombre
    };

    private static CreateIngresoCommand CrearIngreso(
        MovimientoImportDto mov, ColumnMapping map, ReglaCategorizacionDto? regla, Guid usuarioId) => new()
    {
        Importe = mov.Importe,
        Fecha = mov.Fecha,
        Descripcion = mov.Descripcion,
        CategoriaId = Guid.Empty,
        ConceptoId = Guid.Empty,
        ClienteId = null,
        PersonaId = null,
        CuentaId = Guid.Empty,
        FormaPagoId = Guid.Empty,
        UsuarioId = usuarioId,
        CategoriaNombre = regla?.CategoriaNombre ?? map.CategoriaIngresoNombre,
        ConceptoNombre = regla?.ConceptoNombre ?? map.ConceptoNombre,
        CuentaNombre = map.CuentaNombre,
        FormaPagoNombre = regla?.FormaPagoNombre ?? map.FormaPagoNombre
    };
}
