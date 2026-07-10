using SergioIzq.Domain.Kernel.Abstractions.Results;
using MediatR;

namespace Kash.Shared.Application.Abstractions.Messaging;

public interface IQuery<TResult> : IRequest<Result<TResult>>
{

}
