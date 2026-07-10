using SergioIzq.Infrastructure.Kernel.Persistence;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kash.Infrastructure.Persistence.Warmup
{
    /// <summary>
    /// Servicio que pre-calienta las conexiones a la base de datos al iniciar la aplicación.
    /// Esto reduce la latencia de la primera request eliminando el "cold start".
    /// </summary>
    public class DatabaseWarmupService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseWarmupService> _logger;

        public DatabaseWarmupService(
            IServiceProvider serviceProvider,
            ILogger<DatabaseWarmupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("🔥 Iniciando warm-up de conexiones a base de datos...");

                using var scope = _serviceProvider.CreateScope();
                var connectionFactory = scope.ServiceProvider
                     .GetRequiredService<IDbConnectionFactory>();

                // Abrir y cerrar 5 conexiones para llenar el pool
                var tasks = Enumerable.Range(0, 5).Select(async _ =>
                       {
                           using var connection = connectionFactory.CreateConnection();
                           connection.Open();
                           await connection.QuerySingleAsync<int>("SELECT 1", cancellationToken);
                       });

                await Task.WhenAll(tasks);

                _logger.LogInformation("✅ Warm-up completado. Pool de conexiones listo.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error durante warm-up de conexiones (no crítico)");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
