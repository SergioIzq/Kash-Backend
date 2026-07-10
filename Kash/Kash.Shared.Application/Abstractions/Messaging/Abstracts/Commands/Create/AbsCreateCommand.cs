using SergioIzq.Domain.Kernel.Abstractions;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;
using MediatR;

namespace Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;

/// <summary>
/// Comando base genérico para operaciones de Creación.
/// </summary>
/// <typeparam name="TEntity">La Entidad de Dominio que se va a crear.</typeparam>
/// <typeparam name="TDto">El DTO de respuesta que se espera.</typeparam>
// Hereda de IRequest<Result<TDto>> para el flujo de MediatR
public abstract record AbsCreateCommand<TEntity, TId> : IRequest<Result<Guid>>
    where TEntity : AbsEntity<TId>
    where TId : IGuidValueObject
{

}