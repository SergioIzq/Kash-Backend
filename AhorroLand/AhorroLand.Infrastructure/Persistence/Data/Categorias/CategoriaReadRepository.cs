using AhorroLand.Domain;
using AhorroLand.Infrastructure.Persistence.Query;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.ValueObjects;
using Dapper;

namespace AhorroLand.Infrastructure.Persistence.Data.Categorias
{
    public class CategoriaReadRepository : AbsReadRepository<Categoria, CategoriaDto>, ICategoriaReadRepository
    {
        public CategoriaReadRepository(IDbConnectionFactory dbConnectionFactory)
   : base(dbConnectionFactory, "categorias")
        {
        }

        /// <summary>
        /// 🔥 Query específico para Categoría con todas sus columnas.
        /// </summary>
 protected override string BuildGetByIdQuery()
        {
       return @"
            SELECT 
          id as Id,
nombre as Nombre,
  id_usuario as UsuarioId,
       fecha_creacion as FechaCreacion
            FROM categorias 
            WHERE id = @id";
        }

  /// <summary>
        /// 🔥 Query para obtener todas las categorías.
   /// </summary>
 protected override string BuildGetAllQuery()
        {
   return @"
   SELECT 
     id as Id,
      nombre as Nombre,
       id_usuario as UsuarioId,
       fecha_creacion as FechaCreacion
            FROM categorias";
     }

        /// <summary>
        /// 🔥 ORDER BY por nombre ascendente.
    /// </summary>
      protected override string GetDefaultOrderBy()
 {
    return "ORDER BY nombre ASC";
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
        SELECT EXISTS(
      SELECT 1 
   FROM categorias 
     WHERE nombre = @Nombre AND id_usuario = @UsuarioId
      ) as Exists";

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
            ) as Exists";

            var exists = await connection.ExecuteScalarAsync<bool>(
  new CommandDefinition(sql,
       new { Nombre = nombre.Value, UsuarioId = usuarioId.Value, ExcludeId = excludeId },
  cancellationToken: cancellationToken));

         return exists;
   }
    }
}