using Kash.Domain;
using SergioIzq.Infrastructure.Kernel.Persistence;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;
using Dapper;

namespace Kash.Infrastructure.Persistence.Data.Categorias
{
    public class CategoriaReadRepository : AbsReadRepository<Categoria, CategoriaDto, CategoriaId>, ICategoriaReadRepository
    {
        public CategoriaReadRepository(IDbConnectionFactory dbConnectionFactory)
            : base(dbConnectionFactory)
        {
        }

        /// <summary>
        /// ÚNICA CONFIGURACIÓN REQUERIDA: Define todas las características del repositorio.
        /// </summary>
        protected override ReadRepositoryConfiguration ConfigureRepository()
        {
            return ReadRepositoryConfiguration.Simple(
                tableName: "categorias",
                selectColumns: new List<string>
                {
                    "id as Id",
                    "nombre as Nombre",
                    "descripcion as Descripcion",
                    "id_usuario as UsuarioId",
                    "fecha_creacion as FechaCreacion"
                },
                searchableColumns: new List<string>
                {
                    "nombre",
                    "descripcion"
                },
                sortableColumns: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Nombre", "nombre" },
                    { "FechaCreacion", "fecha_creacion" }
                },
                defaultOrderBy: "nombre ASC"
            );
        }

        public async Task<bool> ExistsWithSameNameAsync(Nombre nombre, UsuarioId usuarioId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
        SELECT EXISTS(
      SELECT 1 
   FROM categorias 
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
    FROM categorias 
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
