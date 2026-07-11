using Kash.Application.Features.Ingresos.Commands;
using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Application.Kernel.Interfaces;
using SergioIzq.Application.Kernel.Orchestration;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

/// <summary>
/// REFACTORIZADO: Handler que usa los hooks de la clase base.
/// Reducido de ~120 líneas a ~100 líneas (17% menos código).
/// Auto-crea: Categoria, Concepto, Cliente, Persona, Cuenta, FormaPago si no existen.
/// Categoría se crea PRIMERO, luego Concepto con esa CategoríaId.
/// </summary>
public sealed class CreateIngresoCommandHandler
    : AbsCreateCommandHandler<Ingreso, IngresoId, CreateIngresoCommand>
{
    private readonly ICategoriaFinderOrCreatorService _categoriaFinderService;
    private readonly IConceptoFinderOrCreatorService _conceptoFinderService;
    private readonly IClienteFinderOrCreatorService _clienteFinderService;
    private readonly IPersonaFinderOrCreatorService _personaFinderService;
    private readonly ICuentaFinderOrCreatorService _cuentaFinderService;
    private readonly IFormaPagoFinderOrCreatorService _formaPagoFinderService;
    private readonly IEntityDependencyOrchestrator _dependencyOrchestrator;

    public CreateIngresoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Ingreso, IngresoId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext,
        ICategoriaFinderOrCreatorService categoriaFinderService,
        IConceptoFinderOrCreatorService conceptoFinderService,
        IClienteFinderOrCreatorService clienteFinderService,
        IPersonaFinderOrCreatorService personaFinderService,
        ICuentaFinderOrCreatorService cuentaFinderService,
        IFormaPagoFinderOrCreatorService formaPagoFinderService,
        IEntityDependencyOrchestrator dependencyOrchestrator)
    : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _categoriaFinderService = categoriaFinderService;
        _conceptoFinderService = conceptoFinderService;
        _clienteFinderService = clienteFinderService;
        _personaFinderService = personaFinderService;
        _cuentaFinderService = cuentaFinderService;
        _formaPagoFinderService = formaPagoFinderService;
        _dependencyOrchestrator = dependencyOrchestrator;
    }

    /// <summary>
    /// HOOK: Prepara las dependencias (Categoria, Concepto, Cliente, Persona, Cuenta, FormaPago).
    /// Busca o crea las entidades relacionadas de forma asíncrona.
    /// ORDEN IMPORTANTE: Categoría PRIMERO, luego Concepto con esa CategoríaId.
    /// </summary>
    protected override Task<Result<Dictionary<string, object>>> PrepareDependenciesAsync(
        CreateIngresoCommand command,
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
                RequiredErrorMessage: "Se requiere una Categoría para crear el concepto."),

            new(
                Key: "ConceptoId",
                Id: command.ConceptoId,
                Nombre: command.ConceptoNombre,
                FindOrCreateAsync: _conceptoFinderService.FindOrCreateAsync,
                ToDependencyValue: id => ConceptoId.Create(id).Value,
                RequiredErrorMessage: "Se requiere un Concepto para crear el ingreso.",
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
                RequiredErrorMessage: "Se requiere una Cuenta para crear el ingreso."),

            new(
                Key: "FormaPagoId",
                Id: command.FormaPagoId,
                Nombre: command.FormaPagoNombre,
                FindOrCreateAsync: _formaPagoFinderService.FindOrCreateAsync,
                ToDependencyValue: id => FormaPagoId.Create(id).Value,
                RequiredErrorMessage: "Se requiere una Forma de Pago para crear el ingreso."),
        };

        return _dependencyOrchestrator.ResolveAsync(usuarioId.Value, steps, cancellationToken);
    }

    /// <summary>
    /// HOOK NUEVO: Indica que las dependencias deben guardarse ANTES de crear el Ingreso.
    /// Esto evita problemas de concurrencia cuando se auto-crean múltiples entidades relacionadas.
    /// </summary>
    protected override bool ShouldPersistDependenciesFirst()
    {
        return true; // ACTIVAR persistencia previa para evitar DbUpdateConcurrencyException
    }

    /// <summary>
    /// HOOK: Crea la entidad de dominio con las dependencias preparadas.
    /// </summary>
    protected override Ingreso CreateEntity(CreateIngresoCommand command, Dictionary<string, object>? dependencies = null)
    {
        // 1. Value Objects básicos
        var importeVO = Cantidad.Create(command.Importe).Value;
        var descripcionVO = new Descripcion(command.Descripcion ?? string.Empty);
        var fechaVO = FechaRegistro.Create(command.Fecha).Value;
        var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

        // 2. IDs de las dependencias preparadas (pueden haber sido creados)
        var conceptoId = (ConceptoId)dependencies!["ConceptoId"];
        var clienteId = dependencies.ContainsKey("ClienteId") ? (ClienteId?)dependencies["ClienteId"] : null;
        var personaId = dependencies.ContainsKey("PersonaId") ? (PersonaId?)dependencies["PersonaId"] : null;
        var cuentaId = (CuentaId)dependencies["CuentaId"];
        var formaPagoId = (FormaPagoId)dependencies["FormaPagoId"];

        // 3. Creación de la entidad de dominio
        return Ingreso.Create(
            importeVO,
            fechaVO,
            conceptoId,
            clienteId,
            personaId,
            cuentaId,
            formaPagoId,
            usuarioId,
            descripcionVO);
    }
}
