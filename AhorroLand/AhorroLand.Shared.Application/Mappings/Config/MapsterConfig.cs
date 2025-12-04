using AhorroLand.Domain;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AhorroLand.Infrastructure.Configuration;

public static class MapsterConfig
{
    public static void RegisterMapsterConfiguration(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;

        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);

        // 2. Aplicar Configuraciones (Mapas Específicos)
        // Escanea las asambleas donde defines tus mapeos específicos (si usas IRegister)
        TypeAdapterConfig.GlobalSettings.Scan(
            Assembly.GetExecutingAssembly(),
            Assembly.GetAssembly(typeof(Cliente))!,
            Assembly.GetAssembly(typeof(ClienteDto))! // Escanear también donde están los DTOs
        );

        // ❌ ELIMINADO: Ya está configurado en ClienteMappingRegister
        // config.NewConfig<ClienteDto, Cliente>()
        //   .MapWith(src => Cliente.Create(new Nombre(src.Nombre), new UsuarioId(src.UsuarioId)));

        // ✅ Mapeo global para Value Objects → Guid
        config.NewConfig<UsuarioId, Guid>()
            .MapWith(src => src.Value);

        config.NewConfig<Guid, UsuarioId>()
            .MapWith(src => UsuarioId.Create(src).Value);

        // 3. Registrar la configuración como Singleton
        services.AddSingleton(TypeAdapterConfig.GlobalSettings);

        // 4. Registrar IAdapter (para usar .Adapt<T> fuera del DbContext)
        services.AddSingleton<IMapper, ServiceMapper>();
    }
}