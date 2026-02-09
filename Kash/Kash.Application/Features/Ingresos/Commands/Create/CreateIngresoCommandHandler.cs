using Kash.Application.Features.Ingresos.Commands;
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
public sealed class CreateIngresoCommandHandler
    : AbsCreateCommandHandler<Ingreso, IngresoId, CreateIngresoCommand>
{
    private readonly IConceptoFinderOrCreatorService _conceptoFinderService;
    private readonly IClienteFinderOrCreatorService _clienteFinderService;
    private readonly IPersonaFinderOrCreatorService _personaFinderService;
    private readonly IFormaPagoFinderOrCreatorService _formaPagoFinderService;

    public CreateIngresoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Ingreso, IngresoId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext,
        IConceptoFinderOrCreatorService conceptoFinderService,
        IClienteFinderOrCreatorService clienteFinderService,
        IPersonaFinderOrCreatorService personaFinderService,
        IFormaPagoFinderOrCreatorService formaPagoFinderService)
    : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _conceptoFinderService = conceptoFinderService;
        _clienteFinderService = clienteFinderService;
        _personaFinderService = personaFinderService;
        _formaPagoFinderService = formaPagoFinderService;
    }

    /// <summary>
    /// 🔥 HOOK: Prepara las dependencias (Concepto, Cliente, Persona, FormaPago).
    /// Busca o crea las entidades relacionadas de forma asíncrona.
    /// </summary>
    protected override async Task<Result<Dictionary<string, object>>> PrepareDependenciesAsync(
        CreateIngresoCommand command,
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
                    "Se requiere un Concepto para crear el ingreso."));
            }

            dependencies["ConceptoId"] = ConceptoId.Create(conceptoGuid.Value).Value;

            // 2. 🔥 CLIENTE: Buscar o crear (opcional)
            var clienteGuid = await _clienteFinderService.FindOrCreateAsync(
                command.ClienteId,
                command.ClienteNombre,
                usuarioId.Value,
                cancellationToken: cancellationToken);

            if (clienteGuid.HasValue)
            {
                dependencies["ClienteId"] = ClienteId.Create(clienteGuid.Value).Value;
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

            // 4. 🔥 FORMA DE PAGO: Buscar o crear (obligatorio)
            var formaPagoGuid = await _formaPagoFinderService.FindOrCreateAsync(
                command.FormaPagoId,
                command.FormaPagoNombre,
                usuarioId.Value,
                null,
                cancellationToken);

            if (formaPagoGuid.HasValue)
            {
                dependencies["FormaPagoId"] = FormaPagoId.Create(formaPagoGuid.Value).Value;
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
    protected override Ingreso CreateEntity(CreateIngresoCommand command, Dictionary<string, object>? dependencies = null)
    {
        // 1. Value Objects básicos
        var importeVO = Cantidad.Create(command.Importe).Value;
        var descripcionVO = new Descripcion(command.Descripcion ?? string.Empty);
        var fechaVO = FechaRegistro.Create(command.Fecha).Value;
        var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

        // 2. ID obligatorio que no cambia
        var cuentaId = CuentaId.Create(command.CuentaId).Value;

        // 3. IDs de las dependencias preparadas
        var conceptoId = (ConceptoId)dependencies!["ConceptoId"];
        var clienteId = dependencies.ContainsKey("ClienteId") ? (ClienteId?)dependencies["ClienteId"] : null;
        var personaId = dependencies.ContainsKey("PersonaId") ? (PersonaId?)dependencies["PersonaId"] : null;
        
        // 4. FormaPagoId: usar del diccionario si existe, sino del command
        var formaPagoId = dependencies.ContainsKey("FormaPagoId") 
            ? (FormaPagoId)dependencies["FormaPagoId"]
            : FormaPagoId.Create(command.FormaPagoId).Value;

        // 5. Creación de la entidad de dominio
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
