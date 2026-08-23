using SergioIzq.Application.Kernel.Messaging;
using Kash.Shared.Application.Dtos;

namespace Kash.Application.Features.Auth.Queries;

public sealed record GetApiTokenStatusQuery(Guid UsuarioId) : IQuery<ApiTokenStatusDto>;
