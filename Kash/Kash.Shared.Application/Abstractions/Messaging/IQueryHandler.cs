using SergioIzq.Domain.Kernel.Abstractions.Results;
using MediatR;

namespace Kash.Shared.Application.Abstractions.Messaging;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>> where TQuery : IQuery<TResponse>
{
}