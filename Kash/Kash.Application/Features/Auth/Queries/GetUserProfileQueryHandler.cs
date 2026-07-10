using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Auth.Queries
{
    public sealed class GetUserProfileQueryHandler : IQueryHandler<GetUserProfileQuery, UsuarioDto>
    {
        private readonly IReadRepository<Usuario, UsuarioDto, UsuarioId> _usuarioReadRepository;

        public GetUserProfileQueryHandler(IReadRepository<Usuario, UsuarioDto, UsuarioId> usuarioReadRepository)
        {
            _usuarioReadRepository = usuarioReadRepository;
        }

        public async Task<Result<UsuarioDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            var usuario = await _usuarioReadRepository.GetReadModelByIdAsync(request.UserId, cancellationToken);

            if (usuario is null)
            {
                return Result.Failure<UsuarioDto>(Error.NotFound("Usuario no encontrado"));
            }

            return Result.Success(usuario);
        }
    }
}
