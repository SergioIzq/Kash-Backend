using Dapper;
using Kash.Shared.Application.Dtos;
using SergioIzq.Infrastructure.Kernel.Persistence;
using ApplicationInterface = Kash.Application.Interfaces;

namespace Kash.Infrastructure.Persistence.Query;

public sealed class IngresoExportRepository : ApplicationInterface.IIngresoExportRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    // Mismas searchableColumns que IngresoReadRepository.ConfigureRepository(), para que el
    // texto de búsqueda de la exportación encuentre lo mismo que el buscador del listado.
    private static readonly string[] SearchableColumns = ["i.descripcion", "c.nombre", "cat.nombre", "cli.nombre", "p.nombre", "cta.nombre"];

    public IngresoExportRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<IReadOnlyList<IngresoDto>> GetForExportAsync(
        Guid usuarioId,
        ApplicationInterface.IngresoExportFiltro filtro,
        CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var where = new List<string> { "i.id_usuario = @UsuarioId" };
        var parametros = new DynamicParameters();
        parametros.Add("UsuarioId", usuarioId);

        if (filtro.FechaInicio.HasValue && filtro.FechaFin.HasValue)
        {
            where.Add("i.fecha BETWEEN @FechaInicio AND @FechaFin");
            parametros.Add("FechaInicio", filtro.FechaInicio.Value);
            parametros.Add("FechaFin", filtro.FechaFin.Value);
        }

        if (filtro.ConceptoIds is { Length: > 0 })
        {
            where.Add("i.id_concepto IN @ConceptoIds");
            parametros.Add("ConceptoIds", filtro.ConceptoIds);
        }

        if (filtro.CategoriaIds is { Length: > 0 })
        {
            where.Add("c.id_categoria IN @CategoriaIds");
            parametros.Add("CategoriaIds", filtro.CategoriaIds);
        }

        if (filtro.ClienteIds is { Length: > 0 })
        {
            where.Add("i.id_cliente IN @ClienteIds");
            parametros.Add("ClienteIds", filtro.ClienteIds);
        }

        if (filtro.PersonaIds is { Length: > 0 })
        {
            where.Add("i.id_persona IN @PersonaIds");
            parametros.Add("PersonaIds", filtro.PersonaIds);
        }

        if (!string.IsNullOrWhiteSpace(filtro.SearchTerm))
        {
            var busqueda = string.Join(" OR ", SearchableColumns.Select(columna => $"{columna} LIKE @SearchTerm"));
            where.Add($"({busqueda})");
            parametros.Add("SearchTerm", $"%{filtro.SearchTerm}%");
        }

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
            WHERE {string.Join(" AND ", where)}
            ORDER BY i.fecha DESC, i.id DESC";

        var resultados = await connection.QueryAsync<IngresoDto>(
            new CommandDefinition(sql, parametros, cancellationToken: cancellationToken));

        return resultados.ToList();
    }
}
