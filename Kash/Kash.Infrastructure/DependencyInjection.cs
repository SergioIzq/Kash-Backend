using Kash.Infrastructure.Persistence.Command;
using Kash.Infrastructure.Persistence.Query;
using Kash.Infrastructure.Services.Auth;
using Kash.Shared.Application.Interfaces;
using SergioIzq.AspNetCore.Kernel.DependencyInjection;
using SergioIzq.Infrastructure.Kernel.DependencyInjection;
using SergioIzq.Infrastructure.Kernel.Persistence;
using SergioIzq.Infrastructure.Kernel.Persistence.Interceptors;
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

            services.AddKernelUnitOfWork(); // IUnitOfWork + DomainEventDispatcherInterceptor (Scoped)

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

            // SergioIzq.Infrastructure.Kernel.UnitOfWork pide un DbContext genérico por constructor;
            // AddDbContext<KashDbContext> solo registra el tipo concreto, así que lo exponemos también.
            services.AddScoped<DbContext>(sp => sp.GetRequiredService<KashDbContext>());

            // NUEVO: Registrar MediatR handlers desde Infrastructure (Event Handlers)
            services.AddMediatR(cfg =>
             cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
                    );

            // 2️⃣ DAPPER: Factory Pattern (Quirúrgico)
            // Eliminamos services.AddScoped<IDbConnection> para evitar conexiones vivas innecesarias.
            services.AddScoped<IDbConnectionFactory, SqlDbConnectionFactory>();

            // 4️⃣ Servicios del kernel: caché, email en cola, scheduler Hangfire,
            // validador de dominio, warm-up de BD, y los servicios web
            // (IUserContext por claims, IPasswordHasher, IFileStorageService en wwwroot)
            services.AddKernelCache();
            services.AddKernelEmail(configuration);
            services.AddKernelJobScheduling();
            services.AddKernelDomainValidator();
            services.AddKernelDatabaseWarmup();
            services.AddKernelUserContext();
            services.AddKernelPasswordHasher();
            services.AddKernelFileStorage();

            // 5️⃣ Auth específica de Kash: adapter de IJwtTokenGenerator(Usuario) sobre el
            // generador genérico del kernel (KernelJwtTokenGenerator lo registra
            // AddKernelJwtAuthentication en Program.cs)
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            // 6️⃣ Repositorios Manuales (Dashboard + Reportes)
            services.AddScoped<ApplicationInterface.IDashboardRepository, DashboardRepository>();
            services.AddScoped<ApplicationInterface.IReporteRepository, ReporteRepository>();
            services.AddScoped<ApplicationInterface.IGastoHabitualesRepository, GastoHabitualesRepository>();
            services.AddScoped<ApplicationInterface.IIngresoHabitualesRepository, IngresoHabitualesRepository>();
            services.AddScoped<ApplicationInterface.IGastoSugerenciaRepository, GastoSugerenciaRepository>();
            services.AddScoped<ApplicationInterface.IIngresoSugerenciaRepository, IngresoSugerenciaRepository>();
            services.AddScoped<ApplicationInterface.IPresupuestoPdfGenerator, Reporting.PresupuestoPdfGenerator>();
            services.AddScoped<ApplicationInterface.IPresupuestoExcelGenerator, Reporting.PresupuestoExcelGenerator>();

            // 7️⃣ Repositorios Automáticos (Scrutor, vía SergioIzq.Infrastructure.Kernel)
            services.AddKernelRepositories(Assembly.GetExecutingAssembly());

            // 8️⃣ Scrutor: Servicios Infraestructura específicos de Kash
            services.Scan(scan => scan
              .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes.InNamespaces("Kash.Infrastructure.Services")
             .Where(c => !typeof(BackgroundService).IsAssignableFrom(c)))
          .AsImplementedInterfaces()
                     .WithScopedLifetime());

            // HttpClient para resolución ISIN → Ticker (Yahoo Finance)
            services.AddHttpClient("YahooFinance", c =>
                c.BaseAddress = new Uri("https://query1.finance.yahoo.com"));

            return services;
        }
    }
}
