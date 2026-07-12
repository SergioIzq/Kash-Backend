using Dapper;
using Kash.Application.Interfaces.Repositories;
using Kash.Domain;
using SergioIzq.Infrastructure.Kernel.Persistence;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;
using IInversionReadRepository = Kash.Application.Interfaces.Repositories.IInversionReadRepository;

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

    public async Task<bool> ExistsDuplicateAsync(
        Guid usuarioId,
        string ticker,
        DateOnly fechaCompra,
        decimal cantidad,
        decimal precioCompra,
        CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = @"
SELECT COUNT(1)
FROM inversiones
WHERE id_usuario    = @UsuarioId
  AND ticker        = @Ticker
  AND fecha_compra  = @FechaCompra
  AND ABS(cantidad      - @Cantidad)      < 0.00001
  AND ABS(precio_compra - @PrecioCompra)  < 0.001
LIMIT 1";

        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql,
                new
                {
                    UsuarioId    = usuarioId,
                    Ticker       = ticker,
                    FechaCompra  = fechaCompra.ToDateTime(TimeOnly.MinValue),
                    Cantidad     = cantidad,
                    PrecioCompra = precioCompra
                },
                cancellationToken: cancellationToken));

        return count > 0;
    }
}
