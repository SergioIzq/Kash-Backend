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
/// ✅ REFACTORIZADO: Handler con auto-creación de entidades relacionadas.
/// 🔥 Auto-crea: Categoria, Concepto, Proveedor, Persona, Cuenta, FormaPago si no existen.
/// 🔥 Proveedor y Persona son OPCIONALES.
/// 🔥 Reprograma el job en Hangfire si cambia la frecuencia o fecha.
/// </summary>
public sealed class UpdateGastoProgramadoCommandHandler
    : AbsUpdateCommandHandler<GastoProgramado, GastoProgramadoId, GastoProgramadoDto, UpdateGastoProgramadoCommand>
{
    private readonly ICategoriaFinderOrCreatorService _categoriaFinderService;
    private readonly IConceptoFinderOrCreatorService _conceptoFinderService;
    private readonly IProveedorFinderOrCreatorService _proveedorFinderService;
    private readonly IPersonaFinderOrCreatorService _personaFinderService;
    private readonly ICuentaFinderOrCreatorService _cuentaFinderService;
    private readonly IFormaPagoFinderOrCreatorService _formaPagoFinderService;
    private readonly IJobSchedulingService _jobSchedulingService;

    public UpdateGastoProgramadoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<GastoProgramado, GastoProgramadoId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext,
        ICategoriaFinderOrCreatorService categoriaFinderService,
        IConceptoFinderOrCreatorService conceptoFinderService,
        IProveedorFinderOrCreatorService proveedorFinderService,
        IPersonaFinderOrCreatorService personaFinderService,
        ICuentaFinderOrCreatorService cuentaFinderService,
        IFormaPagoFinderOrCreatorService formaPagoFinderService,
        IJobSchedulingService jobSchedulingService)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _categoriaFinderService = categoriaFinderService;
        _conceptoFinderService = conceptoFinderService;
        _proveedorFinderService = proveedorFinderService;
        _personaFinderService = personaFinderService;
        _cuentaFinderService = cuentaFinderService;
        _formaPagoFinderService = formaPagoFinderService;
        _jobSchedulingService = jobSchedulingService;
    }

    /// <summary>
    /// 🔥 HOOK 1: Preparación de dependencias.
    /// Busca o crea entidades relacionadas.
    /// ORDEN IMPORTANTE: Categoría PRIMERO, luego Concepto con esa CategoríaId.
    /// </summary>
    protected override async Task<Result<Dictionary<string, object>>> PrepareDependenciesAsync(
        UpdateGastoProgramadoCommand command,
        CancellationToken cancellationToken)
    {
        var dependencies = new Dictionary<string, object>();

        try
        {
            var usuarioId = _userContext.UserId ?? throw new InvalidOperationException("Usuario no autenticado");

            // 0. 🔥 CATEGORÍA: Buscar o crear PRIMERO (obligatoria para Concepto)
            var categoriaGuid = await _categoriaFinderService.FindOrCreateAsync(
                command.CategoriaId,
                command.CategoriaNombre,
                usuarioId,
                cancellationToken: cancellationToken);

            if (categoriaGuid == null)
            {
                return Result.Failure<Dictionary<string, object>>(
                    Error.Validation("Se requiere una Categoría para el concepto."));
            }

            var categoriaId = CategoriaId.Create(categoriaGuid.Value).Value;
            dependencies["CategoriaId"] = categoriaId;

            // 1. 🔥 CONCEPTO: Buscar o crear con la CategoríaId (obligatorio)
            var conceptoGuid = await _conceptoFinderService.FindOrCreateAsync(
                command.ConceptoId,
                command.ConceptoNombre,
                usuarioId,
                new Dictionary<string, object> { { "CategoriaId", categoriaId.Value } },
                cancellationToken);

            if (conceptoGuid == null)
            {
                return Result.Failure<Dictionary<string, object>>(
                    Error.Validation("Se requiere un Concepto para actualizar el gasto programado."));
            }

            dependencies["ConceptoId"] = ConceptoId.Create(conceptoGuid.Value).Value;

            // 2. 🔥 PROVEEDOR: Buscar o crear (OPCIONAL)
            var proveedorGuid = await _proveedorFinderService.FindOrCreateAsync(
                command.ProveedorId,
                command.ProveedorNombre,
                usuarioId,
                cancellationToken: cancellationToken);

            if (proveedorGuid.HasValue)
            {
                dependencies["ProveedorId"] = ProveedorId.Create(proveedorGuid.Value).Value;
            }

            // 3. 🔥 PERSONA: Buscar o crear (OPCIONAL)
            var personaGuid = await _personaFinderService.FindOrCreateAsync(
                command.PersonaId,
                command.PersonaNombre,
                usuarioId,
                cancellationToken: cancellationToken);

            if (personaGuid.HasValue)
            {
                dependencies["PersonaId"] = PersonaId.Create(personaGuid.Value).Value;
            }

            // 4. 🔥 CUENTA: Buscar o crear (obligatorio)
            var cuentaGuid = await _cuentaFinderService.FindOrCreateAsync(
                command.CuentaId,
                command.CuentaNombre,
                usuarioId,
                cancellationToken: cancellationToken);

            if (cuentaGuid == null)
            {
                return Result.Failure<Dictionary<string, object>>(
                    Error.Validation("Se requiere una Cuenta para actualizar el gasto programado."));
            }

            dependencies["CuentaId"] = CuentaId.Create(cuentaGuid.Value).Value;

            // 5. 🔥 FORMA DE PAGO: Buscar o crear (obligatorio)
            var formaPagoGuid = await _formaPagoFinderService.FindOrCreateAsync(
                command.FormaPagoId,
                command.FormaPagoNombre,
                usuarioId,
                cancellationToken: cancellationToken);

            if (formaPagoGuid == null)
            {
                return Result.Failure<Dictionary<string, object>>(
                    Error.Validation("Se requiere una Forma de Pago para actualizar el gasto programado."));
            }

            dependencies["FormaPagoId"] = FormaPagoId.Create(formaPagoGuid.Value).Value;

            return Result.Success(dependencies);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Dictionary<string, object>>(Error.Validation(ex.Message));
        }
    }

    /// <summary>
    /// 🔥 HOOK 2: Indica que las dependencias deben guardarse ANTES de actualizar el GastoProgramado.
    /// Esto evita problemas de concurrencia cuando se auto-crean múltiples entidades relacionadas.
    /// </summary>
    protected override bool ShouldPersistDependenciesFirst()
    {
        return true; // ✅ ACTIVAR persistencia previa para evitar DbUpdateConcurrencyException
    }

    /// <summary>
    /// 🔥 HOOK 3: Aplica los cambios a la entidad.
    /// Usa las dependencias preparadas (que pueden haber sido creadas).
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

        // IDs desde las dependencias preparadas (pueden haber sido creados)
        var conceptoId = (ConceptoId)dependencies!["ConceptoId"];
        var cuentaId = (CuentaId)dependencies["CuentaId"];
        var formaPagoId = (FormaPagoId)dependencies["FormaPagoId"];

        // Proveedor y Persona: Si no existen en dependencies, crear IDs vacíos
        var proveedorId = dependencies.ContainsKey("ProveedorId") 
            ? (ProveedorId)dependencies["ProveedorId"] 
            : ProveedorId.Create(Guid.Empty).Value;

        var personaId = dependencies.ContainsKey("PersonaId") 
            ? (PersonaId)dependencies["PersonaId"] 
            : PersonaId.Create(Guid.Empty).Value;

        // Aplicar cambios
        var result = entity.Update(
            importeVO,
            command.FechaEjecucion!.Value,
            conceptoId,
            proveedorId,
            personaId,
            cuentaId,
            formaPagoId,
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
    /// 🔥 HOOK 4: Acciones post-actualización.
    /// Reprograma el job en Hangfire si cambia la frecuencia o está activo.
    /// </summary>
    protected override async Task OnEntityUpdatedAsync(
        GastoProgramado entity,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        // Reprogramar el job si está activo
        if (entity.Activo)
        {
            // Actualizar el job recurrente con la nueva frecuencia/fecha
            await _jobSchedulingService.UpdateRecurringJobAsync(
                entity.HangfireJobId,
                entity.FechaEjecucion,
                entity.Frecuencia.Value,
                () => ExecuteGastoProgramadoAsync(entityId));
        }
        else
        {
            // Si está inactivo, eliminar el job
            await _jobSchedulingService.RemoveRecurringJobAsync(entity.HangfireJobId);
        }
    }

    /// <summary>
    /// Método que ejecutará Hangfire periódicamente.
    /// </summary>
    private Task ExecuteGastoProgramadoAsync(Guid gastoProgramadoId)
    {
        // TODO: Implementar ejecución del gasto programado
        return Task.CompletedTask;
    }
}



