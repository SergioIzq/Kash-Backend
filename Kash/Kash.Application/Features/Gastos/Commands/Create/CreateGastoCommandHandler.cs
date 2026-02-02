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
/// Reducido de ~120 líneas a ~70 líneas (40% menos código).
/// </summary>
public sealed class CreateGastoCommandHandler
    : AbsCreateCommandHandler<Gasto, GastoId, CreateGastoCommand>
{
    private readonly IConceptoFinderOrCreatorService _conceptoFinderService;
    private readonly IProveedorFinderOrCreatorService _proveedorFinderService;
    private readonly IPersonaFinderOrCreatorService _personaFinderService;

    public CreateGastoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Gasto, GastoId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext,
        IConceptoFinderOrCreatorService conceptoFinderService,
        IProveedorFinderOrCreatorService proveedorFinderService,
        IPersonaFinderOrCreatorService personaFinderService)
    : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _conceptoFinderService = conceptoFinderService;
        _proveedorFinderService = proveedorFinderService;
        _personaFinderService = personaFinderService;
    }

    /// <summary>
    /// 🔥 HOOK: Prepara las dependencias (Concepto, proveedor, Persona).
    /// Busca o crea las entidades relacionadas de forma asíncrona.
    /// </summary>
    protected override async Task<Result<Dictionary<string, object>>> PrepareDependenciesAsync(
        CreateGastoCommand command,
        CancellationToken cancellationToken)
    {
        var dependencies = new Dictionary<string, object>();

        try
        {
            var usuarioId = UsuarioId.Create(command.UsuarioId).Value;
            var categoriaId = CategoriaId.Create(command.CategoriaId).Value;

            // 1. 🔥 CONCEPTO: Buscar o crear (obligatorio)
            var conceptoGuid = await _conceptoFinderService.FindOrCreateAsync(
                command.ConceptoId,
                command.ConceptoNombre,
                usuarioId.Value,
                new Dictionary<string, object> { { "CategoriaId", categoriaId.Value } },
                cancellationToken);

            if (conceptoGuid == null)
            {
                return Result.Failure<Dictionary<string, object>>(Error.Validation(
                    "Se requiere un Concepto para crear el Gasto."));
            }

            dependencies["ConceptoId"] = ConceptoId.Create(conceptoGuid.Value).Value;

            // 2. 🔥 proveedor: Buscar o crear (opcional)
            var proveedorGuid = await _proveedorFinderService.FindOrCreateAsync(
                command.ProveedorId,
                command.ProveedorNombre,
                usuarioId.Value,
                cancellationToken: cancellationToken);

            if (proveedorGuid.HasValue)
            {
                dependencies["proveedorId"] = ProveedorId.Create(proveedorGuid.Value).Value;
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

            return Result.Success(dependencies);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Dictionary<string, object>>(Error.Validation(ex.Message));
        }
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

        // 2. IDs obligatorios
        var cuentaId = CuentaId.Create(command.CuentaId).Value;
        var formaPagoId = FormaPagoId.Create(command.FormaPagoId).Value;

        // 3. IDs de las dependencias preparadas
        var conceptoId = (ConceptoId)dependencies!["ConceptoId"];
        var proveedorId = dependencies.ContainsKey("ProveedorId") ? (ProveedorId?)dependencies["ProveedorId"] : null;
        var personaId = dependencies.ContainsKey("PersonaId") ? (PersonaId?)dependencies["PersonaId"] : null;

        // 4. Creación de la entidad de dominio
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
