using AhorroLand.Domain;
using AhorroLand.Shared.Application.Dtos;
using Mapster;

namespace AhorroLand.Shared.Application.Mappings
{
    public class GastoMappingRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Mapeo de Entidad Gasto a GastoDto (Lectura)
            config.ForType<Gasto, GastoDto>()

                // --- Mapeo de VOs simples (desenvolver) ---
                .Map(dest => dest.Importe, src => src.Importe.Valor)
                .Map(dest => dest.Fecha, src => src.Fecha.Valor)
                .Map(dest => dest.Descripcion, src => (string?)(src.Descripcion == null ? null : src.Descripcion.ToString()))

                // --- Concepto (ID + Nombre desde navegación) ---
                .Map(dest => dest.ConceptoId, src => src.ConceptoId.Value)
                .Map(dest => dest.ConceptoNombre, src => src.Concepto.Nombre.Value)

                // --- Categoria (ID + Nombre desde navegación) ---
                .Map(dest => dest.CategoriaId, src => src.Concepto.CategoriaId.Value)
                .Map(dest => dest.CategoriaNombre, src => src.Concepto.Categoria!.Nombre.Value)

                // --- Proveedor (ID + Nombre desde navegación) ---
                .Map(dest => dest.ProveedorId, src => src.ProveedorId.Value)
                .Map(dest => dest.ProveedorNombre, src => src.Proveedor.Nombre.Value)

                // --- Persona (ID + Nombre desde navegación) ---
                .Map(dest => dest.PersonaId, src => src.PersonaId.Value)
                .Map(dest => dest.PersonaNombre, src => src.Persona.Nombre.Value)

                // --- Cuenta (ID + Nombre desde navegación) ---
                .Map(dest => dest.CuentaId, src => src.CuentaId.Value)
                .Map(dest => dest.CuentaNombre, src => src.Cuenta.Nombre.Value)

                // --- FormaPago (ID + Nombre desde navegación) ---
                .Map(dest => dest.FormaPagoId, src => src.FormaPagoId.Value)
                .Map(dest => dest.FormaPagoNombre, src => src.FormaPago.Nombre.Value)

                // --- Usuario (Solo ID) ---
                .Map(dest => dest.UsuarioId, src => src.UsuarioId.Value)
                ;
        }
    }
}