using Kash.Domain;
using Kash.Infrastructure.Persistence.Query;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;
using Dapper;

namespace Kash.Infrastructure.Persistence.Data.Personas
{
    public class PersonaReadRepository : AbsReadRepository<Persona, PersonaDto, PersonaId>, IPersonaReadRepository
    {
        public PersonaReadRepository(IDbConnectionFactory dbConnectionFactory)
            : base(dbConnectionFactory)
        {
        }

        /// <summary>
        /// 🔥 ÚNICA CONFIGURACIÓN REQUERIDA: Define todas las características del repositorio.
        /// </summary>
        protected override ReadRepositoryConfiguration ConfigureRepository()
        {
            return ReadRepositoryConfiguration.Simple(
                tableName: "personas",
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
          SELECT COUNT(1) 
      FROM personas 
   WHERE nombre = @Nombre AND id_usuario = @UsuarioId";

            var count = await connection.ExecuteScalarAsync<int>(
            sql,
         new { Nombre = nombre.Value, UsuarioId = usuarioId.Value });

            return count > 0;
        }

        public async Task<bool> ExistsWithSameNameExceptAsync(Nombre nombre, UsuarioId usuarioId, Guid excludeId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
          SELECT COUNT(1) 
       FROM personas 
   WHERE nombre = @Nombre AND id_usuario = @UsuarioId AND id != @ExcludeId";

            var count = await connection.ExecuteScalarAsync<int>(
            sql,
            new { Nombre = nombre.Value, UsuarioId = usuarioId.Value, ExcludeId = excludeId });

            return count > 0;
        }
    }
}