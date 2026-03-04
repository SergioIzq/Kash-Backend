using Kash.Application.Features.Traspasos.Commands;
using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging;
using Kash.Shared.Application.Abstractions.Services;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kash.Application.Features.TraspasosProgramados.Commands.Execute;

/// <summary>
/// Handler que ejecuta la lógica de negocio cuando Hangfire activa el job de un TraspasoProgramado.
/// 🔥 NUEVO: Envía email de notificación al usuario después de ejecutar.
/// </summary>
public sealed class ExecuteTraspasoProgramadoCommandHandler : ICommandHandler<ExecuteTraspasoProgramadoCommand>
{
    private readonly IReadRepository<TraspasoProgramado, TraspasoProgramadoDto, TraspasoProgramadoId> _traspasoProgramadoReadRepository;
    private readonly IReadRepository<Usuario, UsuarioDto, UsuarioId> _usuarioReadRepository;
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;
    private readonly ILogger<ExecuteTraspasoProgramadoCommandHandler> _logger;

    public ExecuteTraspasoProgramadoCommandHandler(
        IReadRepository<TraspasoProgramado, TraspasoProgramadoDto, TraspasoProgramadoId> traspasoProgramadoReadRepository,
        IReadRepository<Usuario, UsuarioDto, UsuarioId> usuarioReadRepository,
        IMediator mediator,
        IEmailService emailService,
        ILogger<ExecuteTraspasoProgramadoCommandHandler> logger)
    {
        _traspasoProgramadoReadRepository = traspasoProgramadoReadRepository;
        _usuarioReadRepository = usuarioReadRepository;
        _mediator = mediator;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> Handle(ExecuteTraspasoProgramadoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Ejecutando TraspasoProgramado {TraspasoProgramadoId}", request.TraspasoProgramadoId);
            }

            // 1. Obtener el TraspasoProgramado
            var traspasoProgramado = await _traspasoProgramadoReadRepository.GetReadModelByIdAsync(
                request.TraspasoProgramadoId,
                cancellationToken);

            if (traspasoProgramado == null)
            {
                _logger.LogWarning("TraspasoProgramado {TraspasoProgramadoId} no encontrado", request.TraspasoProgramadoId);
                return Result.Failure(Error.NotFound($"TraspasoProgramado con ID {request.TraspasoProgramadoId} no encontrado"));
            }

            // 🔥 VALIDACIÓN: Si está inactivo, no ejecutar
            if (!traspasoProgramado.Activo)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("TraspasoProgramado {TraspasoProgramadoId} está inactivo, se omite la ejecución", request.TraspasoProgramadoId);
                }
                return Result.Success();
            }

            // 2. Crear el traspaso real
            var createTraspasoCommand = new CreateTraspasoCommand
            {
                Importe = traspasoProgramado.Importe,
                Fecha = DateTime.Now,
                CuentaDestinoId = traspasoProgramado.CuentaDestinoId,
                CuentaOrigenId = traspasoProgramado.CuentaOrigenId,
                UsuarioId = traspasoProgramado.UsuarioId,
                Descripcion = traspasoProgramado.Descripcion
            };

            var result = await _mediator.Send(createTraspasoCommand, cancellationToken);

            if (result.IsSuccess)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Traspaso creado exitosamente desde TraspasoProgramado {TraspasoProgramadoId}", request.TraspasoProgramadoId);
                }

                // 🔥 NUEVO: Enviar email de notificación al usuario
                await EnviarEmailNotificacionAsync(traspasoProgramado, cancellationToken);
            }
            else
            {
                _logger.LogError("Error al crear Traspaso desde TraspasoProgramado {TraspasoProgramadoId}: {Error}",
                    request.TraspasoProgramadoId, result.Error);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al ejecutar TraspasoProgramado {TraspasoProgramadoId}", request.TraspasoProgramadoId);
            return Result.Failure(Error.Failure("Execute.TraspasoProgramado", "Error de Ejecución", ex.Message));
        }
    }

    /// <summary>
    /// 🔥 NUEVO: Envía un email al usuario notificando que se ejecutó el traspaso programado.
    /// </summary>
    private async Task EnviarEmailNotificacionAsync(TraspasoProgramadoDto traspaso, CancellationToken cancellationToken)
    {
        try
        {
            // Obtener información del usuario
            var usuario = await _usuarioReadRepository.GetReadModelByIdAsync(traspaso.UsuarioId, cancellationToken);

            if (usuario == null)
            {
                _logger.LogWarning("No se pudo obtener el usuario {UsuarioId} para enviar email", traspaso.UsuarioId);
                return;
            }

            var emailBody = $@"
            <html>
                <body style='font-family: Arial, sans-serif; font-size: 16px; color: #333; line-height: 1.6;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 8px;'>
                        
                        <h1 style='color: #2196F3; text-align: center;'>Traspaso Programado Ejecutado</h1>
                        
                        <p>Hola <strong>{usuario.Nombre}</strong>,</p>
                        
                        <p>Te informamos que se ha ejecutado exitosamente un traspaso programado en tu cuenta de <strong>Kash</strong>.</p>
                        
                        <div style='background-color: #f5f5f5; padding: 15px; border-radius: 4px; margin: 20px 0;'>
                            <h3 style='margin-top: 0; color: #555;'>Detalles del Traspaso:</h3>
                            <ul style='list-style: none; padding: 0;'>
                                <li><strong>Importe:</strong> {traspaso.Importe:N2}€</li>
                                <li><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</li>
                                <li><strong>Frecuencia:</strong> {traspaso.Frecuencia}</li>
                                {(string.IsNullOrWhiteSpace(traspaso.Descripcion) ? "" : $"<li><strong>Descripción:</strong> {traspaso.Descripcion}</li>")}
                            </ul>
                        </div>
                        
                        <p style='font-size: 14px; color: #777;'>
                            Este es un mensaje automático. Si no esperabas este traspaso, por favor revisa la configuración de tus operaciones programadas en Kash.
                        </p>
                    </div>
                </body>
            </html>";

            var emailMessage = new EmailMessage(
                usuario.Correo,
                "Traspaso Programado Ejecutado - Kash",
                emailBody
            );

            _emailService.EnqueueEmail(emailMessage);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Email de notificación enviado a {Email} para TraspasoProgramado {Id}", 
                    usuario.Correo, traspaso.Id);
            }
        }
        catch (Exception ex)
        {
            // No fallar la operación si el email falla
            _logger.LogError(ex, "Error al enviar email de notificación para TraspasoProgramado {Id}", traspaso.Id);
        }
    }
}

