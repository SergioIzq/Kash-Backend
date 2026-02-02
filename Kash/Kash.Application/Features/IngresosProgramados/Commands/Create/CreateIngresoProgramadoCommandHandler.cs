using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Interfaces;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.Interfaces;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.IngresosProgramados.Commands;

/// <summary>
/// ✅ REFACTORIZADO: Handler para ingresos programados usando hooks de la clase base.
/// Reducido de ~120 líneas a ~80 líneas (33% menos código).
/// </summary>
public sealed class CreateIngresoProgramadoCommandHandler
    : AbsCreateCommandHandler<IngresoProgramado, IngresoProgramadoId, CreateIngresoProgramadoCommand>
{
    private readonly IDomainValidator _validator;
    private readonly IJobSchedulingService _jobSchedulingService;

    public CreateIngresoProgramadoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<IngresoProgramado, IngresoProgramadoId> writeRepository,
        ICacheService cacheService,
        IDomainValidator validator,
        IJobSchedulingService jobSchedulingService,
        IUserContext userContext)
    : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _validator = validator;
        _jobSchedulingService = jobSchedulingService;
    }

    /// <summary>
    /// 🔥 HOOK 1: Validación pre-creación.
    /// Valida existencia de entidades relacionadas en paralelo.
    /// </summary>
    protected override async Task<Result> ValidateBeforeCreateAsync(
        CreateIngresoProgramadoCommand command,
        CancellationToken cancellationToken)
    {
        // Validación asíncrona en paralelo (Máxima Optimización I/O)
        var validationTasks = new[]
        {
            _validator.ExistsAsync<Concepto, ConceptoId>(ConceptoId.Create(command.ConceptoId).Value),
            _validator.ExistsAsync<Cuenta, CuentaId>(CuentaId.Create(command.CuentaId).Value),
            _validator.ExistsAsync<FormaPago, FormaPagoId>(FormaPagoId.Create(command.FormaPagoId).Value),
            _validator.ExistsAsync<Cliente, ClienteId>(ClienteId.Create(command.ClienteId).Value),
            _validator.ExistsAsync<Persona, PersonaId>(PersonaId.Create(command.PersonaId).Value)
        };

        var results = await Task.WhenAll(validationTasks);

        if (results.Any(r => !r))
        {
            return Result.Failure(
                Error.NotFound("Una o más entidades referenciadas (Concepto, Cuenta, Proveedor, etc.) no existen."));
        }

        return Result.Success();
    }

    /// <summary>
    /// 🔥 HOOK 2: Preparación de dependencias.
    /// Genera el HangfireJobId antes de crear la entidad.
    /// </summary>
    protected override Task<Result<Dictionary<string, object>>> PrepareDependenciesAsync(
        CreateIngresoProgramadoCommand command,
        CancellationToken cancellationToken)
    {
        var dependencies = new Dictionary<string, object>
        {
            ["HangfireJobId"] = _jobSchedulingService.GenerateJobId()
        };

        return Task.FromResult(Result.Success(dependencies));
    }

    /// <summary>
    /// 🔥 HOOK 3: Crea la entidad de dominio.
    /// </summary>
    protected override IngresoProgramado CreateEntity(
        CreateIngresoProgramadoCommand command,
        Dictionary<string, object>? dependencies = null)
    {
        // Value Objects
        var importe = Cantidad.Create(command.Importe).Value;
        var frecuencia = Frecuencia.Create(command.Frecuencia).Value;
        var descripcion = new Descripcion(command.Descripcion ?? string.Empty);

        // IDs
        var conceptoId = ConceptoId.Create(command.ConceptoId).Value;
        var cuentaId = CuentaId.Create(command.CuentaId).Value;
        var formaPagoId = FormaPagoId.Create(command.FormaPagoId).Value;
        var proveedorId = ClienteId.Create(command.ClienteId).Value;
        var categoriaId = CategoriaId.Create(command.CategoriaId).Value;
        var personaId = PersonaId.Create(command.PersonaId).Value;

        // HangfireJobId desde las dependencias
        var hangfireJobId = (string)dependencies!["HangfireJobId"];

        // Creación de la entidad
        return IngresoProgramado.Create(
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
    }
}

