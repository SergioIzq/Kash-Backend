using AhorroLand.Domain;
using AhorroLand.Shared.Application.Dtos;
using Mapster;

namespace AhorroLand.Shared.Application.Mappings
{
    public class TraspasoProgramadoMappingRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Mapeo para TraspasoProgramado → TraspasoProgramadoDto
            config.ForType<TraspasoProgramado, TraspasoProgramadoDto>()
                // Value Objects
                .Map(dest => dest.Id, src => src.Id.Value)
                .Map(dest => dest.Importe, src => src.Importe.Valor)
                .Map(dest => dest.FechaEjecucion, src => src.FechaEjecucion)
                .Map(dest => dest.Descripcion, src => src.Descripcion != null ? src.Descripcion.Value._Value : null)
                .Map(dest => dest.UsuarioId, src => src.UsuarioId.Value)
                .Map(dest => dest.Frecuencia, src => src.Frecuencia.Value)
                .Map(dest => dest.Activo, src => src.Activo)
                .Map(dest => dest.HangfireJobId, src => src.HangfireJobId)

                // IDs de relaciones
                .Map(dest => dest.CuentaOrigenId, src => src.CuentaOrigenId.Value)
                .Map(dest => dest.CuentaDestinoId, src => src.CuentaDestinoId.Value)

                // Nombres de relaciones (navegación)
                .Map(dest => dest.CuentaOrigenNombre, src => src.CuentaOrigen != null ? src.CuentaOrigen.Nombre.Value : "")
                .Map(dest => dest.CuentaDestinoNombre, src => src.CuentaDestino != null ? src.CuentaDestino.Nombre.Value : "");
        }
    }
}