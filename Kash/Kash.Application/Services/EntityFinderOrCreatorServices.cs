using Kash.Domain;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Application.Interfaces;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Services;

/// <summary>
/// Servicio para buscar o crear Conceptos.
/// </summary>
public class ConceptoFinderOrCreatorService : IConceptoFinderOrCreatorService
{
    private readonly IReadRepository<Concepto, ConceptoDto, ConceptoId> _readRepository;
    private readonly IWriteRepository<Concepto, ConceptoId> _writeRepository;

    public ConceptoFinderOrCreatorService(
        IReadRepository<Concepto, ConceptoDto, ConceptoId> readRepository,
        IWriteRepository<Concepto, ConceptoId> writeRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }

    public async Task<Guid?> FindOrCreateAsync(
        Guid? id,
        string? nombre,
        Guid usuarioId,
        Dictionary<string, object>? additionalData = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Validar que se proporcionó CategoriaId (obligatorio para Concepto)
        if (additionalData == null || !additionalData.TryGetValue("CategoriaId", out var categoriaIdObj))
        {
            throw new ArgumentException("Se requiere CategoriaId para crear un Concepto.");
        }

        var categoriaId = CategoriaId.Create((Guid)categoriaIdObj).Value;

        // 2. Buscar por ID si se proporcionó
        if (id.HasValue)
        {
            var existing = await _readRepository.GetReadModelByIdAsync(id.Value, cancellationToken);
            if (existing != null)
            {
                return existing.Id;
            }
        }

        // 3. Buscar por nombre (case-insensitive) si se proporcionó
        if (!string.IsNullOrWhiteSpace(nombre))
        {
            var conceptos = await _readRepository.SearchForAutocompleteAsync(
                usuarioId,
                nombre,
                limit: 100,
                cancellationToken: cancellationToken);

            var matchExacto = conceptos.FirstOrDefault(c =>
                c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));

            if (matchExacto != null)
            {
                return matchExacto.Id;
            }

            // 4. Crear nuevo concepto
            var nombreVO = Nombre.Create(nombre).Value;
            var usuarioIdVO = UsuarioId.Create(usuarioId).Value;
            var nuevoConcepto = Concepto.Create(nombreVO, categoriaId, usuarioIdVO);
            _writeRepository.Add(nuevoConcepto);

            return nuevoConcepto.Id.Value;
        }

        // Si llegamos aquí, no se proporcionó ni ID ni nombre
        throw new ArgumentException("Se requiere ID o Nombre para buscar o crear un Concepto.");
    }
}

/// <summary>
/// Servicio para buscar o crear Formas de Pago.
/// </summary>
public class FormaPagoFinderOrCreatorService : IFormaPagoFinderOrCreatorService
{
    private readonly IReadRepository<FormaPago, FormaPagoDto, FormaPagoId> _readRepository;
    private readonly IFormaPagoWriteRepository _writeRepository;

    public FormaPagoFinderOrCreatorService(
        IReadRepository<FormaPago, FormaPagoDto, FormaPagoId> readRepository,
        IFormaPagoWriteRepository writeRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }

    public async Task<Guid?> FindOrCreateAsync(
        Guid? id,
        string? nombre,
        Guid usuarioId,
        Dictionary<string, object>? additionalData = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Buscar por ID si se proporcionó
        if (id.HasValue)
        {
            var existing = await _readRepository.GetReadModelByIdAsync(id.Value, cancellationToken);
            if (existing != null)
            {
                return existing.Id;
            }
        }

        // 2. Buscar por nombre (case-insensitive) si se proporcionó
        if (!string.IsNullOrWhiteSpace(nombre))
        {
            // Crear la entidad temporal para buscar o crear
            var nombreVO = Nombre.Create(nombre).Value;
            var usuarioIdVO = UsuarioId.Create(usuarioId).Value;
            var nuevaFormaPago = FormaPago.Create(nombreVO, usuarioIdVO);

            // Usar el método FindOrCreateAsync que reutiliza si existe
            var result = await _writeRepository.FindOrCreateAsync(nuevaFormaPago, cancellationToken);

            if (result.IsSuccess)
            {
                return result.Value.Id.Value;
            }
        }

        // Si llegamos aquí, no se proporcionó ni ID ni nombre
        return null;
    }
}

/// <summary>
/// Servicio para buscar o crear Clientes.
/// </summary>
public class ClienteFinderOrCreatorService : IClienteFinderOrCreatorService
{
    private readonly IReadRepository<Cliente, ClienteDto, ClienteId> _readRepository;
    private readonly IWriteRepository<Cliente, ClienteId> _writeRepository;

    public ClienteFinderOrCreatorService(
        IReadRepository<Cliente, ClienteDto, ClienteId> readRepository,
        IWriteRepository<Cliente, ClienteId> writeRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }

