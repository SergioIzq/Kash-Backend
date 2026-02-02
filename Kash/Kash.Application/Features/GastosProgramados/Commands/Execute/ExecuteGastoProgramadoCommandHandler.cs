using Kash.Application.Features.Gastos.Commands;
using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging;
using Kash.Shared.Application.Abstractions.Services;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kash.Application.Features.GastosProgramados.Commands.Execute;

/// <summary>
/// Handler que ejecuta la lógica de negocio cuando Hangfire activa el job de un GastoProgramado.
/// 🔥 NUEVO: Envía email de notificación al usuario después de ejecutar.
/// </summary>
public sealed class ExecuteGastoProgramadoCommandHandler : ICommandHandler<ExecuteGastoProgramadoCommand>
{
    private readonly IReadRepository<GastoProgramado, GastoProgramadoDto, GastoProgramadoId> _gastoProgramadoReadRepository;
    private readonly IReadRepository<Usuario, UsuarioDto, UsuarioId> _usuarioReadRepository;
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;
    private readonly ILogger<ExecuteGastoProgramadoCommandHandler> _logger;

    public ExecuteGastoProgramadoCommandHandler(
        IReadRepository<GastoProgramado, GastoProgramadoDto, GastoProgramadoId> gastoProgramadoReadRepository,
        IReadRepository<Usuario, UsuarioDto, UsuarioId> usuarioReadRepository,
        IMediator mediator,
        IEmailService emailService,
        ILogger<ExecuteGastoProgramadoCommandHandler> logger)
    {
        _gastoProgramadoReadRepository = gastoProgramadoReadRepository;
        _usuarioReadRepository = usuarioReadRepository;
        _mediator = mediator;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> Handle(ExecuteGastoProgramadoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Ejecutando GastoProgramado {GastoProgramadoId}", request.GastoProgramadoId);
            }

            // 1. Obtener el GastoProgramado
            var gastoProgramado = await _gastoProgramadoReadRepository.GetReadModelByIdAsync(
                request.GastoProgramadoId,
                cancellationToken);

            if (gastoProgramado == null)
            {
                _logger.LogWarning("GastoProgramado {GastoProgramadoId} no encontrado", request.GastoProgramadoId);
                return Result.Failure(Error.NotFound($"GastoProgramado con ID {request.GastoProgramadoId} no encontrado"));
            }

            // 🔥 VALIDACIÓN: Si está inactivo, no ejecutar
            if (!gastoProgramado.Activo)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("GastoProgramado {GastoProgramadoId} está inactivo, se omite la ejecución", request.GastoProgramadoId);
                }
                return Result.Success();
            }

            // 2. Crear el gasto real
            var createGastoCommand = new CreateGastoCommand
            {
                Importe = gastoProgramado.Importe,
                Fecha = DateTime.Now,
                ConceptoId = gastoProgramado.ConceptoId,
                CategoriaId = gastoProgramado.CategoriaId,
                ProveedorId = gastoProgramado.ProveedorId,
                PersonaId = gastoProgramado.PersonaId,
                CuentaId = gastoProgramado.CuentaId,
                FormaPagoId = gastoProgramado.FormaPagoId,
                UsuarioId = gastoProgramado.UsuarioId,
                Descripcion = gastoProgramado.Descripcion
            };

            var result = await _mediator.Send(createGastoCommand, cancellationToken);

            if (result.IsSuccess)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Gasto creado exitosamente desde GastoProgramado {GastoProgramadoId}", request.GastoProgramadoId);
                }

                // 🔥 NUEVO: Enviar email de notificación al usuario
                await EnviarEmailNotificacionAsync(gastoProgramado, cancellationToken);
            }
            else
            {
                _logger.LogError("Error al crear Gasto desde GastoProgramado {GastoProgramadoId}: {Error}",
                    request.GastoProgramadoId, result.Error);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al ejecutar GastoProgramado {GastoProgramadoId}", request.GastoProgramadoId);
            return Result.Failure(Error.Failure("Execute.GastoProgramado", "Error de Ejecución", ex.Message));
        }
    }

    /// <summary>
    /// 🔥 NUEVO: Envía un email al usuario notificando que se ejecutó el gasto programado.
    /// </summary>
    private async Task EnviarEmailNotificacionAsync(GastoProgramadoDto gasto, CancellationToken cancellationToken)
    {
        try
        {
            // Obtener información del usuario
            var usuario = await _usuarioReadRepository.GetReadModelByIdAsync(gasto.UsuarioId, cancellationToken);

            if (usuario == null)
            {
                _logger.LogWarning("No se pudo obtener el usuario {UsuarioId} para enviar email", gasto.UsuarioId);
                return;
            }

            var emailBody = $@"
            <html>
                <body style='font-family: Arial, sans-serif; font-size: 16px; color: #333; line-height: 1.6;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 8px;'>
                        
                        <h1 style='color: #f44336; text-align: center;'>Gasto Programado Ejecutado</h1>
                        
                        <p>Hola <strong>{usuario.Nombre}</strong>,</p>
                        
                        <p>Te informamos que se ha ejecutado exitosamente un gasto programado en tu cuenta de <strong>Kash</strong>.</p>
                        
                        <div style='background-color: #fff3e0; padding: 15px; border-radius: 4px; margin: 20px 0; border-left: 4px solid #f44336;'>
                            <h3 style='margin-top: 0; color: #555;'>Detalles del Gasto:</h3>
                            <ul style='list-style: none; padding: 0;'>
                                <li><strong>Importe:</strong> ${gasto.Importe:N2}</li>
                                <li><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</li>
                                <li><strong>Frecuencia:</strong> {gasto.Frecuencia}</li>
                                {(string.IsNullOrWhiteSpace(gasto.Descripcion) ? "" : $"<li><strong>Descripción:</strong> {gasto.Descripcion}</li>")}
                            </ul>
                        </div>
                        
                        <p style='font-size: 14px; color: #777;'>
                            Este es un mensaje automático. Si no esperabas este gasto, por favor revisa la configuración de tus operaciones programadas en Kash.
                        </p>
                    </div>
                </body>
            </html>";

            var emailMessage = new EmailMessage(
                usuario.Correo,
                "Gasto Programado Ejecutado - Kash",
                emailBody
            );

            _emailService.EnqueueEmail(emailMessage);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Email de notificación enviado a {Email} para GastoProgramado {Id}", 
                    usuario.Correo, gasto.Id);
            }
        }
        catch (Exception ex)
        {
            // No fallar la operación si el email falla
            _logger.LogError(ex, "Error al enviar email de notificación para GastoProgramado {Id}", gasto.Id);
        }
    }
}

