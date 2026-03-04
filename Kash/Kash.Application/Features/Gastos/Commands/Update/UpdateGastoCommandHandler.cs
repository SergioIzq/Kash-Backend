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

namespace Kash.Application.Features.Gastos.Commands;

/// <summary>
/// ✅ REFACTORIZADO: Handler con auto-creación de entidades relacionadas.
/// Reducido de ~120 líneas a ~110 líneas.
/// 🔥 Si Categoria/Proveedor/Persona/Concepto/Cuenta/FormaPago no existen, los crea automáticamente.
/// 🔥 Categoría se crea PRIMERO, luego Concepto con esa CategoríaId.
/// </summary>
public sealed class UpdateGastoCommandHandler
    : AbsUpdateCommandHandler<Gasto, GastoId, GastoDto, UpdateGastoCommand>
{
    private readonly ICategoriaFinderOrCreatorService _categoriaFinderService;
    private readonly IConceptoFinderOrCreatorService _conceptoFinderService;
    private readonly IProveedorFinderOrCreatorService _proveedorFinderService;
    private readonly IPersonaFinderOrCreatorService _personaFinderService;
    private readonly ICuentaFinderOrCreatorService _cuentaFinderService;
    private readonly IFormaPagoFinderOrCreatorService _formaPagoFinderService;

    public UpdateGastoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Gasto, GastoId> writeRepository,
        ICacheService cacheService,
        IReadRepository<Gasto, GastoDto, GastoId> readOnlyRepository,
        IUserContext userContext,
        ICategoriaFinderOrCreatorService categoriaFinderService,
        IConceptoFinderOrCreatorService conceptoFinderService,
        IProveedorFinderOrCreatorService proveedorFinderService,
        IPersonaFinderOrCreatorService personaFinderService,
        ICuentaFinderOrCreatorService cuentaFinderService,
        IFormaPagoFinderOrCreatorService formaPagoFinderService)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _categoriaFinderService = categoriaFinderService;
        _conceptoFinderService = conceptoFinderService;
        _proveedorFinderService = proveedorFinderService;
        _personaFinderService = personaFinderService;
        _cuentaFinderService = cuentaFinderService;
        _formaPagoFinderService = formaPagoFinderService;
    }

    /// <summary>
    /// 🔥 HOOK 1: Buscar o crear entidades relacionadas (Categoria, Concepto, Proveedor, Persona, Cuenta, FormaPago).
    /// Ya no necesitamos ValidateBeforeUpdateAsync porque todas las entidades se auto-crean.
    /// ORDEN IMPORTANTE: Categoría PRIMERO, luego Concepto con esa CategoríaId.
    /// </summary>
    protected override async Task<Result<Dictionary<string, object>>> PrepareDependenciesAsync(
        UpdateGastoCommand command,
        CancellationToken cancellationToken)
    {
        var dependencies = new Dictionary<string, object>();

        try
        {
            var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

            // 0. 🔥 CATEGORÍA: Buscar o crear PRIMERO (obligatoria para Concepto)
            var categoriaGuid = await _categoriaFinderService.FindOrCreateAsync(
                command.CategoriaId,
                command.CategoriaNombre,
                usuarioId.Value,
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
                usuarioId.Value,
                new Dictionary<string, object> { { "CategoriaId", categoriaId.Value } },
                cancellationToken);

            if (conceptoGuid == null)
            {
                return Result.Failure<Dictionary<string, object>>(
                    Error.Validation("Se requiere un Concepto para actualizar el ingreso."));
            }

            dependencies["ConceptoId"] = ConceptoId.Create(conceptoGuid.Value).Value;

            // 2. 🔥 CLIENTE: Buscar o crear (opcional)
            var proveedorGuid = await _proveedorFinderService.FindOrCreateAsync(
                command.ProveedorId,
                command.ProveedorNombre,
                usuarioId.Value,
                cancellationToken: cancellationToken);

            if (proveedorGuid.HasValue)
            {
                dependencies["ProveedorId"] = ProveedorId.Create(proveedorGuid.Value).Value;
            }

            // 3. 🔥 PERSONA: Buscar o crear (opcional)
            var personaGuid = await _personaFinderService.FindOrCreateAsync(
                command.PersonaId,
                command.PersonaNombre,
                usuarioId.Value,
                cancellationToken: cancellationToken);

            if (personaGuid.HasValue)
            {
                dependencies["PersonaId"] = PersonaId.Create(personaGuid.Value).Value;
            }

            // 4. 🔥 CUENTA: Buscar o crear (obligatorio)
            var cuentaGuid = await _cuentaFinderService.FindOrCreateAsync(
                command.CuentaId,
                command.CuentaNombre,
                usuarioId.Value,
                cancellationToken: cancellationToken);

            if (cuentaGuid == null)
            {
                return Result.Failure<Dictionary<string, object>>(
                    Error.Validation("Se requiere una Cuenta para actualizar el ingreso."));
            }

            dependencies["CuentaId"] = CuentaId.Create(cuentaGuid.Value).Value;

            // 5. 🔥 FORMA DE PAGO: Buscar o crear (obligatorio)
            var formaPagoGuid = await _formaPagoFinderService.FindOrCreateAsync(
                command.FormaPagoId,
                command.FormaPagoNombre,
                usuarioId.Value,
                cancellationToken: cancellationToken);

            if (formaPagoGuid == null)
            {
                return Result.Failure<Dictionary<string, object>>(
                    Error.Validation("Se requiere una Forma de Pago para actualizar el ingreso."));
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
    /// 🔥 HOOK NUEVO: Indica que las dependencias deben guardarse ANTES de actualizar el Gasto.
    /// Esto evita problemas de concurrencia cuando se auto-crean múltiples entidades relacionadas.
    /// </summary>
    protected override bool ShouldPersistDependenciesFirst()
    {
        return true; // ✅ ACTIVAR persistencia previa para evitar DbUpdateConcurrencyException
    }

    /// <summary>
    /// 🔥 HOOK 2: Aplica los cambios del comando a la entidad.
    /// Usa las dependencias preparadas (que pueden haber sido creadas).
    /// </summary>
    protected override void ApplyChanges(
        Gasto entity,
        UpdateGastoCommand command,
        Dictionary<string, object>? dependencies = null)
    {
        // Value Objects obligatorios
        var importeVO = Cantidad.Create(command.Importe).Value;
        var fechaVO = FechaRegistro.Create(command.Fecha).Value;
        var usuarioIdVO = UsuarioId.Create(command.UsuarioId).Value;
        var descripcionVO = new Descripcion(command.Descripcion ?? string.Empty);

        // IDs desde las dependencias (pueden haber sido creados automáticamente)
        var conceptoId = (ConceptoId)dependencies!["ConceptoId"];
        var proveedorId = dependencies.ContainsKey("ProveedorId")
            ? (ProveedorId?)dependencies["ProveedorId"]
            : null;
        var personaId = dependencies.ContainsKey("PersonaId")
            ? (PersonaId?)dependencies["PersonaId"]
            : null;
        var cuentaId = (CuentaId)dependencies["CuentaId"];
        var formaPagoId = (FormaPagoId)dependencies["FormaPagoId"];

        // Aplicar cambios a la entidad
        entity.Update(
            importeVO,
            fechaVO,
            conceptoId,
            proveedorId,
            personaId,
            cuentaId,
            formaPagoId,
            usuarioIdVO,
            descripcionVO
        );
    }
}