using AhorroLand.Shared.Domain.ValueObjects.Ids;
using Dapper;

namespace AhorroLand.Infrastructure.TypesHandlers
{
    public static class DapperTypeHandlerRegistration
    {
        public static void RegisterGuidValueObjectHandlers()
        {
            // ? Registrar handler para Guid que maneja tanto BINARY(16) como strings UUID
            SqlMapper.AddTypeHandler(new GuidBinaryTypeHandler());

            // ? Registrar handlers para Value Objects específicos
            SqlMapper.AddTypeHandler(new GuidValueObjectTypeHandler<UsuarioId>(g => UsuarioId.Create(g).Value));
            SqlMapper.AddTypeHandler(new GuidValueObjectTypeHandler<ClienteId>(g => ClienteId.Create(g).Value));
            SqlMapper.AddTypeHandler(new GuidValueObjectTypeHandler<CategoriaId>(g => CategoriaId.Create(g).Value));
            SqlMapper.AddTypeHandler(new GuidValueObjectTypeHandler<ConceptoId>(g => ConceptoId.Create(g).Value ));
            SqlMapper.AddTypeHandler(new GuidValueObjectTypeHandler<CuentaId>(g => CuentaId.Create(g).Value));
            SqlMapper.AddTypeHandler(new GuidValueObjectTypeHandler<FormaPagoId>(g => FormaPagoId.Create(g).Value));
            SqlMapper.AddTypeHandler(new GuidValueObjectTypeHandler<PersonaId>(g => PersonaId.Create(g).Value));
            SqlMapper.AddTypeHandler(new GuidValueObjectTypeHandler<ProveedorId>(g => ProveedorId.Create(g).Value));
        }
    }
}
