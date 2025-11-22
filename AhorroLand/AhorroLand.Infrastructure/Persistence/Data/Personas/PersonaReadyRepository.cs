using AhorroLand.Domain;
using AhorroLand.Infrastructure.Persistence.Query;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.ValueObjects;
using Dapper;

namespace AhorroLand.Infrastructure.Persistence.Data.Personas
{
    public class PersonaReadRepository : AbsReadRepository<Persona, PersonaDto>, IPersonaReadRepository
    {
        public PersonaReadRepository(IDbConnectionFactory dbConnectionFactory)
            : base(dbConnectionFactory, "personas")
        {
        }

        /// <summary>
        /// 🔥 NUEVO: Define las columnas por las que se puede ordenar.
        /// </summary>
        protected override Dictionary<string, string> GetSortableColumns()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Nombre", "nombre" },
                { "FechaCreacion", "fecha_creacion" }
            };
        }

        /// <summary>
        /// 🔥 NUEVO: Define las columnas en las que se puede buscar.
        /// </summary>
        protected override List<string> GetSearchableColumns()
        {
            return new List<string>
            {
                "nombre"
            };
        }

        public async Task<bool> ExistsWithSameNameAsync(Nombre nombre, UsuarioId usuarioId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
               SELECT COUNT(1) 
                 FROM Personas 
                WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId";

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
                 FROM Personas 
                WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId AND Id != @ExcludeId";

            var count = await connection.ExecuteScalarAsync<int>(
                sql,
                new { Nombre = nombre.Value, UsuarioId = usuarioId.Value, ExcludeId = excludeId });

            return count > 0;
        }
    }
}