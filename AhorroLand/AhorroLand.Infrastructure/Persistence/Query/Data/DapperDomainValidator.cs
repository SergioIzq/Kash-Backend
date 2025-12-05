using AhorroLand.Infrastructure.Persistence.Query;
using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.Interfaces;
using Dapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace AhorroLand.Infrastructure.DataAccess;

public class DapperDomainValidator : IDomainValidator
{
    private readonly IDbConnectionFactory _connectionFactory;

    // ✅ CAMBIO 1: Inyectamos el Factory, no la conexión directa
    public DapperDomainValidator(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> ExistsAsync<TEntity, TId>(TId id)
        where TEntity : AbsEntity<TId>
        where TId : IGuidValueObject
    {
        // ✅ CAMBIO 2: Creamos la conexión bajo demanda (Pattern Factory)
        using var connection = _connectionFactory.CreateConnection();

        // Dapper abre la conexión automáticamente si está cerrada, 
        // pero con Factory a veces es buena práctica ser explícito o dejar que Execute lo haga.
        // connection.Open(); 

        // 1. Obtener tabla
        var tableName = GetTableName<TEntity>();

        // ✅ CAMBIO 3 (Optimización): 
        // Como TId implementa IGuidValueObject, no necesitamos Reflection lento.
        // Accedemos directamente a la propiedad definida en la interfaz.
        var realIdValue = id.Value;

        // 2. Query optimizada (SELECT 1 es más rápido que COUNT(*))
        var sql = $"SELECT 1 FROM {tableName} WHERE id = @Id LIMIT 1";

        // 3. Ejecutar
        var result = await connection.ExecuteScalarAsync<int?>(sql, new { Id = realIdValue });

        return result.HasValue;
    }

    // --- Métodos Privados ---

    private static string GetTableName<TEntity>()
    {
        var type = typeof(TEntity);
        var tableAttr = type.GetCustomAttribute<TableAttribute>();

        if (tableAttr != null && !string.IsNullOrEmpty(tableAttr.Name))
        {
            return tableAttr.Name;
        }
        // Fallback: pluralización simple si no hay atributo [Table]
        return type.Name.ToLower() + "s";
    }
}