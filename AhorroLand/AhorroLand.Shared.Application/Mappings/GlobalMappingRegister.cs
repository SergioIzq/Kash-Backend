using AhorroLand.Shared.Domain.ValueObjects;
using Mapster;

namespace AhorroLand.Shared.Application.Mappings
{
    public class GlobalMappingRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Mapeo de VO a Primitivo (Para DTOs: Nombre -> string)
            config.ForType<Nombre, string>()
                  .MapWith(src => src.Value);

            config.ForType<Cantidad, decimal>()
                  .MapWith(src => src.Valor);

            config.ForType<Descripcion, string?>()
                  .MapWith(src => src._Value);

            config.ForType<FechaRegistro, DateTime>()
                  .MapWith(src => src.Valor);

            // Mapeo de Primitivo a VO (Para Entidades: string -> Nombre)
            config.ForType<string, Nombre>()
                  .MapWith(src => Nombre.Create(src).Value);

            config.ForType<decimal, Cantidad>()
                  .MapWith(src => Cantidad.Create(src).Value);
        }
    }
}