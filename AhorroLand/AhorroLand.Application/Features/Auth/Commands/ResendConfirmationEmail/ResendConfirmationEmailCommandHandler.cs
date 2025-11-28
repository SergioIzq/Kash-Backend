using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging;
using AhorroLand.Shared.Application.Abstractions.Services;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.ValueObjects;

namespace AhorroLand.Application.Features.Auth.Commands.ResendConfirmationEmail;

public sealed class ResendConfirmationEmailCommandHandler : ICommandHandler<ResendConfirmationEmailCommand>
{
    private readonly IUsuarioWriteRepository _usuarioWriteRepository;
    private readonly IUsuarioReadRepository _usuarioReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public ResendConfirmationEmailCommandHandler(
        IUsuarioWriteRepository usuarioWriteRepository,
        IUsuarioReadRepository usuarioReadRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService)
    {
        _usuarioWriteRepository = usuarioWriteRepository;
        _usuarioReadRepository = usuarioReadRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<Result> Handle(ResendConfirmationEmailCommand request, CancellationToken cancellationToken)
    {
        // 1. Buscar usuario
        var emailVo = new Email(request.Correo);
        var user = await _usuarioReadRepository.GetByEmailAsync(emailVo, cancellationToken);

        // 2. Validaciones
        if (user is null)
        {
            // Por seguridad, no decimos "no existe", decimos "si existe, se envió".
            return Result.Success();
        }

        if (user.Activo)
        {
            return Result.Failure(new Error("Auth.AlreadyConfirmed", "Esta cuenta ya está confirmada. Inicia sesión."));
        }

        // 3. Generar NUEVO token (invalida el anterior)
        var newToken = ConfirmationToken.GenerateNew();

        // Asumiendo que tienes un método para actualizar esto en tu dominio
        user.SetTokenConfirmacion(newToken);

        await _usuarioWriteRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);


        // 5. Encolar email de confirmación (no bloqueante)
        var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:4200";
        var confirmUrl = $"{baseUrl}/auth/confirmar-correo?token={user.TokenConfirmacion!.Value.Value}";

        var emailBody = $@"
                            <html>
                                      <body style='font-family: Arial, sans-serif; font-size: 16px; color: #333;'>
                               <h1>Gracias por registrarte en AhorroLand</h1>
                                <p>Estamos felices de tenerte aquí. Por favor accede al siguiente enlace para verificar y activar tu cuenta:</p>
                                 <p><a href='{confirmUrl}' target='_blank' style='color: #1a73e8; text-decoration: none;'>Confirmar mi cuenta</a></p>
                           </body>
                            </html>";

        var emailMessage = new EmailMessage(
            user.Correo.Value,
            "Bienvenido a AhorroLand",
            emailBody
        );

        _emailService.EnqueueEmail(emailMessage);
        return Result.Success();
    }
}
