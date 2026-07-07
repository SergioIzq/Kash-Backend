using Kash.Domain;
using Kash.Infrastructure.Persistence.Query;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;
using Dapper;

namespace Kash.Infrastructure.Persistence.Data.Conceptos
{
    public class ConceptoReadRepository : AbsReadRepository<Concepto, ConceptoDto, ConceptoId>, IConceptoReadRepository
    {
        public ConceptoReadRepository(IDbConnectionFactory dbConnectionFactory)
            : base(dbConnectionFactory)
        {
        }

        /// <summary>
        /// ÚNICA CONFIGURACIÓN REQUERIDA: Define todas las características del repositorio.
        /// </summary>
        protected override ReadRepositoryConfiguration ConfigureRepository()
        {
            return ReadRepositoryConfiguration.WithJoins(
                tableName: "conceptos",
                tableAlias: "c",
                selectColumns: new List<string>
                {
                    "c.id as Id",
                    "c.nombre as Nombre",
                    "c.id_categoria as CategoriaId",
                    "COALESCE(cat.nombre, '') as CategoriaNombre",
                    "c.id_usuario as UsuarioId"
                },
                joinClause: "LEFT JOIN categorias cat ON c.id_categoria = cat.id",
                searchableColumns: new List<string>
                {
                    "c.nombre",
                    "cat.nombre"
                },
                sortableColumns: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Nombre", "c.nombre" },
                    { "CategoriaNombre", "cat.nombre" },
                    { "FechaCreacion", "c.fecha_creacion" }
                },
                defaultOrderBy: "c.nombre ASC"
            );
        }

        public async Task<bool> ExistsWithSameNameAsync(Nombre nombre, UsuarioId usuarioId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
SELECT EXISTS(
    SELECT 1 
    FROM conceptos 
    WHERE nombre = @Nombre AND id_usuario = @UsuarioId
) as ItemExists";

            var exists = await connection.ExecuteScalarAsync<bool>(
   new CommandDefinition(sql,
          new { Nombre = nombre.Value, UsuarioId = usuarioId.Value },
           cancellationToken: cancellationToken));

            return exists;
        }

        public async Task<bool> ExistsWithSameNameExceptAsync(Nombre nombre, UsuarioId usuarioId, Guid excludeId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
SELECT EXISTS(
SELECT 1 
    FROM conceptos 
    WHERE nombre = @Nombre AND id_usuario = @UsuarioId AND id != @ExcludeId
) as ItemExists";

            var exists = await connection.ExecuteScalarAsync<bool>(
      new CommandDefinition(sql,
      new { Nombre = nombre.Value, UsuarioId = usuarioId.Value, ExcludeId = excludeId },
   cancellationToken: cancellationToken));

            return exists;
        }
    }
}
