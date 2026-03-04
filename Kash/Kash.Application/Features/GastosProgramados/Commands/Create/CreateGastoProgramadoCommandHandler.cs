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

namespace Kash.Application.Features.GastosProgramados.Commands;

/// <summary>
/// ✅ REFACTORIZADO: Handler para gastos programados con auto-creación de entidades.
/// Reducido de ~120 líneas a ~110 líneas.
/// 🔥 Auto-crea: Categoria, Concepto, Proveedor, Persona, Cuenta, FormaPago si no existen.
/// 🔥 Proveedor y Persona son OPCIONALES.
/// 🔥 Programa el job en Hangfire después de persistir.
/// </summary>
public sealed class CreateGastoProgramadoCommandHandler
    : AbsCreateCommandHandler<GastoProgramado, GastoProgramadoId, CreateGastoProgramadoCommand>
{
    private readonly ICategoriaFinderOrCreatorService _categoriaFinderService;
    private readonly IConceptoFinderOrCreatorService _conceptoFinderService;
    private readonly IProveedorFinderOrCreatorService _proveedorFinderService;
    private readonly IPersonaFinderOrCreatorService _personaFinderService;
    private readonly ICuentaFinderOrCreatorService _cuentaFinderService;
    private readonly IFormaPagoFinderOrCreatorService _formaPagoFinderService;
    private readonly IJobSchedulingService _jobSchedulingService;
    private readonly IMediator _mediator;

    public CreateGastoProgramadoCommandHandler(
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
        IMediator mediator)
    : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _categoriaFinderService = categoriaFinderService;
        _conceptoFinderService = conceptoFinderService;
        _proveedorFinderService = proveedorFinderService;
        _personaFinderService = personaFinderService;
        _cuentaFinderService = cuentaFinderService;
        _formaPagoFinderService = formaPagoFinderService;
        _jobSchedulingService = jobSchedulingService;
        _mediator = mediator;
    }

    /// <summary>
    /// 🔥 HOOK 1: Preparación de dependencias.
    /// Busca o crea entidades relacionadas + genera HangfireJobId.
    /// ORDEN IMPORTANTE: Categoría PRIMERO, luego Concepto con esa CategoríaId.
    /// </summary>
    protected override async Task<Result<Dictionary<string, object>>> PrepareDependenciesAsync(
        CreateGastoProgramadoCommand command,
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
                return Result.Failure<Dictionary<string, object>>(Error.Validation(
                    "Se requiere una Categoría para crear el concepto."));
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
                return Result.Failure<Dictionary<string, object>>(Error.Validation(
                    "Se requiere un Concepto para crear el gasto programado."));
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
                return Result.Failure<Dictionary<string, object>>(Error.Validation(
                    "Se requiere una Cuenta para crear el gasto programado."));
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
                return Result.Failure<Dictionary<string, object>>(Error.Validation(
                    "Se requiere una Forma de Pago para crear el gasto programado."));
            }

            dependencies["FormaPagoId"] = FormaPagoId.Create(formaPagoGuid.Value).Value;

            // 6. 🔥 HANGFIRE JOB ID: Generar para la programación
            dependencies["HangfireJobId"] = _jobSchedulingService.GenerateJobId();

            return Result.Success(dependencies);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Dictionary<string, object>>(Error.Validation(ex.Message));
        }
    }

    /// <summary>
    /// 🔥 HOOK 2: Indica que las dependencias deben guardarse ANTES de crear el GastoProgramado.
    /// Esto evita problemas de concurrencia cuando se auto-crean múltiples entidades relacionadas.
    /// </summary>
    protected override bool ShouldPersistDependenciesFirst()
    {
        return true; // ✅ ACTIVAR persistencia previa para evitar DbUpdateConcurrencyException
    }

    /// <summary>
    /// 🔥 HOOK 3: Crea la entidad de dominio con las dependencias preparadas.
    /// </summary>
    protected override GastoProgramado CreateEntity(
        CreateGastoProgramadoCommand command,
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

        // Proveedor y Persona: Si no existen en dependencies, crear IDs vacíos
        var proveedorId = dependencies.ContainsKey("proveedorId") ? (ProveedorId?)dependencies["ProveedorId"] : null;
        var personaId = dependencies.ContainsKey("PersonaId") ? (PersonaId?)dependencies["PersonaId"] : null;
        var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

        // HangfireJobId desde las dependencias
        var hangfireJobId = (string)dependencies["HangfireJobId"];

        // Creación de la entidad programada
        return GastoProgramado.Create(
            importe,
            command.FechaEjecucion!.Value,
            conceptoId,
            proveedorId,
            frecuencia,
            personaId,
            cuentaId,
            formaPagoId,
            usuarioId,
            hangfireJobId,
            descripcion
        );
    }

    /// <summary>
    /// 🔥 HOOK 4: Acciones post-persistencia.
    /// Programa el job recurrente en Hangfire DESPUÉS de guardar exitosamente.
    /// </summary>
    protected override async Task OnEntityCreatedAsync(
        GastoProgramado entity,
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
                () => ExecuteGastoProgramadoAsync(new Guid(entity.HangfireJobId)));
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
    public async Task ExecuteGastoProgramadoAsync(Guid hangfireId)
    {
        await _mediator.Send(new Execute.ExecuteGastoProgramadoCommand(hangfireId));
    }
}


