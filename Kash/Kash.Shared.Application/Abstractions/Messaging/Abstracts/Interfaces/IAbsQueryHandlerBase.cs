using Kash.Shared.Domain.Abstractions;
using Kash.Shared.Domain.Interfaces;

namespace Kash.Shared.Application.Abstractions.Messaging.Abstracts.Interfaces;

/// <summary>
/// Interfaz base para todos los Query Handlers.
/// 🔥 SIMPLIFICADA: Ya no contiene métodos, solo sirve como marcador de tipo.
/// Los handlers usan IReadRepository directamente para consultas optimizadas.
/// </summary>
/// <typeparam name="TEntity">La entidad raíz que maneja el handler.</typeparam>
public interface IQueryHandlerBase<TEntity, TId>
    where TEntity : AbsEntity<TId>
    where TId : IGuidValueObject
{
    // Interfaz vacía - solo sirve como marcador de tipo
}
