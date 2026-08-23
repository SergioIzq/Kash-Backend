using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;

namespace Kash.Application.Features.Auth.Commands.GenerateApiToken;

public sealed class GenerateApiTokenCommandHandler : ICommandHandler<GenerateApiTokenCommand, string>
{
    private readonly IUsuarioWriteRepository _usuarioWriteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApiTokenHasher _apiTokenHasher;

    public GenerateApiTokenCommandHandler(
        IUsuarioWriteRepository usuarioWriteRepository,
        IUnitOfWork unitOfWork,
        IApiTokenHasher apiTokenHasher)
    {
        _usuarioWriteRepository = usuarioWriteRepository;
        _unitOfWork = unitOfWork;
        _apiTokenHasher = apiTokenHasher;
    }

    public async Task<Result<string>> Handle(GenerateApiTokenCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioWriteRepository.GetByIdAsync(request.UsuarioId, cancellationToken);

        if (usuario is null)
        {
            return Result.Failure<string>(Error.NotFound("Usuario no encontrado"));
        }

        var (plainToken, hash) = _apiTokenHasher.GenerateToken();

        usuario.GenerarTokenApi(hash);

        _usuarioWriteRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(plainToken);
    }
}
