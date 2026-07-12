using Kash.Application.Features.Movimientos.Commands.Import.Models;
using Kash.Application.Interfaces;
using Kash.Domain;
using Kash.Shared.Application.Interfaces;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;
using SergioIzq.Application.Kernel.Services;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;

namespace Kash.Application.Services;

/// <summary>
/// Implementación de <see cref="IMovimientoBulkCreator"/>. Ver el resumen de la interfaz para el
/// contrato. La clave de rendimiento frente a crear los movimientos uno a uno vía CQRS es:
/// (1) resolver cada dependencia por nombre una sola vez (memoización), (2) aplicar el saldo de
/// cada cuenta de forma agregada en lugar de por evento por fila, y (3) dos SaveChanges en total
/// (dependencias y luego movimientos+saldo) en vez de dos por fila.
/// </summary>
public sealed class MovimientoBulkCreator : IMovimientoBulkCreator
{
    private readonly ICategoriaFinderOrCreatorService _categoriaFinder;
    private readonly IConceptoFinderOrCreatorService _conceptoFinder;
    private readonly ICuentaFinderOrCreatorService _cuentaFinder;
    private readonly IFormaPagoFinderOrCreatorService _formaPagoFinder;
    private readonly IProveedorFinderOrCreatorService _proveedorFinder;
    private readonly IWriteRepository<Gasto, GastoId> _gastoRepository;
    private readonly IWriteRepository<Ingreso, IngresoId> _ingresoRepository;
    private readonly IWriteRepository<Cuenta, CuentaId> _cuentaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public MovimientoBulkCreator(
        ICategoriaFinderOrCreatorService categoriaFinder,
        IConceptoFinderOrCreatorService conceptoFinder,
        ICuentaFinderOrCreatorService cuentaFinder,
        IFormaPagoFinderOrCreatorService formaPagoFinder,
        IProveedorFinderOrCreatorService proveedorFinder,
        IWriteRepository<Gasto, GastoId> gastoRepository,
        IWriteRepository<Ingreso, IngresoId> ingresoRepository,
        IWriteRepository<Cuenta, CuentaId> cuentaRepository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService)
    {
        _categoriaFinder = categoriaFinder;
        _conceptoFinder = conceptoFinder;
        _cuentaFinder = cuentaFinder;
        _formaPagoFinder = formaPagoFinder;
        _proveedorFinder = proveedorFinder;
        _gastoRepository = gastoRepository;
        _ingresoRepository = ingresoRepository;
        _cuentaRepository = cuentaRepository;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<BulkCreateResult> CrearAsync(
        Guid usuarioId,
        IReadOnlyList<MovimientoACrear> movimientos,
        CancellationToken cancellationToken = default)
    {
        var errores = new List<MovimientoImportError>();
        if (movimientos.Count == 0)
            return new BulkCreateResult(0, 0, 0, errores);

        var usuarioIdVO = UsuarioId.Create(usuarioId).Value;

        // Cachés de memoización: cada nombre distinto se resuelve (o se crea) una sola vez.
        var cacheCategoria = new Dictionary<string, Guid>();
        var cacheConcepto = new Dictionary<string, Guid>();
        var cacheCuenta = new Dictionary<string, Guid>();
        var cacheFormaPago = new Dictionary<string, Guid>();
        var cacheProveedor = new Dictionary<string, Guid>();

        var preparados = new List<MovimientoPreparado>(movimientos.Count);

        // Saldo agregado por cuenta (se aplica una vez, no un evento por fila).
        var saldoPorCuenta = new Dictionary<Guid, (decimal Ingresos, decimal Gastos)>();
        var fallidos = 0;

        // --- Fase 1: validar, resolver dependencias (memoizadas) y agregar saldo ---
        foreach (var mov in movimientos)
        {
            try
            {
                var importeResult = Cantidad.Create(mov.Importe);
                if (importeResult.IsFailure)
                {
                    fallidos++;
                    errores.Add(new MovimientoImportError(0, mov.Descripcion ?? string.Empty, importeResult.Error.Message));
                    continue;
                }

                var fechaResult = FechaRegistro.Create(mov.Fecha);
                if (fechaResult.IsFailure)
                {
                    fallidos++;
                    errores.Add(new MovimientoImportError(0, mov.Descripcion ?? string.Empty, fechaResult.Error.Message));
                    continue;
                }

                var categoriaId = await ResolverAsync(cacheCategoria, mov.CategoriaNombre,
                    (nombre, ct) => _categoriaFinder.FindOrCreateAsync(null, nombre, usuarioId, null, ct), cancellationToken);

                var conceptoId = await ResolverAsync(cacheConcepto, $"{categoriaId}|{Normalizar(mov.ConceptoNombre)}",
                    (_, ct) => _conceptoFinder.FindOrCreateAsync(null, mov.ConceptoNombre, usuarioId,
                        new Dictionary<string, object> { ["CategoriaId"] = categoriaId }, ct), cancellationToken);

                var cuentaId = await ResolverAsync(cacheCuenta, mov.CuentaNombre,
                    (nombre, ct) => _cuentaFinder.FindOrCreateAsync(null, nombre, usuarioId, null, ct), cancellationToken);

                var formaPagoId = await ResolverAsync(cacheFormaPago, mov.FormaPagoNombre,
                    (nombre, ct) => _formaPagoFinder.FindOrCreateAsync(null, nombre, usuarioId, null, ct), cancellationToken);

                Guid? proveedorId = null;
                if (mov.EsGasto && !string.IsNullOrWhiteSpace(mov.ProveedorNombre))
                {
                    proveedorId = await ResolverOpcionalAsync(cacheProveedor, mov.ProveedorNombre,
                        (nombre, ct) => _proveedorFinder.FindOrCreateAsync(null, nombre, usuarioId, null, ct), cancellationToken);
                }

                preparados.Add(new MovimientoPreparado(
                    mov.EsGasto,
                    importeResult.Value,
                    fechaResult.Value,
                    new Descripcion(mov.Descripcion ?? string.Empty),
                    conceptoId,
                    cuentaId,
                    formaPagoId,
                    proveedorId));

                var acumulado = saldoPorCuenta.GetValueOrDefault(cuentaId);
                if (mov.EsGasto)
                    acumulado.Gastos += mov.Importe;
                else
                    acumulado.Ingresos += mov.Importe;
                saldoPorCuenta[cuentaId] = acumulado;
            }
            catch (Exception ex)
            {
                fallidos++;
                errores.Add(new MovimientoImportError(0, mov.Descripcion ?? string.Empty, ex.Message));
            }
        }

        // Persistir las dependencias auto-creadas antes de referenciarlas desde los movimientos
        // y de cargar las cuentas para ajustar su saldo.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // --- Fase 2: aplicar saldo agregado por cuenta ---
        foreach (var (cuentaId, montos) in saldoPorCuenta)
        {
            var cuenta = await _cuentaRepository.GetByIdAsync(cuentaId, cancellationToken);
            if (cuenta is null)
                continue;

            if (montos.Ingresos > 0)
                cuenta.Depositar(Cantidad.Create(montos.Ingresos).Value);
            if (montos.Gastos > 0)
                cuenta.Retirar(Cantidad.Create(montos.Gastos).Value);

            _cuentaRepository.Update(cuenta);
        }

        // --- Fase 3: construir y añadir los movimientos ---
        var gastos = 0;
        var ingresos = 0;
        foreach (var p in preparados)
        {
            if (p.EsGasto)
            {
                var gasto = Gasto.Create(
                    p.Importe, p.Fecha,
                    ConceptoId.Create(p.ConceptoId).Value,
                    p.ProveedorId.HasValue ? ProveedorId.Create(p.ProveedorId.Value).Value : null,
                    null,
                    CuentaId.Create(p.CuentaId).Value,
                    FormaPagoId.Create(p.FormaPagoId).Value,
                    usuarioIdVO,
                    p.Descripcion);

                // El saldo ya se aplicó de forma agregada; se descartan los eventos de saldo
                // por fila para no volver a descontarlo (único suscriptor de estos eventos).
                gasto.ClearDomainEvents();
                _gastoRepository.Add(gasto);
                gastos++;
            }
            else
            {
                var ingreso = Ingreso.Create(
                    p.Importe, p.Fecha,
                    ConceptoId.Create(p.ConceptoId).Value,
                    null,
                    null,
                    CuentaId.Create(p.CuentaId).Value,
                    FormaPagoId.Create(p.FormaPagoId).Value,
                    usuarioIdVO,
                    p.Descripcion);

                ingreso.ClearDomainEvents();
                _ingresoRepository.Add(ingreso);
                ingresos++;
            }
        }

        // Movimientos + saldo en una única transacción.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (gastos + ingresos > 0)
            await InvalidarCacheAsync(usuarioId);

        return new BulkCreateResult(gastos, ingresos, fallidos, errores);
    }

    private static string Normalizar(string nombre) => nombre.Trim().ToLowerInvariant();

    private static async Task<Guid> ResolverAsync(
        Dictionary<string, Guid> cache,
        string clave,
        Func<string, CancellationToken, Task<Guid?>> finder,
        CancellationToken cancellationToken)
    {
        var key = Normalizar(clave);
        if (cache.TryGetValue(key, out var existente))
            return existente;

        var resuelto = await finder(clave, cancellationToken);
        var id = resuelto ?? throw new InvalidOperationException($"No se pudo resolver la dependencia '{clave}'.");
        cache[key] = id;
        return id;
    }

    private static async Task<Guid?> ResolverOpcionalAsync(
        Dictionary<string, Guid> cache,
        string nombre,
        Func<string, CancellationToken, Task<Guid?>> finder,
        CancellationToken cancellationToken)
    {
        var key = Normalizar(nombre);
        if (cache.TryGetValue(key, out var existente))
            return existente;

        var resuelto = await finder(nombre, cancellationToken);
        if (resuelto is null)
            return null;

        cache[key] = resuelto.Value;
        return resuelto.Value;
    }

    private async Task InvalidarCacheAsync(Guid usuarioId)
    {
        // Se invalidan las versiones de lista de todas las entidades que la importación pudo tocar.
        string[] entidades = ["Gasto", "Ingreso", "Cuenta", "Categoria", "Concepto", "FormaPago", "Proveedor"];
        foreach (var entidad in entidades)
            await _cacheService.RemoveAsync($"list_version:{entidad}:{usuarioId}");
    }

    private sealed record MovimientoPreparado(
        bool EsGasto,
        Cantidad Importe,
        FechaRegistro Fecha,
        Descripcion Descripcion,
        Guid ConceptoId,
        Guid CuentaId,
        Guid FormaPagoId,
        Guid? ProveedorId);
}
