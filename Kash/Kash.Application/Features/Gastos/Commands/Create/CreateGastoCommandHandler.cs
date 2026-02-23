using Kash.Application.Features.Gastos.Commands;
using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Interfaces;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.Interfaces;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

/// <summary>
/// ✅ REFACTORIZADO: Handler que usa los hooks de la clase base.
/// Reducido de ~120 líneas a ~100 líneas (17% menos código).
/// 🔥 Auto-crea: Categoria, Concepto, Proveedor, Persona, Cuenta, FormaPago si no existen.
/// 🔥 Categoría se crea PRIMERO, luego Concepto con esa CategoríaId.
/// </summary>
public sealed class CreateGastoCommandHandler
    : AbsCreateCommandHandler<Gasto, GastoId, CreateGastoCommand>
{
    private readonly ICategoriaFinderOrCreatorService _categoriaFinderService;
    private readonly IConceptoFinderOrCreatorService _conceptoFinderService;
    private readonly IProveedorFinderOrCreatorService _proveedorFinderService;
    private readonly IPersonaFinderOrCreatorService _personaFinderService;
    private readonly ICuentaFinderOrCreatorService _cuentaFinderService;
    private readonly IFormaPagoFinderOrCreatorService _formaPagoFinderService;

    public CreateGastoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Gasto, GastoId> writeRepository,
        ICacheService cacheService,
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
    /// 🔥 HOOK: Prepara las dependencias (Categoria, Concepto, Proveedor, Persona, Cuenta, FormaPago).
    /// Busca o crea las entidades relacionadas de forma asíncrona.
    /// ORDEN IMPORTANTE: Categoría PRIMERO, luego Concepto con esa CategoríaId.
    /// </summary>
    protected override async Task<Result<Dictionary<string, object>>> PrepareDependenciesAsync(
        CreateGastoCommand command,
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
                return Result.Failure<Dictionary<string, object>>(Error.Validation(
                    "Se requiere una Categoría para crear el concepto."));
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
                return Result.Failure<Dictionary<string, object>>(Error.Validation(
                    "Se requiere un Concepto para crear el gasto."));
            }

            dependencies["ConceptoId"] = ConceptoId.Create(conceptoGuid.Value).Value;

            // 2. 🔥 PROVEEDOR: Buscar o crear (obligatorio)
            var proveedorGuid = await _proveedorFinderService.FindOrCreateAsync(
                command.ProveedorId,
                command.ProveedorNombre,
                usuarioId.Value,
                cancellationToken: cancellationToken);

            if (proveedorGuid.HasValue)
            {
                dependencies["ProveedorId"] = ProveedorId.Create(proveedorGuid.Value).Value;
            }

            // 3. 🔥 PERSONA: Buscar o crear (obligatorio)
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
                return Result.Failure<Dictionary<string, object>>(Error.Validation(
                    "Se requiere una Cuenta para crear el gasto."));
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
                return Result.Failure<Dictionary<string, object>>(Error.Validation(
                    "Se requiere una Forma de Pago para crear el gasto."));
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
    /// 🔥 HOOK NUEVO: Indica que las dependencias deben guardarse ANTES de crear el Gasto.
    /// Esto evita problemas de concurrencia cuando se auto-crean múltiples entidades relacionadas.
    /// </summary>
    protected override bool ShouldPersistDependenciesFirst()
    {
        return true; // ✅ ACTIVAR persistencia previa para evitar DbUpdateConcurrencyException
    }

    /// <summary>
    /// 🔥 HOOK: Crea la entidad de dominio con las dependencias preparadas.
    /// </summary>
    protected override Gasto CreateEntity(CreateGastoCommand command, Dictionary<string, object>? dependencies = null)
    {
        // 1. Value Objects básicos
        var importeVO = Cantidad.Create(command.Importe).Value;
        var descripcionVO = new Descripcion(command.Descripcion ?? string.Empty);
        var fechaVO = FechaRegistro.Create(command.Fecha).Value;
        var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

        // 2. IDs de las dependencias preparadas (pueden haber sido creados)
        var conceptoId = (ConceptoId)dependencies!["ConceptoId"];
        var proveedorId = dependencies.ContainsKey("ProveedorId") ? (ProveedorId?)dependencies["ProveedorId"] : null;
        var personaId = dependencies.ContainsKey("PersonaId") ? (PersonaId?)dependencies["PersonaId"] : null;
        var cuentaId = (CuentaId)dependencies["CuentaId"];
        var formaPagoId = (FormaPagoId)dependencies["FormaPagoId"];

        // 3. Creación de la entidad de dominio
        return Gasto.Create(
            importeVO,
            fechaVO,
            conceptoId,
            proveedorId,
            personaId,
            cuentaId,
            formaPagoId,
            usuarioId,
            descripcionVO);
    }
}

