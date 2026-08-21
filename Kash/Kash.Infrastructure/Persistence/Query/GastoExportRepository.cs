using Dapper;
using Kash.Shared.Application.Dtos;
using SergioIzq.Infrastructure.Kernel.Persistence;
using ApplicationInterface = Kash.Application.Interfaces;

namespace Kash.Infrastructure.Persistence.Query;

public sealed class GastoExportRepository : ApplicationInterface.IGastoExportRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    // Mismas searchableColumns que GastoReadRepository.ConfigureRepository(), para que el
    // texto de búsqueda de la exportación encuentre lo mismo que el buscador del listado.
    private static readonly string[] SearchableColumns = ["g.descripcion", "c.nombre", "cat.nombre", "prov.nombre", "p.nombre", "cta.nombre"];

    public GastoExportRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<IReadOnlyList<GastoDto>> GetForExportAsync(
        Guid usuarioId,
        ApplicationInterface.GastoExportFiltro filtro,
        CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var where = new List<string> { "g.id_usuario = @UsuarioId" };
        var parametros = new DynamicParameters();
        parametros.Add("UsuarioId", usuarioId);

        if (filtro.FechaInicio.HasValue && filtro.FechaFin.HasValue)
        {
            where.Add("g.fecha BETWEEN @FechaInicio AND @FechaFin");
            parametros.Add("FechaInicio", filtro.FechaInicio.Value);
            parametros.Add("FechaFin", filtro.FechaFin.Value);
        }

        if (filtro.ConceptoIds is { Length: > 0 })
        {
            where.Add("g.id_concepto IN @ConceptoIds");
            parametros.Add("ConceptoIds", filtro.ConceptoIds);
        }

        if (filtro.CategoriaIds is { Length: > 0 })
        {
            where.Add("c.id_categoria IN @CategoriaIds");
            parametros.Add("CategoriaIds", filtro.CategoriaIds);
        }

        if (filtro.ProveedorIds is { Length: > 0 })
        {
            where.Add("g.id_proveedor IN @ProveedorIds");
            parametros.Add("ProveedorIds", filtro.ProveedorIds);
        }

        if (filtro.PersonaIds is { Length: > 0 })
        {
            where.Add("g.id_persona IN @PersonaIds");
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
            WHERE {string.Join(" AND ", where)}
            ORDER BY g.fecha DESC, g.id DESC";

        var resultados = await connection.QueryAsync<GastoDto>(
            new CommandDefinition(sql, parametros, cancellationToken: cancellationToken));

        return resultados.ToList();
    }
}
