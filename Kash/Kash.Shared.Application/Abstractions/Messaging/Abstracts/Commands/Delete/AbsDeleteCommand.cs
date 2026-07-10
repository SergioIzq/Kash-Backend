using SergioIzq.Domain.Kernel.Abstractions;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;
using MediatR;

namespace Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;

/// <summary>
/// Comando base genérico para operaciones de Eliminación.
/// </summary>
/// <typeparam name="TEntity">La Entidad de Dominio que se va a eliminar.</typeparam>
// Devuelve un Result<Unit> o simplemente Result para indicar éxito o fallo.
public abstract record AbsDeleteCommand<TEntity, TId>(Guid Id) : IRequest<Result>
    where TEntity : AbsEntity<TId>
    where TId : IGuidValueObject;