using Kash.Application.Features.Inversiones.Commands.Import.Parsers;
using Kash.Application.Features.Movimientos.Commands.Import.Parsers;
using Kash.Shared.Application.Interfaces;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Kash.Application
{
    /// <summary>
    /// ✅ Inyección de dependencias automática usando Marker Interfaces.
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

            // 🔥 Parsers de importación de inversiones
            services.AddScoped<GenericCsvParser>();
            services.AddScoped<TradeRepublicCsvParser>();
            services.AddScoped<TradeRepublicPdfParser>();
            services.AddScoped<DegiroCsvParser>();
            services.AddScoped<InteractiveBrokersCsvParser>();
            services.AddScoped<BinanceCsvParser>();

            // 🔥 Parsers de importación de movimientos bancarios (gastos/ingresos, cualquier banco)
            services.AddScoped<GenericBankCsvParser>();
            services.AddScoped<GenericBankPdfParser>();

            // 🔥 Registrar servicios automáticamente por marker interface
            services.RegisterMarkedServices(typeof(DependencyInyection).Assembly);

            return services;
        }

        /// <summary>
        /// Extensión para registrar servicios marcados con IApplicationService, ITransientService, ISingletonService
        /// </summary>
        private static IServiceCollection RegisterMarkedServices(this IServiceCollection services, Assembly assembly)
        {
            // Obtener todos los tipos del ensamblado
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => !IsMediatRRequestOrHandler(t)) // 🔥 Excluir queries/commands/handlers de MediatR
                .ToList();

            foreach (var implementationType in types)
            {
                // Obtener todas las interfaces que implementa
                var allInterfaces = implementationType.GetInterfaces().ToList();

                // Verificar si alguna interfaz hereda de un marker interface
                var hasMarker = allInterfaces.Any(i => IsOrImplementsMarkerInterface(i));
                
                if (!hasMarker) continue;

                // Filtrar interfaces para registrar (excluyendo markers y system)
                var interfacesToRegister = allInterfaces
                    .Where(i => !IsMarkerInterface(i) && !IsSystemInterface(i) && !IsMediatRInterface(i))
                    .ToList();

                if (!interfacesToRegister.Any()) continue;

                // Determinar el lifetime basado en el marker
                ServiceLifetime lifetime = DetermineLifetime(allInterfaces);

                // Registrar cada interfaz con su implementación
                foreach (var interfaceType in interfacesToRegister)
                {
                    services.Add(new ServiceDescriptor(interfaceType, implementationType, lifetime));                                       
                }
            }

            return services;
        }

        /// <summary>
        /// 🔥 NUEVO: Verifica si un tipo es una Query/Command/Handler de MediatR
        /// </summary>
        private static bool IsMediatRRequestOrHandler(Type type)
        {
            var interfaces = type.GetInterfaces();
            
            // Excluir IRequest<T>, IBaseRequest (Queries/Commands)
            if (interfaces.Any(i => 
                i.IsGenericType && i.GetGenericTypeDefinition().FullName?.Contains("IRequest") == true))
                return true;

            if (interfaces.Any(i => i.FullName?.Contains("IBaseRequest") == true))
                return true;

            // Excluir IRequestHandler<T>, INotificationHandler<T> (Handlers)
            if (interfaces.Any(i => 
                i.IsGenericType && 
                (i.GetGenericTypeDefinition().FullName?.Contains("IRequestHandler") == true ||
                 i.GetGenericTypeDefinition().FullName?.Contains("INotificationHandler") == true)))
                return true;

            return false;
        }

        /// <summary>
        /// 🔥 NUEVO: Verifica si una interfaz es de MediatR
        /// </summary>
        private static bool IsMediatRInterface(Type type)
        {
            return type.FullName?.StartsWith("MediatR") == true;
        }

        /// <summary>
        /// Verifica si un tipo ES un marker interface o HEREDA de uno
        /// </summary>
        private static bool IsOrImplementsMarkerInterface(Type type)
        {
            // Es directamente un marker?
            if (IsMarkerInterface(type)) return true;

            // Sus interfaces heredan de un marker?
            var baseInterfaces = type.GetInterfaces();
            return baseInterfaces.Any(i => IsMarkerInterface(i));
        }

        private static ServiceLifetime DetermineLifetime(List<Type> interfaces)
        {
            // Buscar en todas las interfaces (incluidas las heredadas)
            foreach (var interfaceType in interfaces)
            {
                var baseInterfaces = interfaceType.GetInterfaces();
                
                if (baseInterfaces.Contains(typeof(ISingletonService)) || interfaceType == typeof(ISingletonService))
                    return ServiceLifetime.Singleton;

                if (baseInterfaces.Contains(typeof(ITransientService)) || interfaceType == typeof(ITransientService))
                    return ServiceLifetime.Transient;

                if (baseInterfaces.Contains(typeof(IApplicationService)) || interfaceType == typeof(IApplicationService))
                    return ServiceLifetime.Scoped;
            }

            // Por defecto Scoped
            return ServiceLifetime.Scoped;
        }

        private static bool IsMarkerInterface(Type type)
        {
            return type == typeof(IApplicationService) ||
                   type == typeof(ITransientService) ||
                   type == typeof(ISingletonService);
        }

        private static bool IsSystemInterface(Type type)
        {
            return type == typeof(IDisposable) ||
                   type.Namespace?.StartsWith("System") == true;
        }
    }
}