    public async Task<Guid?> FindOrCreateAsync(
        Guid? id,
        string? nombre,
        Guid usuarioId,
        Dictionary<string, object>? additionalData = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Buscar por ID si se proporcionó
        if (id.HasValue)
        {
            var existing = await _readRepository.GetReadModelByIdAsync(id.Value, cancellationToken);
            if (existing != null)
            {
                return existing.Id;
            }
        }

        // 2. Buscar por nombre (case-insensitive) si se proporcionó
        if (!string.IsNullOrWhiteSpace(nombre))
        {
            var clientes = await _readRepository.SearchForAutocompleteAsync(
                usuarioId,
                nombre,
                limit: 100,
                cancellationToken: cancellationToken);

            var matchExacto = clientes.FirstOrDefault(c =>
                c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));

            if (matchExacto != null)
            {
                return matchExacto.Id;
            }

            // 3. Crear nuevo cliente
            var nombreVO = Nombre.Create(nombre).Value;
            var usuarioIdVO = UsuarioId.Create(usuarioId).Value;
            var nuevoCliente = Cliente.Create(nombreVO, usuarioIdVO);
            _writeRepository.Add(nuevoCliente);

            return nuevoCliente.Id.Value;
        }

        // Cliente es opcional, retornar null si no se proporcionó nada
        return null;
    }
}

/// <summary>
/// Servicio para buscar o crear Proveedores.
/// </summary>
public class ProveedorFinderOrCreatorService : IProveedorFinderOrCreatorService
{
    private readonly IReadRepository<Proveedor, ProveedorDto, ProveedorId> _readRepository;
    private readonly IWriteRepository<Proveedor, ProveedorId> _writeRepository;

    public ProveedorFinderOrCreatorService(
        IReadRepository<Proveedor, ProveedorDto, ProveedorId> readRepository,
        IWriteRepository<Proveedor, ProveedorId> writeRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }

    public async Task<Guid?> FindOrCreateAsync(
        Guid? id,
        string? nombre,
        Guid usuarioId,
        Dictionary<string, object>? additionalData = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Buscar por ID si se proporcionó
        if (id.HasValue)
        {
            var existing = await _readRepository.GetReadModelByIdAsync(id.Value, cancellationToken);
            if (existing != null)
            {
                return existing.Id;
            }
        }

        // 2. Buscar por nombre (case-insensitive) si se proporcionó
        if (!string.IsNullOrWhiteSpace(nombre))
        {
            var clientes = await _readRepository.SearchForAutocompleteAsync(
                usuarioId,
                nombre,
                limit: 100,
                cancellationToken: cancellationToken);

            var matchExacto = clientes.FirstOrDefault(c =>
                c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));

            if (matchExacto != null)
            {
                return matchExacto.Id;
            }

            // 3. Crear nuevo proveedor
            var nombreVO = Nombre.Create(nombre).Value;
            var usuarioIdVO = UsuarioId.Create(usuarioId).Value;
            var nuevoProveedor = Proveedor.Create(nombreVO, usuarioIdVO);
            _writeRepository.Add(nuevoProveedor);

            return nuevoProveedor.Id.Value;
        }

        // Cliente es opcional, retornar null si no se proporcionó nada
        return null;
    }
}

/// <summary>
/// Servicio para buscar o crear Personas.
/// </summary>
public class PersonaFinderOrCreatorService : IPersonaFinderOrCreatorService
{
    private readonly IReadRepository<Persona, PersonaDto, PersonaId> _readRepository;
    private readonly IWriteRepository<Persona, PersonaId> _writeRepository;

    public PersonaFinderOrCreatorService(
        IReadRepository<Persona, PersonaDto, PersonaId> readRepository,
        IWriteRepository<Persona, PersonaId> writeRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }

    public async Task<Guid?> FindOrCreateAsync(
        Guid? id,
        string? nombre,
        Guid usuarioId,
        Dictionary<string, object>? additionalData = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Buscar por ID si se proporcionó
        if (id.HasValue)
        {
            var existing = await _readRepository.GetReadModelByIdAsync(id.Value, cancellationToken);
            if (existing != null)
            {
                return existing.Id;
            }
        }

        // 2. Buscar por nombre (case-insensitive) si se proporcionó
        if (!string.IsNullOrWhiteSpace(nombre))
        {
            var personas = await _readRepository.SearchForAutocompleteAsync(
                usuarioId,
                nombre,
                limit: 100,
                cancellationToken: cancellationToken);

            var matchExacto = personas.FirstOrDefault(p =>
                p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));

            if (matchExacto != null)
            {
                return matchExacto.Id;
            }

            // 3. Crear nueva persona
            var nombreVO = Nombre.Create(nombre).Value;
            var usuarioIdVO = UsuarioId.Create(usuarioId).Value;
            var nuevaPersona = Persona.Create(nombreVO, usuarioIdVO);
            _writeRepository.Add(nuevaPersona);

            return nuevaPersona.Id.Value;
        }

        // Persona es opcional, retornar null si no se proporcionó nada
        return null;
    }
}
