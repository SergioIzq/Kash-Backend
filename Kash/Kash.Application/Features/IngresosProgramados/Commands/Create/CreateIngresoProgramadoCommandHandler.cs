using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Interfaces;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.Interfaces;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;
using MediatR;

namespace Kash.Application.Features.IngresosProgramados.Commands;

/// <summary>
/// ✅ REFACTORIZADO: Handler para ingresos programados con auto-creación de entidades.
/// Reducido de ~120 líneas a ~110 líneas.
/// 🔥 Auto-crea: Categoria, Concepto, Cliente, Persona, Cuenta, FormaPago si no existen.
/// 🔥 Cliente y Persona son OPCIONALES.
/// 🔥 Programa el job en Hangfire después de persistir.
/// </summary>
public sealed class CreateIngresoProgramadoCommandHandler
    : AbsCreateCommandHandler<IngresoProgramado, IngresoProgramadoId, CreateIngresoProgramadoCommand>
{
    private readonly ICategoriaFinderOrCreatorService _categoriaFinderService;
    private readonly IConceptoFinderOrCreatorService _conceptoFinderService;
    private readonly IClienteFinderOrCreatorService _clienteFinderService;
    private readonly IPersonaFinderOrCreatorService _personaFinderService;
    private readonly ICuentaFinderOrCreatorService _cuentaFinderService;
    private readonly IFormaPagoFinderOrCreatorService _formaPagoFinderService;
    private readonly IJobSchedulingService _jobSchedulingService;
    private readonly IMediator _mediator;
    private readonly IEntityDependencyOrchestrator _dependencyOrchestrator;

    public CreateIngresoProgramadoCommandHandler(
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
        IMediator mediator,
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
        _mediator = mediator;
        _dependencyOrchestrator = dependencyOrchestrator;
    }

    /// <summary>
    /// 🔥 HOOK 1: Preparación de dependencias.
    /// Busca o crea entidades relacionadas + genera HangfireJobId.
    /// ORDEN IMPORTANTE: Categoría PRIMERO, luego Concepto con esa CategoríaId.
    /// </summary>
    protected override async Task<Result<Dictionary<string, object>>> PrepareDependenciesAsync(
        CreateIngresoProgramadoCommand command,
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
                RequiredErrorMessage: "Se requiere una Categoría para crear el concepto."),

            new(
                Key: "ConceptoId",
                Id: command.ConceptoId,
                Nombre: command.ConceptoNombre,
                FindOrCreateAsync: _conceptoFinderService.FindOrCreateAsync,
                ToDependencyValue: id => ConceptoId.Create(id).Value,
                RequiredErrorMessage: "Se requiere un Concepto para crear el ingreso programado.",
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
                RequiredErrorMessage: "Se requiere una Cuenta para crear el ingreso programado."),

            new(
                Key: "FormaPagoId",
                Id: command.FormaPagoId,
                Nombre: command.FormaPagoNombre,
                FindOrCreateAsync: _formaPagoFinderService.FindOrCreateAsync,
                ToDependencyValue: id => FormaPagoId.Create(id).Value,
                RequiredErrorMessage: "Se requiere una Forma de Pago para crear el ingreso programado."),
        };

        var dependenciesResult = await _dependencyOrchestrator.ResolveAsync(usuarioId, steps, cancellationToken);
        if (dependenciesResult.IsFailure)
        {
            return dependenciesResult;
        }

        // El HangfireJobId no es una dependencia buscable: se genera aparte de la orquestación.
        dependenciesResult.Value["HangfireJobId"] = _jobSchedulingService.GenerateJobId();

        return dependenciesResult;
    }

    /// <summary>
    /// 🔥 HOOK 2: Indica que las dependencias deben guardarse ANTES de crear el IngresoProgramado.
    /// Esto evita problemas de concurrencia cuando se auto-crean múltiples entidades relacionadas.
    /// </summary>
    protected override bool ShouldPersistDependenciesFirst()
    {
        return true; // ✅ ACTIVAR persistencia previa para evitar DbUpdateConcurrencyException
    }

    /// <summary>
    /// 🔥 HOOK 3: Crea la entidad de dominio con las dependencias preparadas.
    /// </summary>
    protected override IngresoProgramado CreateEntity(
        CreateIngresoProgramadoCommand command,
        Dictionary<string, object>? dependencies = null)
    {
        // Value Objects
        var importe = Cantidad.Create(command.Importe).Value;
        var frecuencia = Frecuencia.Create(command.Frecuencia).Value;
        var descripcion = new Descripcion(command.Descripcion ?? string.Empty);

        // IDs desde las dependencias preparadas (pueden haber sido creados)
        var conceptoId = (ConceptoId)dependencies!["ConceptoId"];
        var cuentaId = (CuentaId)dependencies["CuentaId"];
        var formaPagoId = (FormaPagoId)dependencies["FormaPagoId"];

        // Cliente y Persona: Si no existen en dependencies, crear IDs vacíos
        var clienteId = dependencies.ContainsKey("ClienteId") ? (ClienteId?)dependencies["ClienteId"] : null;
        var personaId = dependencies.ContainsKey("PersonaId") ? (PersonaId?)dependencies["PersonaId"] : null;
        var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

        // HangfireJobId desde las dependencias
        var hangfireJobId = (string)dependencies["HangfireJobId"];

        // Creación de la entidad programada
        return IngresoProgramado.Create(
            importe,
            command.FechaEjecucion!.Value,
            conceptoId,
            clienteId,
            frecuencia,
            personaId,
            cuentaId,
            formaPagoId,
            hangfireJobId,
            usuarioId,
            descripcion
        );
    }

    /// <summary>
    /// 🔥 HOOK 4: Acciones post-persistencia.
    /// Programa el job recurrente en Hangfire DESPUÉS de guardar exitosamente.
    /// </summary>
    protected override async Task OnEntityCreatedAsync(
        IngresoProgramado entity,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        // Solo programar si está activo
        if (entity.Activo)
        {
            var cronExpression = ConvertToCronExpression(entity.Frecuencia.Value, entity.FechaEjecucion);
            await _jobSchedulingService.ScheduleRecurringJobAsync(
                entity.HangfireJobId,
                entity.FechaEjecucion,
                cronExpression,
                () => ExecuteIngresoProgramadoAsync(new Guid(entity.HangfireJobId)));
        }
    }

    /// <summary>
    /// Convierte el valor de Frecuencia (Diario, Semanal, etc.) a una expresión cron válida.
    /// Respeta la hora y minutos de FechaEjecucion.
    /// </summary>
    private static string ConvertToCronExpression(string frecuencia, DateTime fechaEjecucion)
    {
        var minuto = fechaEjecucion.Minute;
        var hora = fechaEjecucion.Hour;
        var dia = fechaEjecucion.Day;
        var mes = fechaEjecucion.Month;
        var diaSemana = (int)fechaEjecucion.DayOfWeek;

        return frecuencia.ToLowerInvariant() switch
        {
            "diario" => $"{minuto} {hora} * * *",                    // Todos los días a la hora especificada
            "semanal" => $"{minuto} {hora} * * {diaSemana}",         // Mismo día de la semana a la hora especificada
            "mensual" => $"{minuto} {hora} {dia} * *",               // Mismo día del mes a la hora especificada
            "anual" => $"{minuto} {hora} {dia} {mes} *",             // Mismo día y mes del año a la hora especificada
            _ => throw new ArgumentException($"Frecuencia no soportada: {frecuencia}")
        };
    }

    /// <summary>
    /// Método que ejecutará Hangfire periódicamente.
    /// Envía el comando ExecuteIngresoProgramadoCommand a través de MediatR.
    /// 🔥 IMPORTANTE: Debe ser PUBLIC para que Hangfire pueda invocarlo.
    /// </summary>
    public async Task ExecuteIngresoProgramadoAsync(Guid hangfireId)
    {
        await _mediator.Send(new Execute.ExecuteIngresoProgramadoCommand(hangfireId));
    }
}
