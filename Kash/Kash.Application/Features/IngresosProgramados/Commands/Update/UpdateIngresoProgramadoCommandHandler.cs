using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Application.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.IngresosProgramados.Commands;

/// <summary>
/// REFACTORIZADO: Handler con auto-creación de entidades relacionadas.
/// Auto-crea: Categoria, Concepto, Cliente, Persona, Cuenta, FormaPago si no existen.
/// Cliente y Persona son OPCIONALES.
/// Reprograma el job en Hangfire si cambia la frecuencia o fecha.
/// </summary>
public sealed class UpdateIngresoProgramadoCommandHandler
    : AbsUpdateCommandHandler<IngresoProgramado, IngresoProgramadoId, IngresoProgramadoDto, UpdateIngresoProgramadoCommand>
{
    private readonly ICategoriaFinderOrCreatorService _categoriaFinderService;
    private readonly IConceptoFinderOrCreatorService _conceptoFinderService;
    private readonly IClienteFinderOrCreatorService _clienteFinderService;
    private readonly IPersonaFinderOrCreatorService _personaFinderService;
    private readonly ICuentaFinderOrCreatorService _cuentaFinderService;
    private readonly IFormaPagoFinderOrCreatorService _formaPagoFinderService;
    private readonly IJobSchedulingService _jobSchedulingService;
    private readonly IEntityDependencyOrchestrator _dependencyOrchestrator;

    public UpdateIngresoProgramadoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<IngresoProgramado, IngresoProgramadoId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext,
        ICategoriaFinderOrCreatorService categoriaFinderService,
        IConceptoFinderOrCreatorService conceptoFinderService,
        IClienteFinderOrCreatorService clienteFinderService,
        IPersonaFinderOrCreatorService personaFinderService,
        ICuentaFinderOrCreatorService cuentaFinderService,
        IFormaPagoFinderOrCreatorService formaPagoFinderService,
        IJobSchedulingService jobSchedulingService,
        IEntityDependencyOrchestrator dependencyOrchestrator)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _categoriaFinderService = categoriaFinderService;
        _conceptoFinderService = conceptoFinderService;
        _clienteFinderService = clienteFinderService;
        _personaFinderService = personaFinderService;
        _cuentaFinderService = cuentaFinderService;
        _formaPagoFinderService = formaPagoFinderService;
        _jobSchedulingService = jobSchedulingService;
        _dependencyOrchestrator = dependencyOrchestrator;
    }

    /// <summary>
    /// HOOK 1: Preparación de dependencias.
    /// Busca o crea entidades relacionadas.
    /// ORDEN IMPORTANTE: Categoría PRIMERO, luego Concepto con esa CategoríaId.
    /// </summary>
    protected override Task<Result<Dictionary<string, object>>> PrepareDependenciesAsync(
        UpdateIngresoProgramadoCommand command,
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
                RequiredErrorMessage: "Se requiere un Concepto para actualizar el ingreso programado.",
                AdditionalData: resolved => new Dictionary<string, object> { { "CategoriaId", resolved["CategoriaId"] } }),

            new(
                Key: "ClienteId",
                Id: command.ClienteId,
                Nombre: command.ClienteNombre,
                FindOrCreateAsync: _clienteFinderService.FindOrCreateAsync,
                ToDependencyValue: id => ClienteId.Create(id).Value,
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
                RequiredErrorMessage: "Se requiere una Cuenta para actualizar el ingreso programado."),

            new(
                Key: "FormaPagoId",
                Id: command.FormaPagoId,
                Nombre: command.FormaPagoNombre,
                FindOrCreateAsync: _formaPagoFinderService.FindOrCreateAsync,
                ToDependencyValue: id => FormaPagoId.Create(id).Value,
                RequiredErrorMessage: "Se requiere una Forma de Pago para actualizar el ingreso programado."),
        };

        return _dependencyOrchestrator.ResolveAsync(usuarioId, steps, cancellationToken);
    }

    /// <summary>
    /// HOOK 2: Indica que las dependencias deben guardarse ANTES de actualizar el IngresoProgramado.
    /// Esto evita problemas de concurrencia cuando se auto-crean múltiples entidades relacionadas.
    /// </summary>
    protected override bool ShouldPersistDependenciesFirst()
    {
        return true; // ACTIVAR persistencia previa para evitar DbUpdateConcurrencyException
    }

    /// <summary>
    /// HOOK 3: Aplica los cambios del comando a la entidad.
    /// Usa las dependencias preparadas (que pueden haber sido creadas).
    /// </summary>
    protected override void ApplyChanges(
        IngresoProgramado entity, 
        UpdateIngresoProgramadoCommand command, 
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

        // Cliente y Persona: Si no existen en dependencies, crear IDs vacíos
        var clienteId = dependencies.ContainsKey("ClienteId") 
            ? (ClienteId)dependencies["ClienteId"] 
            : ClienteId.Create(Guid.Empty).Value;

        var personaId = dependencies.ContainsKey("PersonaId") 
            ? (PersonaId)dependencies["PersonaId"] 
            : PersonaId.Create(Guid.Empty).Value;

        // Llamar al método Update de la entidad
        var result = entity.Update(
            importeVO,
            command.FechaEjecucion,
            conceptoId,
            clienteId,
            personaId,
            cuentaId,
            formaPagoId,
            frecuenciaVO,
            command.Activo,
            descripcionVO
        );

        // Si el resultado es un fallo, lanzar excepción
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }

    /// <summary>
    /// HOOK 4: Acciones post-actualización.
    /// Reprograma el job en Hangfire si cambia la frecuencia o está activo.
    /// </summary>
    protected override async Task OnEntityUpdatedAsync(
        IngresoProgramado entity,
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
                () => ExecuteIngresoProgramadoAsync(entityId));
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
    private Task ExecuteIngresoProgramadoAsync(Guid ingresoProgramadoId)
    {
        // TODO: Implementar ejecución del ingreso programado
        return Task.CompletedTask;
    }
}


