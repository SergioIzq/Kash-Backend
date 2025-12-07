using AhorroLand.Domain;
using AhorroLand.Shared.Application.Dtos;
using Mapster;

namespace AhorroLand.Shared.Application.Mappings
{
    public class TraspasoMappingRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.ForType<Traspaso, TraspasoDto>()
                // 1. Mapeo de Value Objects Primitivos (El manual de instrucciones)
                .Map(dest => dest.Id, src => src.Id.Value)
                .Map(dest => dest.Importe, src => src.Importe.Valor) // .Valor o .Value según tu VO
                .Map(dest => dest.Fecha, src => src.Fecha.Valor)
                .Map(dest => dest.Descripcion, src => (string?)(src.Descripcion == null ? null : src.Descripcion.ToString()))
                .Map(dest => dest.UsuarioId, src => src.UsuarioId.Value)

                // 2. Mapeo de IDs de Relaciones
                .Map(dest => dest.CuentaOrigenId, src => src.CuentaOrigenId.Value)
                .Map(dest => dest.CuentaDestinoId, src => src.CuentaDestinoId.Value)

                .Map(dest => dest.CuentaOrigenNombre, src => src.CuentaOrigen != null ? src.CuentaOrigen.Nombre.Value : "")
                .Map(dest => dest.CuentaDestinoNombre, src => src.CuentaDestino != null ? src.CuentaDestino.Nombre.Value : "");
        }
    }
}