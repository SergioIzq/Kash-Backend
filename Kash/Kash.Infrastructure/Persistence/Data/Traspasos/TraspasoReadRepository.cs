using Kash.Domain;
using SergioIzq.Infrastructure.Kernel.Persistence;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Infrastructure.Persistence.Data.Traspasos
{
    public class TraspasoReadRepository : AbsReadRepository<Traspaso, TraspasoDto, TraspasoId>, ITraspasoReadRepository
    {
        public TraspasoReadRepository(IDbConnectionFactory dbConnectionFactory)
            : base(dbConnectionFactory)
        {
        }

        /// <summary>
        /// ÚNICA CONFIGURACIÓN REQUERIDA: Define todas las características del repositorio.
        /// </summary>
        protected override ReadRepositoryConfiguration ConfigureRepository()
        {
            return ReadRepositoryConfiguration.WithJoins(
                tableName: "traspasos",
                tableAlias: "t",
                selectColumns: new List<string>
                {
                    "t.id as Id",
                    "t.importe as Importe",
                    "t.fecha as Fecha",
                    "t.descripcion as Descripcion",
                    "t.id_cuenta_origen as CuentaOrigenId",
                    "COALESCE(co.nombre, '') as CuentaOrigenNombre",
                    "t.id_cuenta_destino as CuentaDestinoId",
                    "COALESCE(cd.nombre, '') as CuentaDestinoNombre",
                    "t.id_usuario as UsuarioId"
                },
                joinClause: @"LEFT JOIN cuentas co ON t.id_cuenta_origen = co.id
LEFT JOIN cuentas cd ON t.id_cuenta_destino = cd.id",
                searchableColumns: new List<string>
                {
                    "t.descripcion",
                    "co.nombre",
                    "cd.nombre"
                },
                numericSearchableColumns: new List<string>
                {
                    "t.importe"
                },
                dateSearchableColumns: new List<string>
                {
                    "t.fecha"
                },
                sortableColumns: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Fecha", "t.fecha" },
                    { "Importe", "t.importe" },
                    { "CuentaOrigenNombre", "co.nombre" },
                    { "CuentaDestinoNombre", "cd.nombre" }
                },
                defaultOrderBy: "t.fecha DESC, t.id DESC"
            );
        }
    }
}
