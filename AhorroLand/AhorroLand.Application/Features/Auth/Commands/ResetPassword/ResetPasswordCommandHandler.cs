using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.ValueObjects;

namespace AhorroLand.Application.Features.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand>
{
    private readonly IUsuarioWriteRepository _usuarioWriteRepository;
    private readonly IUsuarioReadRepository _usuarioReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(
        IUsuarioWriteRepository usuarioWriteRepository,
        IUsuarioReadRepository usuarioReadRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _usuarioWriteRepository = usuarioWriteRepository;
        _usuarioReadRepository = usuarioReadRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // 1. Buscar usuario
        var emailResult = Email.Create(request.Email);
        var usuario = await _usuarioReadRepository.GetByEmailAsync(emailResult.Value, cancellationToken);

        if (usuario is null)
        {
            return Result.Failure(Error.NotFound());
        }

        // 2. Hash de la nueva contraseña
        var hashedPassword = _passwordHasher.HashPassword(request.NewPassword);
        var passwordHashResult = PasswordHash.Create(hashedPassword);

        // 3. Lógica de Dominio: Cambiar contraseña
        // Este método en el dominio debe verificar:
        // - Que el token coincida
        // - Que el token no haya expirado
        // - Si es válido, actualiza la password y borra el token usado
        var resultadoCambio = usuario.RestablecerContrasena(request.Token, passwordHashResult.Value);

        if (resultadoCambio.IsFailure)
        {
            return resultadoCambio; // Retorna error si el token expiró o es inválido
        }

        // 4. Guardar cambios
        _usuarioWriteRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}