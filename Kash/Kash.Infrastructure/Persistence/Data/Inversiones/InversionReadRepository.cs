using Kash.Domain;
using Kash.Infrastructure.Persistence.Query;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Infrastructure.Persistence.Data.Inversiones;

public class InversionReadRepository
    : AbsReadRepository<Inversion, InversionDto, InversionId>, IInversionReadRepository
{
    public InversionReadRepository(IDbConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory)
    {
    }

    protected override ReadRepositoryConfiguration ConfigureRepository()
    {
        return ReadRepositoryConfiguration.Simple(
            tableName: "inversiones",
            selectColumns:
            [
                "id           AS Id",
                "nombre       AS Nombre",
                "ticker       AS Ticker",
                "tipo         AS Tipo",
                "cantidad     AS Cantidad",
                "precio_compra AS PrecioCompra",
                "moneda       AS Moneda",
                "fecha_compra AS FechaCompra",
                "descripcion  AS Descripcion",
                "plataforma   AS Plataforma",
                "id_usuario   AS UsuarioId"
            ],
            searchableColumns:
            [
                "nombre",
                "ticker"
            ],
            sortableColumns: new Dictionary<string, string>
            {
                ["nombre"]       = "nombre",
                ["ticker"]       = "ticker",
                ["tipo"]         = "tipo",
                ["fechaCompra"]  = "fecha_compra",
                ["cantidad"]     = "cantidad",
                ["precioCompra"] = "precio_compra"
            },
            defaultOrderBy: "fecha_compra DESC, id DESC");
    }
}
