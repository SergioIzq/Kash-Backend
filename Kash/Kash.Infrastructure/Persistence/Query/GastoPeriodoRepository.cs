using Dapper;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Infrastructure.Kernel.Persistence;
using ApplicationInterface = Kash.Application.Interfaces;

namespace Kash.Infrastructure.Persistence.Query;

public sealed class GastoPeriodoRepository : ApplicationInterface.IGastoPeriodoRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GastoPeriodoRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<PeriodoResult<GastoDto>> GetPagedByPeriodoAsync(
        Guid usuarioId,
        DateTime fechaInicio,
        DateTime fechaFin,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string whereClause = "g.id_usuario = @UsuarioId AND g.fecha BETWEEN @FechaInicio AND @FechaFin";

        var parametros = new DynamicParameters();
        parametros.Add("UsuarioId", usuarioId);
        parametros.Add("FechaInicio", fechaInicio);
        parametros.Add("FechaFin", fechaFin);

        var totalesSql = $@"
            SELECT COUNT(*) AS TotalCount, COALESCE(SUM(g.importe), 0) AS SumaImporte
            FROM gastos g
            WHERE {whereClause}";

        var totales = await connection.QuerySingleAsync<TotalesPeriodo>(
            new CommandDefinition(totalesSql, parametros, cancellationToken: cancellationToken));

        parametros.Add("PageSize", pageSize);
        parametros.Add("Offset", (page - 1) * pageSize);

        var sql = $@"
            SELECT
                g.id as Id,
                g.importe as Importe,
                g.fecha as Fecha,
                g.descripcion as Descripcion,
                g.id_concepto as ConceptoId,
                COALESCE(c.nombre, '') as ConceptoNombre,
                c.id_categoria as CategoriaId,
                cat.nombre as CategoriaNombre,
                g.id_proveedor as ProveedorId,
                prov.nombre as ProveedorNombre,
                g.id_persona as PersonaId,
                p.nombre as PersonaNombre,
                g.id_cuenta as CuentaId,
                COALESCE(cta.nombre, '') as CuentaNombre,
                g.id_forma_pago as FormaPagoId,
                COALESCE(fp.nombre, '') as FormaPagoNombre,
                g.id_usuario as UsuarioId
            FROM gastos g
            LEFT JOIN conceptos c ON g.id_concepto = c.id
            LEFT JOIN categorias cat ON c.id_categoria = cat.id
            LEFT JOIN proveedores prov ON g.id_proveedor = prov.id
            LEFT JOIN personas p ON g.id_persona = p.id
            LEFT JOIN cuentas cta ON g.id_cuenta = cta.id
            LEFT JOIN formas_pago fp ON g.id_forma_pago = fp.id
            WHERE {whereClause}
            ORDER BY g.fecha DESC, g.id DESC
            LIMIT @PageSize OFFSET @Offset";

        var items = await connection.QueryAsync<GastoDto>(
            new CommandDefinition(sql, parametros, cancellationToken: cancellationToken));

        var pagina = new PagedList<GastoDto>(items.ToList(), page, pageSize, (int)totales.TotalCount);

        return new PeriodoResult<GastoDto>(pagina, totales.SumaImporte);
    }

    // COUNT(*) llega como BIGINT/Int64 de MySQL; Dapper exige coincidencia exacta de tipos
    // al materializar un record por constructor (a diferencia de ExecuteScalarAsync<int>,
    // que sí convierte implícitamente).
    private sealed record TotalesPeriodo(long TotalCount, decimal SumaImporte);
}
