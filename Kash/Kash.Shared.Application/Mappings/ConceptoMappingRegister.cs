using Kash.Domain;
using Kash.Shared.Application.Dtos;
using Mapster;

namespace Kash.Shared.Application.Mappings
{
    public class ConceptoMappingRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Mapeo de Entidad a DTO (Lectura)
            config.ForType<Concepto, ConceptoDto>()

                // Mapeo de Relaciones Aplanadas
                .Map(dest => dest.CategoriaId, src => src.CategoriaId);
        }
    }
}
