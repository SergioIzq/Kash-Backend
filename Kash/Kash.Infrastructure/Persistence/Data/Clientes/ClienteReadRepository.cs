using Kash.Domain;
using Kash.Infrastructure.Persistence.Query;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;
using Dapper;

namespace Kash.Infrastructure.Persistence.Data.Clientes
{
    /// <summary>
    /// Repositorio de lectura optimizado para Clientes.
    /// Incluye filtro por usuario para aprovechar índices de base de datos.
    /// </summary>
    public class ClienteReadRepository : AbsReadRepository<Cliente, ClienteDto, ClienteId>, IClienteReadRepository
    {
        public ClienteReadRepository(IDbConnectionFactory dbConnectionFactory)
            : base(dbConnectionFactory)
        {
        }

        /// <summary>
        /// ÚNICA CONFIGURACIÓN REQUERIDA: Define todas las características del repositorio.
        /// </summary>
        protected override ReadRepositoryConfiguration ConfigureRepository()
        {
            return ReadRepositoryConfiguration.Simple(
                tableName: "clientes",
                selectColumns: new List<string>
                {
                    "id as Id",
                    "nombre as Nombre",
                    "id_usuario as UsuarioId",
                    "fecha_creacion as FechaCreacion"
                },
                searchableColumns: new List<string>
                {
                    "nombre"
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
                    FROM clientes 
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
                    FROM clientes 
                    WHERE nombre = @Nombre 
                        AND id_usuario = @UsuarioId 
                        AND id != @ExcludeId
                ) as ItemExists";

            var exists = await connection.ExecuteScalarAsync<bool>(
                new CommandDefinition(sql,
                    new { Nombre = nombre.Value, UsuarioId = usuarioId.Value, ExcludeId = excludeId },
                    cancellationToken: cancellationToken));

            return exists;
        }
    }
}
