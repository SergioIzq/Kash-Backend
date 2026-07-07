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
    private readonly IEntityDependencyOrchestrator _dependencyOrchestrator;

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
        IJobSchedulingService jobSchedulingService,
        IEntityDependencyOrchestrator dependencyOrchestrator)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _categoriaFinderService = categoriaFinderService;
        _conceptoFinderService = conceptoFinderService;
        _proveedorFinderService = proveedorFinderService;
        _personaFinderService = personaFinderService;
        _cuentaFinderService = cuentaFinderService;
        _formaPagoFinderService = formaPagoFinderService;
        _jobSchedulingService = jobSchedulingService;
        _dependencyOrchestrator = dependencyOrchestrator;
    }

    /// <summary>
    /// 🔥 HOOK 1: Preparación de dependencias.
    /// Busca o crea entidades relacionadas.
    /// ORDEN IMPORTANTE: Categoría PRIMERO, luego Concepto con esa CategoríaId.
    /// </summary>
    protected override Task<Result<Dictionary<string, object>>> PrepareDependenciesAsync(
        UpdateGastoProgramadoCommand command,
        CancellationToken cancellationToken)
    {
        var usuarioId = _userContext.UserId ?? throw new InvalidOperationException("Usuario no autenticado");

        var steps = new List<DependencyStep>
        {
            new(
                Key: "CategoriaId",
                Id: command.CategoriaId,
                Nombre: command.CategoriaNombre,
                FindOrCreateAsync: _categoriaFinderService.FindOrCreateAsync,
                ToDependencyValue: id => CategoriaId.Create(id).Value,
                RequiredErrorMessage: "Se requiere una Categoría para el concepto."),

            new(
                Key: "ConceptoId",
                Id: command.ConceptoId,
                Nombre: command.ConceptoNombre,
                FindOrCreateAsync: _conceptoFinderService.FindOrCreateAsync,
                ToDependencyValue: id => ConceptoId.Create(id).Value,
                RequiredErrorMessage: "Se requiere un Concepto para actualizar el gasto programado.",
                AdditionalData: resolved => new Dictionary<string, object> { { "CategoriaId", resolved["CategoriaId"] } }),

            new(
                Key: "ProveedorId",
                Id: command.ProveedorId,
                Nombre: command.ProveedorNombre,
                FindOrCreateAsync: _proveedorFinderService.FindOrCreateAsync,
                ToDependencyValue: id => ProveedorId.Create(id).Value,
                Required: false),

            new(
                Key: "PersonaId",
                Id: command.PersonaId,
                Nombre: command.PersonaNombre,
                FindOrCreateAsync: _personaFinderService.FindOrCreateAsync,
                ToDependencyValue: id => PersonaId.Create(id).Value,
                Required: false),

            new(
                Key: "CuentaId",
                Id: command.CuentaId,
                Nombre: command.CuentaNombre,
                FindOrCreateAsync: _cuentaFinderService.FindOrCreateAsync,
                ToDependencyValue: id => CuentaId.Create(id).Value,
                RequiredErrorMessage: "Se requiere una Cuenta para actualizar el gasto programado."),

            new(
                Key: "FormaPagoId",
                Id: command.FormaPagoId,
                Nombre: command.FormaPagoNombre,
                FindOrCreateAsync: _formaPagoFinderService.FindOrCreateAsync,
                ToDependencyValue: id => FormaPagoId.Create(id).Value,
                RequiredErrorMessage: "Se requiere una Forma de Pago para actualizar el gasto programado."),
        };

        return _dependencyOrchestrator.ResolveAsync(usuarioId, steps, cancellationToken);
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



