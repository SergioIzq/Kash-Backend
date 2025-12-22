using Kash.Application.Features.Ingresos.Commands;
using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos; // 🔥 AGREGADO
using Kash.Shared.Application.Interfaces;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.Interfaces;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

public sealed class CreateIngresoCommandHandler
    : AbsCreateCommandHandler<Ingreso, IngresoId, CreateIngresoCommand>
{
    private readonly IWriteRepository<Cliente, ClienteId> _clienteWriteRepository;
    private readonly IWriteRepository<Persona, PersonaId> _personaWriteRepository;
    private readonly IWriteRepository<Concepto, ConceptoId> _conceptoWriteRepository;
    private readonly IReadRepositoryWithDto<Cliente, ClienteDto, ClienteId> _clienteReadRepository; // 🔥 CORREGIDO
    private readonly IReadRepositoryWithDto<Persona, PersonaDto, PersonaId> _personaReadRepository; // 🔥 CORREGIDO
    private readonly IReadRepositoryWithDto<Concepto, ConceptoDto, ConceptoId> _conceptoReadRepository; // 🔥 CORREGIDO

    public CreateIngresoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Ingreso, IngresoId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext,
        IWriteRepository<Cliente, ClienteId> clienteWriteRepository,
        IWriteRepository<Persona, PersonaId> personaWriteRepository,
        IWriteRepository<Concepto, ConceptoId> conceptoWriteRepository,
        IReadRepositoryWithDto<Cliente, ClienteDto, ClienteId> clienteReadRepository, // 🔥 CORREGIDO
        IReadRepositoryWithDto<Persona, PersonaDto, PersonaId> personaReadRepository, // 🔥 CORREGIDO
        IReadRepositoryWithDto<Concepto, ConceptoDto, ConceptoId> conceptoReadRepository) // 🔥 CORREGIDO
    : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _clienteWriteRepository = clienteWriteRepository;
        _personaWriteRepository = personaWriteRepository;
        _conceptoWriteRepository = conceptoWriteRepository;
        _clienteReadRepository = clienteReadRepository;
        _personaReadRepository = personaReadRepository;
        _conceptoReadRepository = conceptoReadRepository;
    }

    public override async Task<Result<Guid>> Handle(
        CreateIngresoCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // 1. 🚀 CONSTRUCCIÓN DE VALUE OBJECTS BÁSICOS
            var importeVO = Cantidad.Create(command.Importe).Value;
            var descripcionVO = new Descripcion(command.Descripcion ?? string.Empty);
            var fechaVO = FechaRegistro.Create(command.Fecha).Value;
            var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

            // 2. 🔥 IDS OBLIGATORIOS (sin auto-creación por ahora)
            var categoriaId = CategoriaId.Create(command.CategoriaId).Value;
            var cuentaId = CuentaId.Create(command.CuentaId).Value;
            var formaPagoId = FormaPagoId.Create(command.FormaPagoId).Value;

            // 3. 🔥 CONCEPTO: Buscar o crear
            var conceptoId = await FindOrCreateConceptoAsync(
                command.ConceptoId, 
                command.ConceptoNombre, 
                categoriaId, 
                usuarioId, 
                cancellationToken);

            // 4. 🔥 CLIENTE: Buscar o crear (opcional)
            ClienteId? clienteId = null;
            if (command.ClienteId.HasValue || !string.IsNullOrWhiteSpace(command.ClienteNombre))
            {
                clienteId = await FindOrCreateClienteAsync(
                    command.ClienteId, 
                    command.ClienteNombre, 
                    usuarioId, 
                    cancellationToken);
            }

            // 5. 🔥 PERSONA: Buscar o crear (opcional)
            PersonaId? personaId = null;
            if (command.PersonaId.HasValue || !string.IsNullOrWhiteSpace(command.PersonaNombre))
            {
                personaId = await FindOrCreatePersonaAsync(
                    command.PersonaId, 
                    command.PersonaNombre, 
                    usuarioId, 
                    cancellationToken);
            }

            // 6. 🎯 CREACIÓN DE LA ENTIDAD DE DOMINIO
            var ingreso = Ingreso.Create(
                importeVO,
                fechaVO,
                conceptoId,
                clienteId,
                personaId,
                cuentaId,
                formaPagoId,
                usuarioId,
                descripcionVO);

            // 7. 💾 PERSISTENCIA
            var entityResult = await base.CreateAsync(ingreso, cancellationToken);

            if (entityResult.IsFailure)
            {
                return Result.Failure<Guid>(entityResult.Error);
            }

            // 8. ✅ ÉXITO
            return Result.Success(entityResult.Value);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Guid>(Error.Validation(ex.Message));
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(Error.Failure(
                "Error.Unexpected", 
                "Error Inesperado", 
                ex.Message));
        }
    }

    /// <summary>
    /// 🔥 Busca un concepto por ID o nombre, si no existe lo crea.
    /// La búsqueda es case-insensitive.
    /// </summary>
    private async Task<ConceptoId> FindOrCreateConceptoAsync(
        Guid conceptoId,
        string? nombre,
        CategoriaId categoriaId,
        UsuarioId usuarioId,
        CancellationToken cancellationToken)
    {
        // 1. Intentar buscar por ID
        var existingConcepto = await _conceptoReadRepository.GetReadModelByIdAsync(conceptoId, cancellationToken);
        if (existingConcepto != null)
        {
            return ConceptoId.Create(existingConcepto.Id).Value;
        }

        // 2. Si no existe y se proporcionó nombre, buscar por nombre (case-insensitive)
        if (!string.IsNullOrWhiteSpace(nombre))
        {
            var conceptosUsuario = await _conceptoReadRepository.SearchForAutocompleteAsync(
                usuarioId.Value,
                nombre,
                limit: 100, // Buscamos todos para filtrar case-insensitive
                cancellationToken: cancellationToken);

            // Buscar match exacto case-insensitive
            var matchExacto = conceptosUsuario.FirstOrDefault(c => 
                c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));

            if (matchExacto != null)
            {
                return ConceptoId.Create(matchExacto.Id).Value;
            }

            // 3. Si no existe, crear uno nuevo
            var nombreVO = Nombre.Create(nombre).Value;
            var nuevoConcepto = Concepto.Create(nombreVO, categoriaId, usuarioId);
            _conceptoWriteRepository.Add(nuevoConcepto);

            return nuevoConcepto.Id;
        }

        // Si no se proporcionó nombre y no existe, error
        throw new ArgumentException($"El concepto con ID '{conceptoId}' no existe y no se proporcionó nombre para crearlo.");
    }

    /// <summary>
    /// 🔥 Busca un cliente por ID o nombre, si no existe lo crea.
    /// La búsqueda es case-insensitive.
    /// </summary>
    private async Task<ClienteId?> FindOrCreateClienteAsync(
        Guid? clienteId,
        string? nombre,
        UsuarioId usuarioId,
        CancellationToken cancellationToken)
    {
        // 1. Intentar buscar por ID si se proporcionó
        if (clienteId.HasValue)
        {
            var existingCliente = await _clienteReadRepository.GetReadModelByIdAsync(clienteId.Value, cancellationToken);
            if (existingCliente != null)
            {
                return ClienteId.Create(existingCliente.Id).Value;
            }
        }

        // 2. Si no existe y se proporcionó nombre, buscar por nombre (case-insensitive)
        if (!string.IsNullOrWhiteSpace(nombre))
        {
            var clientesUsuario = await _clienteReadRepository.SearchForAutocompleteAsync(
                usuarioId.Value,
                nombre,
                limit: 100,
                cancellationToken: cancellationToken);

            var matchExacto = clientesUsuario.FirstOrDefault(c => 
                c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));

            if (matchExacto != null)
            {
                return ClienteId.Create(matchExacto.Id).Value;
            }

            // 3. Crear nuevo cliente
            var nombreVO = Nombre.Create(nombre).Value;
            var nuevoCliente = Cliente.Create(nombreVO, usuarioId);
            _clienteWriteRepository.Add(nuevoCliente);

            return nuevoCliente.Id;
        }

        // Si no se proporcionó nada, retornar null (cliente es opcional)
        return null;
    }

    /// <summary>
    /// 🔥 Busca una persona por ID o nombre, si no existe la crea.
    /// La búsqueda es case-insensitive.
    /// </summary>
    private async Task<PersonaId?> FindOrCreatePersonaAsync(
        Guid? personaId,
        string? nombre,
        UsuarioId usuarioId,
        CancellationToken cancellationToken)
    {
        // 1. Intentar buscar por ID si se proporcionó
        if (personaId.HasValue)
        {
            var existingPersona = await _personaReadRepository.GetReadModelByIdAsync(personaId.Value, cancellationToken);
            if (existingPersona != null)
            {
                return PersonaId.Create(existingPersona.Id).Value;
            }
        }

        // 2. Si no existe y se proporcionó nombre, buscar por nombre (case-insensitive)
        if (!string.IsNullOrWhiteSpace(nombre))
        {
            var personasUsuario = await _personaReadRepository.SearchForAutocompleteAsync(
                usuarioId.Value,
                nombre,
                limit: 100,
                cancellationToken: cancellationToken);

            var matchExacto = personasUsuario.FirstOrDefault(p => 
                p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));

            if (matchExacto != null)
            {
                return PersonaId.Create(matchExacto.Id).Value;
            }

            // 3. Crear nueva persona
            var nombreVO = Nombre.Create(nombre).Value;
            var nuevaPersona = Persona.Create(Guid.NewGuid(), nombreVO, usuarioId);
            _personaWriteRepository.Add(nuevaPersona);

            return nuevaPersona.Id;
        }

        // Si no se proporcionó nada, retornar null (persona es opcional)
        return null;
    }

    protected override Ingreso CreateEntity(CreateIngresoCommand command)
    {
        throw new NotImplementedException(
            "CreateEntity no debe usarse. La lógica de creación asíncrona reside en el método Handle.");
    }
}
