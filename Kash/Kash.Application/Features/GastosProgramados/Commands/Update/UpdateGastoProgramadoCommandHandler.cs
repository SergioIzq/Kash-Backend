using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Application.Interfaces;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.Interfaces;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.GastosProgramados.Commands;

/// <summary>
/// ✅ REFACTORIZADO: Handler para actualizar gastos programados usando hooks.
/// Maneja automáticamente la reprogramación del job en Hangfire.
/// </summary>
public sealed class UpdateGastoProgramadoCommandHandler
    : AbsUpdateCommandHandler<GastoProgramado, GastoProgramadoId, GastoProgramadoDto, UpdateGastoProgramadoCommand>
{
    private readonly IDomainValidator _validator;
    private readonly IJobSchedulingService _jobSchedulingService;

    public UpdateGastoProgramadoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<GastoProgramado, GastoProgramadoId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext,
        IDomainValidator validator,
        IJobSchedulingService jobSchedulingService)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _validator = validator;
        _jobSchedulingService = jobSchedulingService;
    }

    /// <summary>
    /// 🔥 HOOK 1: Validación de dependencias en paralelo.
    /// </summary>
    protected override async Task<Result> ValidateBeforeUpdateAsync(
        UpdateGastoProgramadoCommand command,
        CancellationToken cancellationToken)
    {
        var validationTasks = new[]
        {
            _validator.ExistsAsync<Concepto, ConceptoId>(ConceptoId.Create(command.ConceptoId).Value),
            _validator.ExistsAsync<Cuenta, CuentaId>(CuentaId.Create(command.CuentaId).Value),
            _validator.ExistsAsync<FormaPago, FormaPagoId>(FormaPagoId.Create(command.FormaPagoId).Value),
            _validator.ExistsAsync<Proveedor, ProveedorId>(ProveedorId.Create(command.ProveedorId).Value),
            _validator.ExistsAsync<Persona, PersonaId>(PersonaId.Create(command.PersonaId).Value)
        };

        var results = await Task.WhenAll(validationTasks);

        if (results.Any(r => !r))
        {
            return Result.Failure(
                Error.NotFound("Una o más entidades referenciadas no existen."));
        }

        return Result.Success();
    }

    /// <summary>
    /// 🔥 HOOK 2: Aplica los cambios a la entidad.
    /// </summary>
    protected override void ApplyChanges(
        GastoProgramado entity,
        UpdateGastoProgramadoCommand command,
        Dictionary<string, object>? dependencies = null)
    {
        // Value Objects
        var importeVO = Cantidad.Create(command.Importe).Value;
        var frecuenciaVO = Frecuencia.Create(command.Frecuencia).Value;
        var descripcionVO = string.IsNullOrWhiteSpace(command.Descripcion)
            ? (Descripcion?)null
            : new Descripcion(command.Descripcion);

        // IDs
        var conceptoIdVO = ConceptoId.Create(command.ConceptoId).Value;
        var proveedorIdVO = ProveedorId.Create(command.ProveedorId).Value;
        var personaIdVO = PersonaId.Create(command.PersonaId).Value;
        var cuentaIdVO = CuentaId.Create(command.CuentaId).Value;
        var formaPagoIdVO = FormaPagoId.Create(command.FormaPagoId).Value;

        // Aplicar cambios
        var result = entity.Update(
            importeVO,
            command.FechaEjecucion!.Value,
            conceptoIdVO,
            proveedorIdVO,
            personaIdVO,
            cuentaIdVO,
            formaPagoIdVO,
            frecuenciaVO,
            command.Activo,
            descripcionVO
        );

        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }

    /// <summary>
    /// 🔥 HOOK 3: Reprograma el job en Hangfire después de actualizar.
    /// Se ejecuta solo si la actualización fue exitosa.
    /// </summary>
    protected override async Task OnEntityUpdatedAsync(
        GastoProgramado entity,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        // 1. Eliminar el job anterior
        await _jobSchedulingService.RemoveRecurringJobAsync(entity.HangfireJobId);

        // 2. Si está activo, reprogramar con los nuevos datos
        if (entity.Activo)
        {
            await _jobSchedulingService.ScheduleRecurringJobAsync(
                entity.HangfireJobId,
                entity.FechaEjecucion,
                entity.Frecuencia.Value,
                () => ExecuteGastoProgramadoAsync(entityId));
        }
        // Si no está activo, el job queda eliminado (pausado)
    }

    /// <summary>
    /// Método que ejecutará Hangfire periódicamente.
    /// </summary>
    private Task ExecuteGastoProgramadoAsync(Guid gastoProgramadoId)
    {
        // Esta lógica dispararía un comando MediatR: ExecuteGastoProgramadoCommand
        return Task.CompletedTask;
    }
}


