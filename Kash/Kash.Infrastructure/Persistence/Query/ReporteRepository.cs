using Kash.Application.Interfaces;
using Kash.Shared.Application.Dtos.Reportes;
using SergioIzq.Infrastructure.Kernel.Persistence;
using Dapper;

namespace Kash.Infrastructure.Persistence.Query;

/// <summary>
/// Repositorio de lectura del reporte de presupuesto. Agrega ingresos y gastos del rango
/// en una sola ida a la base de datos (varios resultsets) y arma la estructura jerárquica
/// categoría → concepto en memoria.
/// </summary>
public sealed class ReporteRepository : IReporteRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public ReporteRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<PresupuestoReportDto> GetPresupuestoAsync(
        Guid usuarioId,
        DateTime fechaInicio,
        DateTime fechaFin,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            -- 1. Totales del período
            SELECT
                (SELECT COALESCE(SUM(importe), 0) FROM ingresos
                 WHERE id_usuario = @UsuarioId AND fecha BETWEEN @Desde AND @Hasta) AS TotalIngresos,
                (SELECT COALESCE(SUM(importe), 0) FROM gastos
                 WHERE id_usuario = @UsuarioId AND fecha BETWEEN @Desde AND @Hasta) AS TotalGastos;

            -- 2. Ingresos: detalle de cada movimiento (se agrega en memoria por categoría → concepto)
            SELECT cat.nombre AS Categoria, con.nombre AS Concepto, cta.nombre AS Cuenta,
                   i.fecha AS Fecha, i.descripcion AS Descripcion, i.importe AS Importe
            FROM ingresos i
            INNER JOIN conceptos con ON i.id_concepto = con.id
            INNER JOIN categorias cat ON con.id_categoria = cat.id
            INNER JOIN cuentas cta ON i.id_cuenta = cta.id
            WHERE i.id_usuario = @UsuarioId AND i.fecha BETWEEN @Desde AND @Hasta
            ORDER BY cat.nombre, con.nombre, i.fecha;

            -- 3. Gastos: detalle de cada movimiento
            SELECT cat.nombre AS Categoria, con.nombre AS Concepto, cta.nombre AS Cuenta,
                   g.fecha AS Fecha, g.descripcion AS Descripcion, g.importe AS Importe
            FROM gastos g
            INNER JOIN conceptos con ON g.id_concepto = con.id
            INNER JOIN categorias cat ON con.id_categoria = cat.id
            INNER JOIN cuentas cta ON g.id_cuenta = cta.id
            WHERE g.id_usuario = @UsuarioId AND g.fecha BETWEEN @Desde AND @Hasta
            ORDER BY cat.nombre, con.nombre, g.fecha;

            -- 4. Desglose por cuenta (ingresos y gastos)
            SELECT cta.nombre AS Cuenta,
                   COALESCE(SUM(CASE WHEN t.tipo = 'I' THEN t.importe END), 0) AS Ingresos,
                   COALESCE(SUM(CASE WHEN t.tipo = 'G' THEN t.importe END), 0) AS Gastos
            FROM (
                SELECT id_cuenta, importe, 'I' AS tipo FROM ingresos
                WHERE id_usuario = @UsuarioId AND fecha BETWEEN @Desde AND @Hasta
                UNION ALL
                SELECT id_cuenta, importe, 'G' AS tipo FROM gastos
                WHERE id_usuario = @UsuarioId AND fecha BETWEEN @Desde AND @Hasta
            ) t
            INNER JOIN cuentas cta ON t.id_cuenta = cta.id
            GROUP BY cta.id, cta.nombre
            ORDER BY cta.nombre;

            -- 5. Desglose por forma de pago (ingresos y gastos)
            SELECT fp.nombre AS FormaPago,
                   COALESCE(SUM(CASE WHEN t.tipo = 'I' THEN t.importe END), 0) AS Ingresos,
                   COALESCE(SUM(CASE WHEN t.tipo = 'G' THEN t.importe END), 0) AS Gastos
            FROM (
                SELECT id_forma_pago, importe, 'I' AS tipo FROM ingresos
                WHERE id_usuario = @UsuarioId AND fecha BETWEEN @Desde AND @Hasta
                UNION ALL
                SELECT id_forma_pago, importe, 'G' AS tipo FROM gastos
                WHERE id_usuario = @UsuarioId AND fecha BETWEEN @Desde AND @Hasta
            ) t
            INNER JOIN formas_pago fp ON t.id_forma_pago = fp.id
            GROUP BY fp.id, fp.nombre
            ORDER BY fp.nombre;

            -- 6. Resumen por mes (ingresos y gastos agregados)
            SELECT t.Anio, t.Mes,
                   COALESCE(SUM(CASE WHEN t.tipo = 'I' THEN t.Total END), 0) AS Ingresos,
                   COALESCE(SUM(CASE WHEN t.tipo = 'G' THEN t.Total END), 0) AS Gastos
            FROM (
                SELECT YEAR(fecha) AS Anio, MONTH(fecha) AS Mes, 'I' AS tipo, SUM(importe) AS Total
                FROM ingresos
                WHERE id_usuario = @UsuarioId AND fecha BETWEEN @Desde AND @Hasta
                GROUP BY YEAR(fecha), MONTH(fecha)
                UNION ALL
                SELECT YEAR(fecha) AS Anio, MONTH(fecha) AS Mes, 'G' AS tipo, SUM(importe) AS Total
                FROM gastos
                WHERE id_usuario = @UsuarioId AND fecha BETWEEN @Desde AND @Hasta
                GROUP BY YEAR(fecha), MONTH(fecha)
            ) t
            GROUP BY t.Anio, t.Mes
            ORDER BY t.Anio, t.Mes;";

        var parametros = new { UsuarioId = usuarioId, Desde = fechaInicio, Hasta = fechaFin };
        var command = new CommandDefinition(sql, parametros, cancellationToken: cancellationToken);

        using var connection = _dbConnectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(command);

        var totales = await multi.ReadSingleAsync<TotalesRow>();
        var ingresosRaw = (await multi.ReadAsync<DetalleRow>()).ToList();
        var gastosRaw = (await multi.ReadAsync<DetalleRow>()).ToList();
        var cuentas = (await multi.ReadAsync<CuentaReporteDto>()).ToList();
        var formasPago = (await multi.ReadAsync<FormaPagoReporteDto>()).ToList();
        var mensualRaw = (await multi.ReadAsync<ResumenMensualDto>()).ToList();

        return new PresupuestoReportDto
        {
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            TotalIngresos = totales.TotalIngresos,
            TotalGastos = totales.TotalGastos,
            ResumenMensual = ConstruirResumenMensual(mensualRaw, fechaInicio, fechaFin),
            Ingresos = AgruparPorCategoria(ingresosRaw),
            Gastos = AgruparPorCategoria(gastosRaw),
            Cuentas = cuentas,
            FormasPago = formasPago
        };
    }

    /// <summary>
    /// Rellena todos los meses del período (incluidos los que no tuvieron movimientos, con 0)
    /// para que el resumen mensual muestre la evolución completa, no solo los meses con datos.
    /// </summary>
    private static List<ResumenMensualDto> ConstruirResumenMensual(
        List<ResumenMensualDto> conDatos, DateTime desde, DateTime hasta)
    {
        var porMes = conDatos.ToDictionary(m => (m.Anio, m.Mes));
        var resultado = new List<ResumenMensualDto>();

        var cursor = new DateTime(desde.Year, desde.Month, 1);
        var fin = new DateTime(hasta.Year, hasta.Month, 1);

        while (cursor <= fin)
        {
            resultado.Add(porMes.TryGetValue((cursor.Year, cursor.Month), out var mes)
                ? mes
                : new ResumenMensualDto { Anio = cursor.Year, Mes = cursor.Month, Ingresos = 0, Gastos = 0 });

            cursor = cursor.AddMonths(1);
        }

        return resultado;
    }

    /// <summary>
    /// Convierte las filas de detalle en la jerarquía categoría → concepto → movimientos,
    /// calculando los totales y el número de movimientos por agregación en memoria.
    /// </summary>
    private static List<CategoriaReporteDto> AgruparPorCategoria(List<DetalleRow> filas)
    {
        return filas
            .GroupBy(f => f.Categoria)
            .Select(gCat => new CategoriaReporteDto
            {
                Categoria = gCat.Key,
                Total = gCat.Sum(f => f.Importe),
                Conceptos = gCat
                    .GroupBy(f => f.Concepto)
                    .Select(gCon => new ConceptoReporteDto
                    {
                        Concepto = gCon.Key,
                        Total = gCon.Sum(f => f.Importe),
                        Cantidad = gCon.Count(),
                        Movimientos = gCon
                            .Select(f => new MovimientoDetalleReporteDto
                            {
                                Fecha = f.Fecha,
                                Descripcion = f.Descripcion ?? string.Empty,
                                Cuenta = f.Cuenta,
                                Importe = f.Importe
                            })
                            .OrderBy(m => m.Fecha)
                            .ToList()
                    })
                    .OrderByDescending(c => c.Total)
                    .ToList()
            })
            .OrderByDescending(c => c.Total)
            .ToList();
    }

    // Records no-posicionales (con init): Dapper los mapea por nombre de columna con coerción de tipos.
    private sealed record TotalesRow
    {
        public decimal TotalIngresos { get; init; }
        public decimal TotalGastos { get; init; }
    }

    private sealed record DetalleRow
    {
        public string Categoria { get; init; } = string.Empty;
        public string Concepto { get; init; } = string.Empty;
        public string Cuenta { get; init; } = string.Empty;
        public DateTime Fecha { get; init; }
        public string? Descripcion { get; init; }
        public decimal Importe { get; init; }
    }
}
