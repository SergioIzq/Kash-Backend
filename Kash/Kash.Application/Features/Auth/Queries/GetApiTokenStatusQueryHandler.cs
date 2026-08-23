using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Application.Features.Auth.Queries;

public sealed class GetApiTokenStatusQueryHandler : IQueryHandler<GetApiTokenStatusQuery, ApiTokenStatusDto>
{
    private readonly IUsuarioWriteRepository _usuarioWriteRepository;

    public GetApiTokenStatusQueryHandler(IUsuarioWriteRepository usuarioWriteRepository)
    {
        _usuarioWriteRepository = usuarioWriteRepository;
    }

    public async Task<Result<ApiTokenStatusDto>> Handle(GetApiTokenStatusQuery request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioWriteRepository.GetByIdAsync(request.UsuarioId, cancellationToken);

        if (usuario is null)
        {
            return Result.Failure<ApiTokenStatusDto>(Error.NotFound("Usuario no encontrado"));
        }

        return Result.Success(new ApiTokenStatusDto(usuario.ApiTokenHash is not null, usuario.ApiTokenCreatedAt));
    }
}
