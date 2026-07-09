using Kash.Domain;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Interfaces.Repositories;

/// <summary>
/// Interfaz para repositorio de lectura de IngresosProgramados.
/// Hereda todos los métodos base del IReadRepository y agrega métodos específicos.
/// </summary>
public interface IIngresoProgramadoReadRepository : IReadRepository<IngresoProgramado, IngresoProgramadoDto, IngresoProgramadoId>
{
    /// <summary>
    /// Busca un ingreso programado por su HangfireJobId y retorna su ID.
    /// Útil para ejecutar trabajos programados en Hangfire.
    /// </summary>
    Task<Guid?> GetIdByHangfireJobIdAsync(string hangfireJobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca un ingreso programado completo por su HangfireJobId.
    /// Retorna el DTO completo con todas las relaciones.
    /// </summary>
    Task<IngresoProgramadoDto?> GetByHangfireJobIdAsync(string hangfireJobId, CancellationToken cancellationToken = default);
}
