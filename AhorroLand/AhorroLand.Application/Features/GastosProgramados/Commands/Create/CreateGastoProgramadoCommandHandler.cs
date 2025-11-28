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

namespace AhorroLand.Application.Features.GastosProgramados.Commands;

public sealed class CreateGastoProgramadoCommandHandler
    : AbsCreateCommandHandler<GastoProgramado, GastoProgramadoId, CreateGastoProgramadoCommand>
{
    private readonly IDomainValidator _validator;
    private readonly IJobSchedulingService _jobSchedulingService;

    public CreateGastoProgramadoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<GastoProgramado, GastoProgramadoId> writeRepository,
        ICacheService cacheService,
        IDomainValidator validator,
        IJobSchedulingService jobSchedulingService)
    : base(unitOfWork, writeRepository, cacheService)
    {
        _validator = validator;
        _jobSchedulingService = jobSchedulingService;
    }

    public override async Task<Result<Guid>> Handle(
        CreateGastoProgramadoCommand command, CancellationToken cancellationToken)
    {
        // 1. VALIDACIÓN ASÍNCRONA EN PARALELO (Máxima Optimización I/O)
        var validationTasks = new[]
        {
            // Validaciones obligatorias
            _validator.ExistsAsync<Concepto, ConceptoId>(new ConceptoId(command.ConceptoId)),
            _validator.ExistsAsync<Cuenta, CuentaId>(new CuentaId(command.CuentaId)),
            _validator.ExistsAsync < FormaPago, FormaPagoId >(new FormaPagoId(command.FormaPagoId)),
            // Validaciones opcionales/contextuales
            _validator.ExistsAsync < Proveedor, ProveedorId >(new ProveedorId(command.ProveedorId)),
            _validator.ExistsAsync < Persona, PersonaId >(new PersonaId(command.PersonaId)),
        };

        // Espera a que todas las consultas terminen al mismo tiempo.
        var results = await Task.WhenAll(validationTasks);

        // 2. CHEQUEO RÁPIDO DE ERRORES DE EXISTENCIA
        if (results.Any(r => !r))
        {
            // Retorno de error con el mensaje de Error.NotFound
            return Result.Failure<Guid>(
                Error.NotFound("Una o más entidades referenciadas (Concepto, Cuenta, Proveedor, etc.) no existen."));
        }

        // 3. CREACIÓN DE VALUE OBJECTS (VOs) DENTRO DE UN TRY-CATCH
        // Volvemos al try-catch para manejar las ArgumentException lanzadas por los VOs.
        try
        {
            // Creación de VOs, que ahora son los que lanzan ArgumentException
            var importe = new Cantidad(command.Importe);
            var frecuencia = new Frecuencia(command.Frecuencia);
            var descripcion = new Descripcion(command.Descripcion ?? string.Empty);

            // Creamos VOs de Identidad
            var conceptoId = new ConceptoId(command.ConceptoId);
            var cuentaId = new CuentaId(command.CuentaId);
            var formaPagoId = new FormaPagoId(command.FormaPagoId);
            var proveedorId = new ProveedorId(command.ProveedorId);
            var categoriaId = new CategoriaId(command.CategoriaId);
            var personaId = new PersonaId(command.PersonaId);

            // Uso del servicio de infraestructura para generar el JobId
            var hangfireJobId = _jobSchedulingService.GenerateJobId();

            // 4. CREACIÓN DE LA ENTIDAD DE DOMINIO (GastoProgramado)
            var gastoProgramado = GastoProgramado.Create(
                importe,
                command.FechaEjecucion!.Value,
                conceptoId,
                proveedorId,
                frecuencia,
                personaId,
                cuentaId,
                formaPagoId,
                hangfireJobId,
                descripcion
            );

            // 5. PERSISTENCIA
            _writeRepository.Add(gastoProgramado);
            var entityResult = await base.CreateAsync(gastoProgramado, cancellationToken);

            if (entityResult.IsFailure)
            {
                return Result.Failure<Guid>(entityResult.Error);
            }

            // 6. MAPEO Y ÉXITO
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

    protected override GastoProgramado CreateEntity(CreateGastoProgramadoCommand command)
    {
        throw new NotImplementedException("CreateEntity no debe usarse. La lógica de creación asíncrona reside en el método Handle.");
    }
}
