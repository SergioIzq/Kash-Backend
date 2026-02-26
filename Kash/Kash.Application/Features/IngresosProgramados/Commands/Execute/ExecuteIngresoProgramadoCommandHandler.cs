using Kash.Application.Features.Ingresos.Commands;
using Kash.Application.Interfaces.Repositories;
using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging;
using Kash.Shared.Application.Abstractions.Services;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kash.Application.Features.IngresosProgramados.Commands.Execute;

/// <summary>
/// Handler que ejecuta la lógica de negocio cuando Hangfire activa el job de un IngresoProgramado.
/// 🔥 NUEVO: Envía email de notificación al usuario después de ejecutar.
/// </summary>
public sealed class ExecuteIngresoProgramadoCommandHandler : ICommandHandler<ExecuteIngresoProgramadoCommand>
{
    private readonly IIngresoProgramadoReadRepository _ingresoProgramadoReadRepository;
    private readonly IReadRepository<Usuario, UsuarioDto, UsuarioId> _usuarioReadRepository;
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;
    private readonly ILogger<ExecuteIngresoProgramadoCommandHandler> _logger;

    public ExecuteIngresoProgramadoCommandHandler(
        IIngresoProgramadoReadRepository ingresoProgramadoReadRepository,
        IReadRepository<Usuario, UsuarioDto, UsuarioId> usuarioReadRepository,
        IMediator mediator,
        IEmailService emailService,
        ILogger<ExecuteIngresoProgramadoCommandHandler> logger)
    {
        _ingresoProgramadoReadRepository = ingresoProgramadoReadRepository;
        _usuarioReadRepository = usuarioReadRepository;
        _mediator = mediator;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> Handle(ExecuteIngresoProgramadoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Ejecutando IngresoProgramado {IngresoProgramadoId}", request.hangfireId);
            }

            var ingresoProgramado = await _ingresoProgramadoReadRepository.GetByHangfireJobIdAsync(
                                    request.hangfireId.ToString(),
                                    cancellationToken);

            if (ingresoProgramado == null)
            {
                _logger.LogWarning("IngresoProgramado {IngresoProgramadoId} no encontrado", request.hangfireId);
                return Result.Failure(Error.NotFound($"IngresoProgramado con ID {request.hangfireId} no encontrado"));
            }

            // 🔥 VALIDACIÓN: Si está inactivo, no ejecutar
            if (!ingresoProgramado.Activo)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("IngresoProgramado {IngresoProgramadoId} está inactivo, se omite la ejecución", request.hangfireId);
                }
                return Result.Success();
            }

            // 2. Crear el ingreso real
            var createIngresoCommand = new CreateIngresoCommand
            {
                Importe = ingresoProgramado.Importe,
                Fecha = DateTime.Now,
                ConceptoId = ingresoProgramado.ConceptoId,
                CategoriaId = ingresoProgramado.CategoriaId,
                ClienteId = ingresoProgramado.ClienteId,
                PersonaId = ingresoProgramado.PersonaId,
                CuentaId = ingresoProgramado.CuentaId,
                FormaPagoId = ingresoProgramado.FormaPagoId,
                UsuarioId = ingresoProgramado.UsuarioId,
                Descripcion = ingresoProgramado.Descripcion
            };

            var result = await _mediator.Send(createIngresoCommand, cancellationToken);

            if (result.IsSuccess)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Ingreso creado exitosamente desde IngresoProgramado {IngresoProgramadoId}", request.hangfireId);
                }

                // 🔥 NUEVO: Enviar email de notificación al usuario
                await EnviarEmailNotificacionAsync(ingresoProgramado, cancellationToken);
            }
            else
            {
                _logger.LogError("Error al crear Ingreso desde IngresoProgramado {IngresoProgramadoId}: {Error}",
                    request.hangfireId, result.Error);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al ejecutar IngresoProgramado {IngresoProgramadoId}", request.hangfireId);
            return Result.Failure(Error.Failure("Execute.IngresoProgramado", "Error de Ejecución", ex.Message));
        }
    }

    /// <summary>
    /// 🔥 NUEVO: Envía un email al usuario notificando que se ejecutó el ingreso programado.
    /// </summary>
    private async Task EnviarEmailNotificacionAsync(IngresoProgramadoDto ingreso, CancellationToken cancellationToken)
    {
        try
        {
            // Obtener información del usuario
            var usuario = await _usuarioReadRepository.GetReadModelByIdAsync(ingreso.UsuarioId, cancellationToken);

            if (usuario == null)
            {
                _logger.LogWarning("No se pudo obtener el usuario {UsuarioId} para enviar email", ingreso.UsuarioId);
                return;
            }

            var emailBody = $@"
            <html>
                <body style='font-family: Arial, sans-serif; font-size: 16px; color: #333; line-height: 1.6;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 8px;'>
                        
                        <h1 style='color: #4caf50; text-align: center;'>Ingreso Programado Ejecutado</h1>
                        
                        <p>Hola <strong>{usuario.Nombre}</strong>,</p>
                        
                        <p>Te informamos que se ha ejecutado exitosamente un ingreso programado en tu cuenta de <strong>Kash</strong>.</p>
                        
                        <div style='background-color: #e8f5e9; padding: 15px; border-radius: 4px; margin: 20px 0; border-left: 4px solid #4caf50;'>
                            <h3 style='margin-top: 0; color: #555;'>Detalles del Ingreso:</h3>
                            <ul style='list-style: none; padding: 0;'>
                                <li><strong>Importe:</strong> ${ingreso.Importe:N2}</li>
                                <li><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</li>
                                <li><strong>Frecuencia:</strong> {ingreso.Frecuencia}</li>
                                {(string.IsNullOrWhiteSpace(ingreso.Descripcion) ? "" : $"<li><strong>Descripción:</strong> {ingreso.Descripcion}</li>")}
                            </ul>
                        </div>
                        
                        <p style='font-size: 14px; color: #777;'>
                            Este es un mensaje automático. Si no esperabas este ingreso, por favor revisa la configuración de tus operaciones programadas en Kash.
                        </p>
                    </div>
                </body>
            </html>";

            var emailMessage = new EmailMessage(
                usuario.Correo,
                "Ingreso Programado Ejecutado - Kash",
                emailBody
            );

            _emailService.EnqueueEmail(emailMessage);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Email de notificación enviado a {Email} para IngresoProgramado {Id}", 
                    usuario.Correo, ingreso.Id);
            }
        }
        catch (Exception ex)
        {
            // No fallar la operación si el email falla
            _logger.LogError(ex, "Error al enviar email de notificación para IngresoProgramado {Id}", ingreso.Id);
        }
    }
}

