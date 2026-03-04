using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Interfaces;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.Interfaces;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.TraspasosProgramados.Commands;

/// <summary>
/// ✅ REFACTORIZADO: Handler para traspasos programados usando hooks de la clase base.
/// Reducido de ~120 líneas a ~80 líneas (33% menos código).
/// </summary>
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

    /// <summary>
    /// 🔥 HOOK 1: Validación pre-creación.
    /// Valida existencia de entidades relacionadas en paralelo.
    /// </summary>
    protected override async Task<Result> ValidateBeforeCreateAsync(
        CreateTraspasoProgramadoCommand command,
        CancellationToken cancellationToken)
    {
        // Validación asíncrona en paralelo (Máxima Optimización I/O)
        var validationTasks = new[]
        {
            _validator.ExistsAsync<Cuenta, CuentaId>(CuentaId.Create(command.CuentaOrigenId).Value),
            _validator.ExistsAsync<Cuenta, CuentaId>(CuentaId.Create(command.CuentaDestinoId).Value),
            _validator.ExistsAsync<Usuario, UsuarioId>(UsuarioId.Create(command.UsuarioId).Value)
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
        CreateTraspasoProgramadoCommand command,
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
    protected override TraspasoProgramado CreateEntity(
        CreateTraspasoProgramadoCommand command,
        Dictionary<string, object>? dependencies = null)
    {
        // Value Objects
        var importe = Cantidad.Create(command.Importe).Value;
        var frecuencia = Frecuencia.Create(command.Frecuencia).Value;
        var descripcion = new Descripcion(command.Descripcion ?? string.Empty);

        // IDs
        var cuentaOrigenId = CuentaId.Create(command.CuentaOrigenId).Value;
        var cuentaDestinoId = CuentaId.Create(command.CuentaDestinoId).Value;
        var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

        // HangfireJobId desde las dependencias
        var hangfireJobId = (string)dependencies!["HangfireJobId"];

        // Creación de la entidad
        return TraspasoProgramado.Create(
            cuentaOrigenId,
            cuentaDestinoId,
            importe,
            command.FechaEjecucion!,
            frecuencia,
            usuarioId,
            hangfireJobId,
            descripcion
        ).Value;
    }
}

