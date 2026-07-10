using SergioIzq.Application.Kernel.Messaging;
using Kash.Shared.Application.Dtos;
namespace Kash.Application.Features.Auth.Queries;


public record GetUserProfileQuery(Guid UserId) : IQuery<UsuarioDto>;
