using Kash.Application.Features.Inversiones.Commands.Import.Parsers;
using Kash.Application.Features.Movimientos.Commands.Import.Parsers;
using SergioIzq.Application.Kernel.DependencyInjection;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Kash.Application
{
    /// <summary>
    /// Inyección de dependencias automática usando Marker Interfaces.
    /// Registra automáticamente servicios marcados con IApplicationService, ITransientService, ISingletonService
    /// </summary>
    public static class DependencyInyection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(DependencyInyection).Assembly)
            );

            var config = TypeAdapterConfig.GlobalSettings;
            config.Scan(typeof(DependencyInyection).Assembly);

            // Parsers de importación de inversiones
            services.AddScoped<GenericCsvParser>();
            services.AddScoped<TradeRepublicCsvParser>();
            services.AddScoped<TradeRepublicPdfParser>();
            services.AddScoped<DegiroCsvParser>();
            services.AddScoped<InteractiveBrokersCsvParser>();
            services.AddScoped<BinanceCsvParser>();

            // Parsers de importación de movimientos bancarios (gastos/ingresos, cualquier banco)
            services.AddScoped<GenericBankCsvParser>();
            services.AddScoped<GenericBankPdfParser>();

            // Registrar servicios automáticamente por marker interface (SergioIzq.Application.Kernel)
            services.AddMarkedServices(typeof(DependencyInyection).Assembly);

            // Orquestador de dependencias del kernel (antes se registraba vía marker;
            // la implementación vive ahora en el paquete y se registra explícitamente)
            services.AddKernelDependencyOrchestration();

            return services;
        }
    }
}

