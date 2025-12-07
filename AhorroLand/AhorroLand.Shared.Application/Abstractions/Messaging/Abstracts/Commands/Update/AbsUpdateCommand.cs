using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;
using MediatR;

namespace AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;

/// <summary>
/// Comando base genérico para operaciones de Actualización.
/// 🔥 MODIFICADO: Ahora devuelve Result<Guid> en lugar de Result<TDto>.
/// </summary>
/// <typeparam name="TEntity">La Entidad de Dominio que se va a actualizar.</typeparam>
/// <typeparam name="TId">El tipo del ID de la entidad.</typeparam>
/// <typeparam name="TDto">El DTO (solo usado para mantener compatibilidad, no se devuelve).</typeparam>
public abstract record AbsUpdateCommand<TEntity, TId, TDto> : IRequest<Result<Guid>>
    where TEntity : AbsEntity<TId>
    where TId : IGuidValueObject
{
    // Propiedad requerida para identificar la entidad a actualizar.
    public Guid Id { get; init; }
}
