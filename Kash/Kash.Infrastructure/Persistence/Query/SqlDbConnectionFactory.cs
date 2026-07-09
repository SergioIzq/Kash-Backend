using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Data;

namespace Kash.Infrastructure.Persistence.Query;

public class SqlDbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlDbConnectionFactory(IConfiguration configuration)
    {
        var baseConnectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // OPTIMIZACIÓN: Configurar pool de conexiones MySQL para mejor rendimiento
        var builder = new MySqlConnectionStringBuilder(baseConnectionString)
        {
            // Pool de conexiones optimizado
            Pooling = true,
            MinimumPoolSize = 5,          // Mínimo de conexiones siempre disponibles
            MaximumPoolSize = 100,        // Máximo de conexiones (ajustar según carga)

            // Timeouts optimizados
            ConnectionTimeout = 15,       // 15 segundos para establecer conexión
            DefaultCommandTimeout = 30,   // 30 segundos para comandos SQL

            // Optimizaciones de red
            Keepalive = 60,              // Mantener conexión viva (ping cada 60 segundos)
            AllowUserVariables = true,    // Permitir variables de usuario (útil para Dapper)

            // Charset y collation
            CharacterSet = "utf8mb4",     // Soporte completo de Unicode

            // Compresión (solo si la latencia de red es alta)
            UseCompression = false        // Desactivado por defecto (CPU vs Network tradeoff)
        };

        _connectionString = builder.ConnectionString;
    }

    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }
}
