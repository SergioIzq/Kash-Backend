using AhorroLand.Domain;
using AhorroLand.Infrastructure.Persistence.Query;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Infrastructure.Persistence.Data.Traspasos
{
    public class TraspasoReadRepository : AbsReadRepository<Traspaso, TraspasoDto, TraspasoId>, ITraspasoReadRepository
    {
        public TraspasoReadRepository(IDbConnectionFactory dbConnectionFactory)
 : base(dbConnectionFactory, "traspasos")
        {
        }

        protected override string GetTableAlias()
        {
            return "t";
        }

        protected override string BuildCountQuery()
        {
            return @"SELECT COUNT(*) FROM traspasos t
LEFT JOIN cuentas co ON t.id_cuenta_origen = co.id
LEFT JOIN cuentas cd ON t.id_cuenta_destino = cd.id";
        }

        protected override string BuildGetByIdQuery()
        {
            return @"
SELECT 
    t.id as Id,
    t.importe as Importe,
    t.fecha as Fecha,
    t.descripcion as Descripcion,
    t.id_cuenta_origen as CuentaOrigenId,
    COALESCE(co.nombre, '') as CuentaOrigenNombre,
    t.id_cuenta_destino as CuentaDestinoId,
    COALESCE(cd.nombre, '') as CuentaDestinoNombre,
    t.id_usuario as UsuarioId
FROM traspasos t
LEFT JOIN cuentas co ON t.id_cuenta_origen = co.id
LEFT JOIN cuentas cd ON t.id_cuenta_destino = cd.id
WHERE t.id = @id";
        }

        protected override string BuildGetAllQuery()
        {
            return @"
SELECT 
    t.id as Id,
    t.importe as Importe,
    t.fecha as Fecha,
    t.descripcion as Descripcion,
    t.id_cuenta_origen as CuentaOrigenId,
    COALESCE(co.nombre, '') as CuentaOrigenNombre,
    t.id_cuenta_destino as CuentaDestinoId,
    COALESCE(cd.nombre, '') as CuentaDestinoNombre,
    t.id_usuario as UsuarioId
FROM traspasos t
LEFT JOIN cuentas co ON t.id_cuenta_origen = co.id
LEFT JOIN cuentas cd ON t.id_cuenta_destino = cd.id";
        }

        protected override string BuildGetPagedQuery()
        {
            return BuildGetAllQuery();
        }

        protected override string GetDefaultOrderBy()
        {
            return "ORDER BY t.fecha DESC, t.id DESC";
        }

        protected override Dictionary<string, string> GetSortableColumns()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
           { "Fecha", "t.fecha" },
          { "Importe", "t.importe" },
                { "CuentaOrigenNombre", "co.nombre" },
    { "CuentaDestinoNombre", "cd.nombre" }
  };
        }

        protected override List<string> GetSearchableColumns()
        {
            return new List<string>
            {
    "t.descripcion",      // Descripción del traspaso
     "co.nombre",          // Nombre de cuenta origen
         "cd.nombre"           // Nombre de cuenta destino
            };
        }

        protected override List<string> GetNumericSearchableColumns()
        {
            return new List<string>
            {
    "t.importe"
          };
        }

        protected override List<string> GetDateSearchableColumns()
        {
            return new List<string>
        {
    "t.fecha"
     };
        }
    }
}