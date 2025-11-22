using AhorroLand.Application.Features.Gastos.Commands;
using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AhorroLand.Application.Features.GastosProgramados.Commands.Execute;

/// <summary>
/// Handler que ejecuta la lógica de negocio cuando Hangfire activa el job de un GastoProgramado.
/// Optimizado para minimizar queries y allocations.
/// </summary>
public sealed class ExecuteGastoProgramadoCommandHandler : ICommandHandler<ExecuteGastoProgramadoCommand>
{
    private readonly IReadRepositoryWithDto<GastoProgramado, GastoProgramadoDto> _gastoProgramadoReadRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<ExecuteGastoProgramadoCommandHandler> _logger;

    public ExecuteGastoProgramadoCommandHandler(
        IReadRepositoryWithDto<GastoProgramado, GastoProgramadoDto> gastoProgramadoReadRepository,
        IMediator mediator,
        ILogger<ExecuteGastoProgramadoCommandHandler> logger)
    {
        _gastoProgramadoReadRepository = gastoProgramadoReadRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result> Handle(ExecuteGastoProgramadoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // ?? OPTIMIZACIÓN: Log estructurado (más eficiente que string interpolation)
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Ejecutando GastoProgramado {GastoProgramadoId}", request.GastoProgramadoId);
            }

            // 1. Obtener el GastoProgramado (AsNoTracking para mejor rendimiento)
            var gastoProgramado = await _gastoProgramadoReadRepository.GetReadModelByIdAsync(
                request.GastoProgramadoId,
                cancellationToken);

            if (gastoProgramado == null)
            {
                _logger.LogWarning("GastoProgramado {GastoProgramadoId} no encontrado", request.GastoProgramadoId);
                return Result.Failure(Error.NotFound($"GastoProgramado con ID {request.GastoProgramadoId} no encontrado"));
            }

            if (!gastoProgramado.Activo)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("GastoProgramado {GastoProgramadoId} está inactivo, se omite la ejecución", request.GastoProgramadoId);
                }
                return Result.Success();
            }

            // ?? OPTIMIZACIÓN: Crear el comando de forma más eficiente
            var descripcion = gastoProgramado.Descripcion;
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
                // ?? OPTIMIZACIÓN: Evitar string interpolation si no es necesario
                Descripcion = !string.IsNullOrEmpty(descripcion)
                    ? descripcion
                    : $"Gasto automático desde programación {gastoProgramado.Id}"
            };

            var result = await _mediator.Send(createGastoCommand, cancellationToken);

            if (result.IsSuccess)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Gasto creado exitosamente desde GastoProgramado {GastoProgramadoId}", request.GastoProgramadoId);
                }
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
}

