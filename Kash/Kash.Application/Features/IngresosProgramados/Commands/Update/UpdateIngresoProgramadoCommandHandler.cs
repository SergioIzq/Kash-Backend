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

namespace Kash.Application.Features.IngresosProgramados.Commands;

/// <summary>
/// ✅ REFACTORIZADO: Handler con auto-creación de entidades relacionadas.
/// 🔥 Auto-crea: Categoria, Concepto, Cliente, Persona, Cuenta, FormaPago si no existen.
/// 🔥 Cliente y Persona son OPCIONALES.
/// 🔥 Reprograma el job en Hangfire si cambia la frecuencia o fecha.
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
        IJobSchedulingService jobSchedulingService)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _categoriaFinderService = categoriaFinderService;
        _conceptoFinderService = conceptoFinderService;
        _clienteFinderService = clienteFinderService;
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
        UpdateIngresoProgramadoCommand command,
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
                    Error.Validation("Se requiere un Concepto para actualizar el ingreso programado."));
            }

            dependencies["ConceptoId"] = ConceptoId.Create(conceptoGuid.Value).Value;

            // 2. 🔥 CLIENTE: Buscar o crear (OPCIONAL)
            var clienteGuid = await _clienteFinderService.FindOrCreateAsync(
                command.ClienteId,
                command.ClienteNombre,
                usuarioId,
                cancellationToken: cancellationToken);

            if (clienteGuid.HasValue)
            {
                dependencies["ClienteId"] = ClienteId.Create(clienteGuid.Value).Value;
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
                    Error.Validation("Se requiere una Cuenta para actualizar el ingreso programado."));
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
                    Error.Validation("Se requiere una Forma de Pago para actualizar el ingreso programado."));
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
    /// 🔥 HOOK 2: Indica que las dependencias deben guardarse ANTES de actualizar el IngresoProgramado.
    /// Esto evita problemas de concurrencia cuando se auto-crean múltiples entidades relacionadas.
    /// </summary>
    protected override bool ShouldPersistDependenciesFirst()
    {
        return true; // ✅ ACTIVAR persistencia previa para evitar DbUpdateConcurrencyException
    }

    /// <summary>
    /// 🔥 HOOK 3: Aplica los cambios del comando a la entidad.
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
    /// 🔥 HOOK 4: Acciones post-actualización.
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


