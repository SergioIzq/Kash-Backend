using SergioIzq.Domain.Kernel.Abstractions.Results;
using MediatR;

namespace Kash.Shared.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>
{

}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}