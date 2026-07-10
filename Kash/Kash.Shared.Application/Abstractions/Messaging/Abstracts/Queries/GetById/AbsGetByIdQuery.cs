using SergioIzq.Domain.Kernel.Abstractions;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;
using MediatR;

namespace Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries
{
    /// <summary>
    /// Consulta base genérica para obtener una entidad por su ID.
    /// </summary>
    /// <typeparam name="TEntity">La Entidad de Dominio que se busca.</typeparam>
    /// <typeparam name="TDto">El DTO de respuesta que se espera (ya mapeado).</typeparam>
    public abstract record AbsGetByIdQuery<TEntity, TId, TDto>(Guid Id) : IRequest<Result<TDto>>
        where TEntity : AbsEntity<TId>
        where TId : IGuidValueObject
        where TDto : class;
}