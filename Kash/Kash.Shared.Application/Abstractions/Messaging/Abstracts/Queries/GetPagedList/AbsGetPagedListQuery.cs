using SergioIzq.Domain.Kernel.Abstractions;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;
using MediatR;

namespace Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries
{
    /// <summary>
    /// Consulta base genérica para obtener listados paginados.
    /// OPTIMIZADO: Incluye UsuarioId para usar índices en la base de datos.
    /// </summary>
    /// <typeparam name="TEntity">La Entidad de Dominio base (para el repositorio).</typeparam>
    /// <typeparam name="TDto">El DTO que representará cada elemento de la lista.</typeparam>
    public abstract record AbsGetPagedListQuery<TEntity, TId, TDto>(
        int Page,
        int PageSize,
        string SearchTerm = "",
        string SortColumn = "",
        string SortOrder = "",
        Guid? UsuarioId = null) : IRequest<Result<PagedList<TDto>>>
        where TEntity : AbsEntity<TId>
        where TId : IGuidValueObject
        where TDto : class;
}
