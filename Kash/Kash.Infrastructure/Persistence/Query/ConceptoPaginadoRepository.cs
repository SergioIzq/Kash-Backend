using Dapper;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Infrastructure.Kernel.Persistence;
using ApplicationInterface = Kash.Application.Interfaces;

namespace Kash.Infrastructure.Persistence.Query;

public sealed class ConceptoPaginadoRepository : ApplicationInterface.IConceptoPaginadoRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    // Mismas columnas sortables que ConceptoReadRepository.ConfigureRepository(), para no abrir
    // el ORDER BY a una columna arbitraria enviada por query string.
    private static readonly Dictionary<string, string> SortableColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Nombre", "c.nombre" },
        { "CategoriaNombre", "cat.nombre" },
        { "FechaCreacion", "c.fecha_creacion" }
    };

    private const string DefaultOrderBy = "c.nombre ASC";

    public ConceptoPaginadoRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<PagedList<ConceptoDto>> GetPagedByCategoriaAsync(
        Guid usuarioId,
        Guid categoriaId,
        int page,
        int pageSize,
        string? searchTerm,
        string? sortColumn,
        string? sortOrder,
        CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var orderByColumn = !string.IsNullOrWhiteSpace(sortColumn) && SortableColumns.TryGetValue(sortColumn, out var mappedColumn)
            ? mappedColumn
            : "c.nombre";
        var orderByDirection = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
        var orderBy = string.IsNullOrWhiteSpace(sortColumn) ? DefaultOrderBy : $"{orderByColumn} {orderByDirection}";

        var parameters = new
        {
            UsuarioId = usuarioId,
            CategoriaId = categoriaId,
            SearchTerm = searchTerm ?? "",
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        };

        const string whereClause = @"
            WHERE c.id_usuario = @UsuarioId
              AND c.id_categoria = @CategoriaId
              AND (@SearchTerm = '' OR c.nombre LIKE CONCAT('%', @SearchTerm, '%'))";

        var countSql = $@"
            SELECT COUNT(*)
            FROM conceptos c
            {whereClause}";

        var itemsSql = $@"
            SELECT
                c.id as Id,
                c.nombre as Nombre,
                c.id_categoria as CategoriaId,
                COALESCE(cat.nombre, '') as CategoriaNombre,
                c.id_usuario as UsuarioId
            FROM conceptos c
            LEFT JOIN categorias cat ON c.id_categoria = cat.id
            {whereClause}
            ORDER BY {orderBy}
            LIMIT @PageSize OFFSET @Offset";

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));

        var items = await connection.QueryAsync<ConceptoDto>(
            new CommandDefinition(itemsSql, parameters, cancellationToken: cancellationToken));

        return new PagedList<ConceptoDto>(items.ToList(), page, pageSize, totalCount);
    }
}
