using SergioIzq.Domain.Kernel.Abstractions;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;
using MediatR;

namespace Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;

/// <summary>
/// Query base abstracta para obtener elementos recientes.
/// Diseñada para ser ultra-rápida (<10ms) con resultados limitados.
/// Ideal para pre-cargar selectores con los elementos usados recientemente.
/// </summary>
public abstract record GetRecentQuery<TEntity, TDto, TId> : IRequest<Result<IEnumerable<TDto>>>
    where TEntity : AbsEntity<TId>
    where TDto : class
    where TId : IGuidValueObject
{
    public Guid? UsuarioId { get; init; }
    public int Limit { get; init; }

    protected GetRecentQuery(int limit = 5)
    {
        Limit = limit > 50 ? 50 : limit; // Máximo 50 resultados
    }
}
