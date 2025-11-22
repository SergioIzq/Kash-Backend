using AhorroLand.Domain;
using AhorroLand.Infrastructure.Persistence.Query;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.ValueObjects;
using Dapper;

namespace AhorroLand.Infrastructure.Persistence.Data.Clientes
{
    /// <summary>
    /// Repositorio de lectura optimizado para Clientes.
    /// ✅ Incluye filtro por usuario para aprovechar índices de base de datos.
    /// </summary>
    public class ClienteReadRepository : AbsReadRepository<Cliente, ClienteDto>, IClienteReadRepository
    {
        public ClienteReadRepository(IDbConnectionFactory dbConnectionFactory)
   : base(dbConnectionFactory, "clientes")
        {
        }

        /// <summary>
      /// 🔥 OPTIMIZADO: Query específico para Cliente con las columnas correctas.
        /// </summary>
  protected override string BuildGetByIdQuery()
     {
   return @"
       SELECT 
       id as Id,
      nombre as Nombre,
         usuario_id as UsuarioId
   FROM clientes 
      WHERE id = @id";
        }

        /// <summary>
        /// 🔥 OPTIMIZADO: Query para obtener todos los clientes.
        /// </summary>
   protected override string BuildGetAllQuery()
 {
     return @"
    SELECT 
          id as Id,
   nombre as Nombre,
   usuario_id as UsuarioId
           FROM clientes";
  }

   /// <summary>
      /// 🔥 OPTIMIZADO: Query base para paginación (sin ORDER BY).
     /// El ORDER BY se agrega en cada método según el contexto.
        /// </summary>
        protected override string BuildGetPagedQuery()
     {
   return @"
   SELECT 
       id as Id,
             nombre as Nombre,
      usuario_id as UsuarioId
   FROM clientes";
     }

        /// <summary>
        /// 🔥 OPTIMIZADO: Query de conteo.
        /// </summary>
protected override string BuildCountQuery()
  {
       return "SELECT COUNT(*) FROM clientes";
        }

        /// <summary>
    /// 🔥 NUEVO: Proporciona el ORDER BY por defecto para paginación sin filtros.
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
       { "Nombre", "nombre" }
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

            // 🚀 OPTIMIZACIÓN: EXISTS es más rápido que COUNT para verificación de existencia
      const string sql = @"
       SELECT EXISTS(
      SELECT 1 
       FROM clientes 
       WHERE nombre = @Nombre AND usuario_id = @UsuarioId
                ) as Exists";

            // 🔧 OPTIMIZACIÓN: Dapper maneja GUIDs nativamente
          var exists = await connection.ExecuteScalarAsync<bool>(
   new CommandDefinition(sql,
         new { Nombre = nombre.Value, UsuarioId = usuarioId.Value },
    cancellationToken: cancellationToken));

      return exists;
}

public async Task<bool> ExistsWithSameNameExceptAsync(Nombre nombre, UsuarioId usuarioId, Guid excludeId, CancellationToken cancellationToken = default)
  {
using var connection = _dbConnectionFactory.CreateConnection();

       // 🚀 OPTIMIZACIÓN: EXISTS es más rápido que COUNT
            const string sql = @"
                SELECT EXISTS(
             SELECT 1 
   FROM clientes 
      WHERE nombre = @Nombre 
AND usuario_id = @UsuarioId 
       AND id != @ExcludeId
     ) as Exists";

       // 🔧 OPTIMIZACIÓN: Dapper maneja GUIDs nativamente
            var exists = await connection.ExecuteScalarAsync<bool>(
  new CommandDefinition(sql,
              new { Nombre = nombre.Value, UsuarioId = usuarioId.Value, ExcludeId = excludeId },
              cancellationToken: cancellationToken));

   return exists;
    }
    }
}