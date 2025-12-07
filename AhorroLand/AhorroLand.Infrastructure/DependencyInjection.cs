using AhorroLand.Infrastructure.Configuration.Settings;
using AhorroLand.Infrastructure.DataAccess;
using AhorroLand.Infrastructure.Persistence.Command;
using AhorroLand.Infrastructure.Persistence.Interceptors;
using AhorroLand.Infrastructure.Persistence.Query;
using AhorroLand.Infrastructure.Persistence.Warmup;
using AhorroLand.Infrastructure.Services;
using AhorroLand.Infrastructure.Services.Auth;
using AhorroLand.Shared.Application.Abstractions.Services;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Application.Servicies;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
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
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 43));

            services.AddScoped<DomainEventDispatcherInterceptor>();

            services.AddDbContext<AhorroLandDbContext>((sp, options) =>
  {
      var interceptor = sp.GetRequiredService<DomainEventDispatcherInterceptor>();

      options.UseMySql(connectionString, serverVersion, mySqlOptions =>
               {
                   mySqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                   mySqlOptions.MaxBatchSize(100);
                   mySqlOptions.CommandTimeout(30);
                   mySqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
               })
   .AddInterceptors(interceptor); // ✅ Ahora sí funciona

      // Configuración de ambiente
      if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
      {
          options.EnableSensitiveDataLogging();
          options.EnableDetailedErrors();
      }
      else
      {
          options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
      }
  });

            // 🔥 NUEVO: Registrar MediatR handlers desde Infrastructure (Event Handlers)
            services.AddMediatR(cfg =>
             cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
                    );

            // 2️⃣ DAPPER: Factory Pattern (Quirúrgico)
            // Eliminamos services.AddScoped<IDbConnection> para evitar conexiones vivas innecesarias.
            services.AddScoped<IDbConnectionFactory, SqlDbConnectionFactory>();

            // 4️⃣ Configuración y Servicios Core
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 🔥 Servicio de caché
            services.AddScoped<ICacheService, DistributedCacheService>();

            // 🔥 Registro de IFileStorageService (Faltaba antes)
            services.AddHttpContextAccessor();
            services.AddScoped<IFileStorageService, LocalFileStorageService>();

            // 5️⃣ Auth & Email
            services.AddScoped<IPasswordHasher, PasswordHasherService>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddSingleton<QueuedEmailService>();
            services.AddSingleton<IEmailService>(sp => sp.GetRequiredService<QueuedEmailService>());
            services.AddHostedService<EmailBackgroundSender>();

            // 6️⃣ Repositorios Manuales (Dashboard)
            services.AddScoped<DashboardRepository>();
            services.AddScoped<ApplicationInterface.IDashboardRepository>(sp => sp.GetRequiredService<DashboardRepository>());
            services.AddScoped<IDashboardRepository>(sp => sp.GetRequiredService<DashboardRepository>());

            // 7️⃣ Scrutor: Repositorios Automáticos
            services.Scan(scan => scan
              .FromAssemblies(Assembly.GetExecutingAssembly())
              .AddClasses(classes => classes.AssignableTo(typeof(IWriteRepository<,>)))
                 .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.Scan(scan => scan
        .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes.Where(type =>
          type.GetInterfaces().Any(i => i.IsGenericType &&
               i.GetGenericTypeDefinition() == typeof(IReadRepositoryWithDto<,,>))))
          .AsImplementedInterfaces()
              .WithScopedLifetime());

            services.AddScoped<IDomainValidator, DapperDomainValidator>();

            // 8️⃣ Scrutor: Servicios Infraestructura
            services.Scan(scan => scan
              .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes.InNamespaces("AhorroLand.Infrastructure.Services")
             .Where(c => !typeof(BackgroundService).IsAssignableFrom(c)
                && c != typeof(QueuedEmailService)
       && c != typeof(LocalFileStorageService)))
          .AsImplementedInterfaces()
                     .WithScopedLifetime());

            // 9️⃣ Warmup
            services.AddHostedService<DatabaseWarmupService>();

            return services;
        }
    }
}