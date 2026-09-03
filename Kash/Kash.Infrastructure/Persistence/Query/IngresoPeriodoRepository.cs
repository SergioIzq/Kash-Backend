using Dapper;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Infrastructure.Kernel.Persistence;
using ApplicationInterface = Kash.Application.Interfaces;

namespace Kash.Infrastructure.Persistence.Query;

public sealed class IngresoPeriodoRepository : ApplicationInterface.IIngresoPeriodoRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public IngresoPeriodoRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<PeriodoResult<IngresoDto>> GetPagedByPeriodoAsync(
        Guid usuarioId,
        DateTime fechaInicio,
        DateTime fechaFin,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string whereClause = "i.id_usuario = @UsuarioId AND i.fecha BETWEEN @FechaInicio AND @FechaFin";

        var parametros = new DynamicParameters();
        parametros.Add("UsuarioId", usuarioId);
        parametros.Add("FechaInicio", fechaInicio);
        parametros.Add("FechaFin", fechaFin);

        var totalesSql = $@"
            SELECT COUNT(*) AS TotalCount, COALESCE(SUM(i.importe), 0) AS SumaImporte
            FROM ingresos i
            WHERE {whereClause}";

        var totales = await connection.QuerySingleAsync<TotalesPeriodo>(
            new CommandDefinition(totalesSql, parametros, cancellationToken: cancellationToken));

        parametros.Add("PageSize", pageSize);
        parametros.Add("Offset", (page - 1) * pageSize);

        var sql = $@"
            SELECT
                i.id as Id,
                i.importe as Importe,
                i.fecha as Fecha,
                i.descripcion as Descripcion,
                i.id_concepto as ConceptoId,
                COALESCE(c.nombre, '') as ConceptoNombre,
                cat.id as CategoriaId,
                cat.nombre as CategoriaNombre,
                i.id_cliente as ClienteId,
                COALESCE(cli.nombre, '') as ClienteNombre,
                i.id_persona as PersonaId,
                COALESCE(p.nombre, '') as PersonaNombre,
                i.id_cuenta as CuentaId,
                COALESCE(cta.nombre, '') as CuentaNombre,
                i.id_forma_pago as FormaPagoId,
                COALESCE(fp.nombre, '') as FormaPagoNombre,
                i.id_usuario as UsuarioId
            FROM ingresos i
            LEFT JOIN conceptos c ON i.id_concepto = c.id
            LEFT JOIN categorias cat ON c.id_categoria = cat.id
            LEFT JOIN clientes cli ON i.id_cliente = cli.id
            LEFT JOIN personas p ON i.id_persona = p.id
            LEFT JOIN cuentas cta ON i.id_cuenta = cta.id
            LEFT JOIN formas_pago fp ON i.id_forma_pago = fp.id
            WHERE {whereClause}
            ORDER BY i.fecha DESC, i.id DESC
            LIMIT @PageSize OFFSET @Offset";

        var items = await connection.QueryAsync<IngresoDto>(
            new CommandDefinition(sql, parametros, cancellationToken: cancellationToken));

        var pagina = new PagedList<IngresoDto>(items.ToList(), page, pageSize, (int)totales.TotalCount);

        return new PeriodoResult<IngresoDto>(pagina, totales.SumaImporte);
    }

    // COUNT(*) llega como BIGINT/Int64 de MySQL; Dapper exige coincidencia exacta de tipos
    // al materializar un record por constructor (a diferencia de ExecuteScalarAsync<int>,
    // que sí convierte implícitamente).
    private sealed record TotalesPeriodo(long TotalCount, decimal SumaImporte);
}
