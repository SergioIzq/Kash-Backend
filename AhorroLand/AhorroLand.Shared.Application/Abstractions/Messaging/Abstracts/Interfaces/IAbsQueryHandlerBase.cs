using AhorroLand.Shared.Domain.Abstractions;

namespace AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Interfaces;

/// <summary>
/// Interfaz base para todos los Query Handlers.
/// 🔥 SIMPLIFICADA: Ya no contiene métodos, solo sirve como marcador de tipo.
/// Los handlers usan IReadRepositoryWithDto directamente para consultas optimizadas.
/// </summary>
/// <typeparam name="TEntity">La entidad raíz que maneja el handler.</typeparam>
public interface IQueryHandlerBase<TEntity>
    where TEntity : AbsEntity
{
    // Interfaz vacía - solo sirve como marcador de tipo
}
