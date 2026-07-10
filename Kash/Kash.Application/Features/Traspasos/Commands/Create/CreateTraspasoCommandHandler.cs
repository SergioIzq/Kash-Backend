using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Traspasos.Commands;

/// <summary>
/// REFACTORIZADO: Handler simplificado usando hooks de la clase base.
/// Reducido de ~100 líneas a ~50 líneas (50% menos código).
/// </summary>
public sealed class CreateTraspasoCommandHandler : AbsCreateCommandHandler<Traspaso, TraspasoId, CreateTraspasoCommand>
{
    private readonly IDomainValidator _validator;

    public CreateTraspasoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Traspaso, TraspasoId> writeRepository,
        ICacheService cacheService,
        IDomainValidator validator,
        IUserContext userContext)
    : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _validator = validator;
    }

    /// <summary>
    /// HOOK 1: Validación pre-creación.
    /// Valida existencia de cuentas en paralelo + regla de negocio (origen != destino).
    /// </summary>
    protected override async Task<Result> ValidateBeforeCreateAsync(
        CreateTraspasoCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Validación de negocio: cuentas diferentes
        if (command.CuentaOrigenId == command.CuentaDestinoId)
        {
            return Result.Failure(
                Error.Validation("La cuenta origen y destino no pueden ser la misma."));
        }

        // 2. Validación de existencia en paralelo
        var validationTasks = new[]
        {
            _validator.ExistsAsync<Cuenta, CuentaId>(CuentaId.Create(command.CuentaOrigenId).Value),
            _validator.ExistsAsync<Cuenta, CuentaId>(CuentaId.Create(command.CuentaDestinoId).Value)
        };

        var results = await Task.WhenAll(validationTasks);

        if (!results[0] || !results[1])
        {
            return Result.Failure(
                Error.NotFound("Cuenta origen o destino no encontrada."));
        }

        return Result.Success();
    }

    /// <summary>
    /// HOOK 2: Crea la entidad de dominio.
    /// Solo necesita implementar la lógica de creación, el resto lo maneja la clase base.
    /// </summary>
    protected override Traspaso CreateEntity(
        CreateTraspasoCommand command,
        Dictionary<string, object>? dependencies = null)
    {
        // Value Objects
        var importeVO = Cantidad.Create(command.Importe).Value;
        var fechaVO = FechaRegistro.Create(command.Fecha).Value;
        var descripcionVO = new Descripcion(command.Descripcion ?? string.Empty);
        var usuarioIdVO = UsuarioId.Create(command.UsuarioId).Value;

        // IDs de identidad
        var cuentaOrigenId = CuentaId.Create(command.CuentaOrigenId).Value;
        var cuentaDestinoId = CuentaId.Create(command.CuentaDestinoId).Value;

        // Creación de la entidad
        return Traspaso.Create(
            cuentaOrigenId,
            cuentaDestinoId,
            importeVO,
            fechaVO,
            usuarioIdVO,
            descripcionVO);
    }
}

