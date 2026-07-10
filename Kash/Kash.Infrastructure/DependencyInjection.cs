using Kash.Infrastructure.Configuration.Settings;
using Kash.Infrastructure.DataAccess;
using Kash.Infrastructure.Persistence.Command;
using Kash.Infrastructure.Persistence.Interceptors;
using Kash.Infrastructure.Persistence.Query;
using Kash.Infrastructure.Persistence.Warmup;
using Kash.Infrastructure.Services;
using Kash.Infrastructure.Services.Auth;
using Kash.Shared.Application.Abstractions.Services;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Interfaces;
using Kash.Shared.Application.Servicies;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using ApplicationInterface = Kash.Application.Interfaces;

namespace Kash.Infrastructure
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

            services.AddDbContext<KashDbContext>((sp, options) =>
  {
      var interceptor = sp.GetRequiredService<DomainEventDispatcherInterceptor>();

      options.UseMySql(connectionString, serverVersion, mySqlOptions =>
               {
                   mySqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                   mySqlOptions.MaxBatchSize(100);
                   mySqlOptions.CommandTimeout(30);
                   mySqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
               })
   .AddInterceptors(interceptor); // Ahora sí funciona

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

            // NUEVO: Registrar MediatR handlers desde Infrastructure (Event Handlers)
            services.AddMediatR(cfg =>
             cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
                    );

            // 2️⃣ DAPPER: Factory Pattern (Quirúrgico)
            // Eliminamos services.AddScoped<IDbConnection> para evitar conexiones vivas innecesarias.
            services.AddScoped<IDbConnectionFactory, SqlDbConnectionFactory>();

            // 4️⃣ Configuración y Servicios Core
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Servicio de caché
            services.AddScoped<ICacheService, DistributedCacheService>();

            // Registro de IFileStorageService (Faltaba antes)
            services.AddHttpContextAccessor();
            services.AddScoped<IFileStorageService, LocalFileStorageService>();

            // 5️⃣ Auth & Email
            services.AddScoped<IPasswordHasher, PasswordHasherService>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddSingleton<QueuedEmailService>();
            services.AddSingleton<IEmailService>(sp => sp.GetRequiredService<QueuedEmailService>());
            services.AddHostedService<EmailBackgroundSender>();

            // 6️⃣ Repositorios Manuales (Dashboard)
            services.AddScoped<ApplicationInterface.IDashboardRepository, DashboardRepository>();

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
               i.GetGenericTypeDefinition() == typeof(IReadRepository<,,>))))
          .AsImplementedInterfaces()
              .WithScopedLifetime());

            services.AddScoped<IDomainValidator, DapperDomainValidator>();

            // 8️⃣ Scrutor: Servicios Infraestructura
            services.Scan(scan => scan
              .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes.InNamespaces("Kash.Infrastructure.Services")
             .Where(c => !typeof(BackgroundService).IsAssignableFrom(c)
                && c != typeof(QueuedEmailService)
       && c != typeof(LocalFileStorageService)))
          .AsImplementedInterfaces()
                     .WithScopedLifetime());

            // HttpClient para resolución ISIN → Ticker (Yahoo Finance)
            services.AddHttpClient("YahooFinance", c =>
                c.BaseAddress = new Uri("https://query1.finance.yahoo.com"));

            // 9️⃣ Warmup
            services.AddHostedService<DatabaseWarmupService>();

            return services;
        }
    }
}
