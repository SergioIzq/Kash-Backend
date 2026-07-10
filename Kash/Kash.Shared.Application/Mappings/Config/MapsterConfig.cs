using Kash.Domain;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;
using SergioIzq.Application.Kernel.Mapping;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Kash.Infrastructure.Configuration;

public static class MapsterConfig
{
    public static void RegisterMapsterConfiguration(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;

        // Recomendado: Para evitar bucles infinitos en relaciones circulares
        config.Default.PreserveReference(true);

        // ---------------------------------------------------------
        // 1. MAPEOS GLOBALES PARA VALUE OBJECTS (PRIMITIVIZACIÓN)
        // ---------------------------------------------------------

        // --- IDs ---
        // Registra ambas direcciones (Id→Guid e Guid→Id vía CreateFromDatabase) para TODOS los
        // IGuidValueObject del assembly, sin mantener la lista a mano al añadir Ids nuevos.
        config.RegisterGuidValueObjects(typeof(UsuarioId).Assembly);

        // --- Valores de Dominio (específicos de Kash) ---
        config.NewConfig<Cantidad, decimal>().MapWith(src => src.Valor);
        config.NewConfig<FechaRegistro, DateTime>().MapWith(src => src.Valor);

        // Para Descripcion (que puede ser nula), manejamos el null check
        config.NewConfig<Descripcion, string?>()
              .MapWith(src => src._Value);

        // ---------------------------------------------------------
        // 2. ESCANEO DE REGISTROS (IRegister)
        // ---------------------------------------------------------
        config.Scan(
            Assembly.GetExecutingAssembly(),
            Assembly.GetAssembly(typeof(Cliente))!,
            Assembly.GetAssembly(typeof(ClienteDto))!
        );

        // 3. Registrar la configuración como Singleton
        services.AddSingleton(config);

        // 4. Registrar IAdapter (para usar .Adapt<T> fuera del DbContext)
        services.AddSingleton<IMapper, ServiceMapper>();
    }
}
