using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging;
using AhorroLand.Shared.Application.Abstractions.Services;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.ValueObjects;

namespace AhorroLand.Application.Features.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand>
{
    private readonly IUsuarioWriteRepository _usuarioWriteRepository;
    private readonly IUsuarioReadRepository _usuarioReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
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

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {

        // 1. Buscar usuario por correo
        var emailVO = Email.Create(request.Email).Value;
        var usuario = await _usuarioReadRepository.GetByEmailAsync(emailVO, cancellationToken);

        // SEGURIDAD: Si el usuario no existe, no hacemos nada pero retornamos Success
        // para evitar enumeración de usuarios.
        if (usuario is null)
        {
            return Result.Success();
        }

        // 2. Lógica de Dominio: Generar token de recuperación
        // (Debes implementar este método en tu entidad Usuario)
        usuario.GenerarTokenRecuperacion();

        // 3. Persistir el cambio (el nuevo token)
        _usuarioWriteRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Construir enlace para el Frontend
        // Nota: Aquí apuntamos a la URL donde el usuario pondrá su nueva contraseña
        var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:4200";

        // Asumimos que usuario.TokenRecuperacion es el ValueObject del token generado
        var resetUrl = $"{baseUrl}/auth/reset-password?token={usuario.TokenRecuperacion!.Value.Value}&email={request.Email}";

        var emailBody = $@"
                <html>
                   <body style='font-family: Arial, sans-serif; font-size: 16px; color: #333;'>
                       <h1>Recuperación de Contraseña</h1>
                       <p>Hemos recibido una solicitud para restablecer tu contraseña en AhorroLand.</p>
                       <p>Si no fuiste tú, puedes ignorar este correo.</p>
                       <p>Para continuar, haz clic en el siguiente enlace:</p>
                       <p><a href='{resetUrl}' target='_blank' style='color: #1a73e8; text-decoration: none;'>Restablecer mi contraseña</a></p>
                   </body>
                </html>";

        // 5. Encolar email
        var emailMessage = new EmailMessage(
            usuario.Correo.Value,
            "Restablecer contraseña - AhorroLand",
            emailBody
        );

        _emailService.EnqueueEmail(emailMessage);

        return Result.Success();
    }
}