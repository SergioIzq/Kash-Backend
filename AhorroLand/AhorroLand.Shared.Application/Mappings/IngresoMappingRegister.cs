using AhorroLand.Domain;
using AhorroLand.Shared.Application.Dtos;
using Mapster;

namespace AhorroLand.Shared.Application.Mappings
{
    public class IngresoMappingRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Mapeo de Entidad a DTO (Lectura)
            config.ForType<Ingreso, IngresoDto>()

                // Mapeo de Relaciones Aplanadas
                .Map(dest => dest.ConceptoId, src => src.ConceptoId)
                .Map(dest => dest.ClienteId, src => src.ClienteId)
                .Map(dest => dest.PersonaId, src => src.PersonaId)
                .Map(dest => dest.CuentaId, src => src.CuentaId)
                .Map(dest => dest.FormaPagoId, src => src.FormaPagoId)
                ;
        }
    }
}