using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;
using Mapster;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using AhorroLand.Shared.Application.Interfaces;

namespace AhorroLand.Application.Features.TraspasosProgramados.Commands;

public sealed class CreateTraspasoProgramadoCommandHandler
    : AbsCreateCommandHandler<TraspasoProgramado, TraspasoProgramadoId, CreateTraspasoProgramadoCommand>
{
    private readonly IDomainValidator _validator;
    private readonly IJobSchedulingService _jobSchedulingService;

    public CreateTraspasoProgramadoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<TraspasoProgramado, TraspasoProgramadoId> writeRepository,
        ICacheService cacheService,
        IDomainValidator validator,
        IJobSchedulingService jobSchedulingService,
        IUserContext userContext)
    : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _validator = validator;
        _jobSchedulingService = jobSchedulingService;
    }

    public override async Task<Result<Guid>> Handle(
        CreateTraspasoProgramadoCommand command, CancellationToken cancellationToken)
    {
        // 1. VALIDACIÓN ASÍNCRONA EN PARALELO (Máxima Optimización I/O)
        var validationTasks = new[]
        {
            // Validaciones obligatorias
            _validator.ExistsAsync < Cuenta, CuentaId >(CuentaId.Create(command.CuentaOrigenId).Value),
            _validator.ExistsAsync < Cuenta, CuentaId >(CuentaId.Create(command.CuentaDestinoId).Value),
        };

        // Espera a que todas las consultas terminen al mismo tiempo.
        var results = await Task.WhenAll(validationTasks);

        // 2. CHEQUEO RÁPIDO DE ERRORES DE EXISTENCIA
        if (results.Any(r => !r))
        {
            // Retorno de error con el mensaje de Error.NotFound
            return Result.Failure<Guid>(
                Error.NotFound("Una o más entidades referenciadas (CuentaOrigen, CuentaDestino) no existen."));
        }

        // 3. CREACIÓN DE VALUE OBJECTS (VOs) DENTRO DE UN TRY-CATCH
        // Volvemos al try-catch para manejar las ArgumentException lanzadas por los VOs.
        try
        {
            // Creación de VOs, que ahora son los que lanzan ArgumentException
            var importe = Cantidad.Create(command.Importe).Value;
            var frecuencia = Frecuencia.Create(command.Frecuencia).Value;
            var descripcion = new Descripcion(command.Descripcion ?? string.Empty);

            // Creamos VOs de Identidad
            var cuentaOrigenId = CuentaId.Create(command.CuentaOrigenId).Value;
            var cuentaDestinoId = CuentaId.Create(command.CuentaDestinoId).Value;
            var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

            // Uso del servicio de infraestructura para generar el JobId
            var hangfireJobId = _jobSchedulingService.GenerateJobId();

            // 4. CREACIÓN DE LA ENTIDAD DE DOMINIO (TraspasoProgramado)
            var traspasoProgramadoResult = TraspasoProgramado.Create(
                cuentaOrigenId,
                cuentaDestinoId,
                importe,
                command.FechaEjecucion,
                frecuencia,
                usuarioId,
                hangfireJobId,
                descripcion
            );

            if (traspasoProgramadoResult.IsFailure)
            {
                return Result.Failure<Guid>(traspasoProgramadoResult.Error);
            }

            // 5. PERSISTENCIA
            _writeRepository.Add(traspasoProgramadoResult.Value);
            var entityResult = await base.CreateAsync(traspasoProgramadoResult.Value, cancellationToken);

            if (entityResult.IsFailure)
            {
                return Result.Failure<Guid>(entityResult.Error);
            }

            return Result.Success(entityResult.Value);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Guid>(Error.Validation(ex.Message));
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(Error.Failure("Error.Unexpected", "Error Inesperado", ex.Message));
        }
    }

    protected override TraspasoProgramado CreateEntity(CreateTraspasoProgramadoCommand command)
    {
        throw new NotImplementedException("CreateEntity no debe usarse. La lógica de creación asíncrona reside en el método Handle.");
    }
}

