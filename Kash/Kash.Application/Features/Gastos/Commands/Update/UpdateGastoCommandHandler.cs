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
/// REFACTORIZADO: Handler con auto-creación de entidades relacionadas.
/// Reducido de ~120 líneas a ~110 líneas.
/// Si Categoria/Proveedor/Persona/Concepto/Cuenta/FormaPago no existen, los crea automáticamente.
/// Categoría se crea PRIMERO, luego Concepto con esa CategoríaId.
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
    private readonly IEntityDependencyOrchestrator _dependencyOrchestrator;

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
        IFormaPagoFinderOrCreatorService formaPagoFinderService,
        IEntityDependencyOrchestrator dependencyOrchestrator)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _categoriaFinderService = categoriaFinderService;
        _conceptoFinderService = conceptoFinderService;
        _proveedorFinderService = proveedorFinderService;
        _personaFinderService = personaFinderService;
        _cuentaFinderService = cuentaFinderService;
        _formaPagoFinderService = formaPagoFinderService;
        _dependencyOrchestrator = dependencyOrchestrator;
    }

    /// <summary>
    /// HOOK 1: Buscar o crear entidades relacionadas (Categoria, Concepto, Proveedor, Persona, Cuenta, FormaPago).
    /// Ya no necesitamos ValidateBeforeUpdateAsync porque todas las entidades se auto-crean.
    /// ORDEN IMPORTANTE: Categoría PRIMERO, luego Concepto con esa CategoríaId.
    /// </summary>
    protected override Task<Result<Dictionary<string, object>>> PrepareDependenciesAsync(
        UpdateGastoCommand command,
        CancellationToken cancellationToken)
    {
        var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

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
                RequiredErrorMessage: "Se requiere un Concepto para actualizar el ingreso.",
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
                RequiredErrorMessage: "Se requiere una Cuenta para actualizar el ingreso."),

            new(
                Key: "FormaPagoId",
                Id: command.FormaPagoId,
                Nombre: command.FormaPagoNombre,
                FindOrCreateAsync: _formaPagoFinderService.FindOrCreateAsync,
                ToDependencyValue: id => FormaPagoId.Create(id).Value,
                RequiredErrorMessage: "Se requiere una Forma de Pago para actualizar el ingreso."),
        };

        return _dependencyOrchestrator.ResolveAsync(usuarioId.Value, steps, cancellationToken);
    }

    /// <summary>
    /// HOOK NUEVO: Indica que las dependencias deben guardarse ANTES de actualizar el Gasto.
    /// Esto evita problemas de concurrencia cuando se auto-crean múltiples entidades relacionadas.
    /// </summary>
    protected override bool ShouldPersistDependenciesFirst()
    {
        return true; // ACTIVAR persistencia previa para evitar DbUpdateConcurrencyException
    }

    /// <summary>
    /// HOOK 2: Aplica los cambios del comando a la entidad.
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
