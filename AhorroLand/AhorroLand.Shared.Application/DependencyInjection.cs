using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.ValueObjects;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AhorroLand.Shared.Application;

public static class DependencyInyection
{
    public static IServiceCollection AddSharedApplication(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;

        // 1. Escanear mapeos manuales (por si alguno específico lo necesita)
        config.Scan(Assembly.GetExecutingAssembly());

        // ==============================================================================
        // 🔥 MAPEO AUTOMÁTICO GLOBAL DE VALUE OBJECTS (Para que no pete nunca más)
        // ==============================================================================

        // A. Enseñar a Mapster a desempaquetar 'Cantidad' a 'decimal' siempre
        config.NewConfig<Cantidad, decimal>()
              .MapWith(src => src.Valor); // O .Value, revisa tu propiedad pública

        // B. Enseñar a Mapster a desempaquetar 'FechaRegistro' a 'DateTime' siempre
        config.NewConfig<FechaRegistro, DateTime>()
              .MapWith(src => src.Valor);

        // C. Enseñar a Mapster a manejar 'Descripcion?' (nullable) a 'string?'
        config.NewConfig<Descripcion?, string?>()
              .MapWith(src => src.HasValue ? src.Value._Value : null);

        config.NewConfig<IGuidValueObject, Guid>()
                      .MapWith(src => src.Value);

        return services;
    }
}