using AhorroLand.Infrastructure.Configuration.Settings;
using AhorroLand.Infrastructure.DataAccess;
using AhorroLand.Infrastructure.Persistence.Command;
using AhorroLand.Infrastructure.Persistence.Interceptors;
using AhorroLand.Infrastructure.Persistence.Query;
using AhorroLand.Infrastructure.Persistence.Warmup;
using AhorroLand.Infrastructure.Services;
using AhorroLand.Infrastructure.Services.Auth;
using AhorroLand.Shared.Application.Abstractions.Services;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using System.Data;
using System.Reflection;
using ApplicationInterface = AhorroLand.Application.Interfaces;

namespace AhorroLand.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
           IConfiguration configuration)
        {
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 43));
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Registrar interceptor de eventos de dominio (necesario para AhorroLandDbContext)
            services.AddScoped<DomainEventDispatcherInterceptor>();

            // 1️⃣ DbContext con optimizaciones de rendimiento
            services.AddDbContext<AhorroLandDbContext>(options =>
         {
             options.UseMySql(connectionString, serverVersion, mySqlOptions =>
       {
           // 🔥 OPTIMIZACIÓN 1: Query Splitting para evitar cartesian explosion
           mySqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);

           // 🔥 OPTIMIZACIÓN 2: Batch commands para mejor rendimiento
           mySqlOptions.MaxBatchSize(100);

           // 🔥 OPTIMIZACIÓN 3: Command timeout
           mySqlOptions.CommandTimeout(30);

           // 🔥 OPTIMIZACIÓN 4: Connection resilience (retry on failure)
           mySqlOptions.EnableRetryOnFailure(
       maxRetryCount: 3,
     maxRetryDelay: TimeSpan.FromSeconds(5),
    errorNumbersToAdd: null);
       });

             // 🔥 OPTIMIZACIÓN 5: Compiled queries caching
             options.EnableThreadSafetyChecks(false); // Solo en producción si estás seguro
         });

            // 2️⃣ Cache distribuida (MemoryCache para desarrollo)
            services.AddDistributedMemoryCache();

            // 3️⃣ Dapper con connection pooling
            services.AddScoped<IDbConnection>(sp =>
             new MySqlConnection(configuration.GetConnectionString("DefaultConnection")));

            // 4️⃣ Email settings
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            // 5️⃣ Registro explícito de dependencias críticas
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 📧 Servicios de Email (Background + Queue)
            services.AddSingleton<QueuedEmailService>();
            services.AddSingleton<IEmailService>(sp => sp.GetRequiredService<QueuedEmailService>());
            services.AddHostedService<EmailBackgroundSender>();

            // 🔐 Servicios de autenticación
            services.AddScoped<IPasswordHasher, PasswordHasherService>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            // 📊 Repositorio de Dashboard (registrado explícitamente para ambas interfaces)
            services.AddScoped<DashboardRepository>();
            services.AddScoped<ApplicationInterface.IDashboardRepository>(sp => sp.GetRequiredService<DashboardRepository>());
            services.AddScoped<IDashboardRepository>(sp => sp.GetRequiredService<DashboardRepository>());

            // 👉 Registro automático de repositorios de ESCRITURA
            services.Scan(scan => scan
         .FromAssemblies(Assembly.GetExecutingAssembly())
  .AddClasses(classes => classes.AssignableTo(typeof(IWriteRepository<,>)))
        .AsImplementedInterfaces()
              .WithScopedLifetime()
        );

            // 👉 🔧 FIX: Registro automático de repositorios de LECTURA
            // Registra implementaciones de IReadRepositoryWithDto<T, TDto>
            services.Scan(scan => scan
           .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes
           .Where(type => type.GetInterfaces()
           .Any(i => i.IsGenericType &&
                 i.GetGenericTypeDefinition() == typeof(IReadRepositoryWithDto<,,>))))
                 .AsImplementedInterfaces()
           .WithScopedLifetime()
          );

            services.AddScoped<IDomainValidator, DapperDomainValidator>();

            // 6️⃣ Registro automático de servicios con Scrutor
            services.Scan(scan => scan
                .FromAssemblies(Assembly.GetExecutingAssembly())
                    .AddClasses(classes => classes
                .InNamespaces("AhorroLand.Infrastructure.Services")
                 .Where(c => !typeof(BackgroundService).IsAssignableFrom(c)
                && c != typeof(QueuedEmailService) // Ya registrado arriba
          && c.GetInterfaces().Length > 0)
             )
                .AsImplementedInterfaces()
                       .WithScopedLifetime()
                   );

            services.AddScoped<IDbConnectionFactory, SqlDbConnectionFactory>();

            // 🔥 Warm-up de conexiones al iniciar
            services.AddHostedService<DatabaseWarmupService>();

            return services;
        }
    }
}
