using Kash.Domain;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;
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

        // Recomendado: Ignorar nulos al mapear si quieres evitar sobrescribir datos con nulls (opcional)
        // config.Default.IgnoreNullValues(true);

        // ---------------------------------------------------------
        // 1. MAPEOS GLOBALES PARA VALUE OBJECTS (PRIMITIVIZACIÓN)
        // ---------------------------------------------------------
        // Al definir esto aquí, Mapster sabrá automáticamente cómo convertir
        // Gasto.Importe (Cantidad) -> GastoDto.Importe (decimal)
        // sin que tengas que repetirlo en cada DTO.

        // --- IDs ---
        config.NewConfig<UsuarioId, Guid>().MapWith(src => src.Value);
        config.NewConfig<Guid, UsuarioId>().MapWith(src => UsuarioId.CreateFromDatabase(src));

        config.NewConfig<GastoId, Guid>().MapWith(src => src.Value);
        config.NewConfig<ConceptoId, Guid>().MapWith(src => src.Value);
        config.NewConfig<ProveedorId, Guid>().MapWith(src => src.Value);
        config.NewConfig<PersonaId, Guid>().MapWith(src => src.Value);
        config.NewConfig<CuentaId, Guid>().MapWith(src => src.Value);
        config.NewConfig<FormaPagoId, Guid>().MapWith(src => src.Value);
        config.NewConfig<CategoriaId, Guid>().MapWith(src => src.Value);
        config.NewConfig<ReglaCategorizacionId, Guid>().MapWith(src => src.Value);

        // --- Valores de Dominio ---
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
