namespace Kash.Shared.Application.Interfaces;

/// <summary>
/// Servicio base para buscar o crear entidades Concepto.
/// </summary>
public interface IConceptoFinderOrCreatorService : IApplicationService
{
    Task<Guid?> FindOrCreateAsync(
        Guid? id,
        string? nombre,
        Guid usuarioId,
        Dictionary<string, object>? additionalData = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio base para buscar o crear entidades Categoria.
/// </summary>
public interface ICategoriaFinderOrCreatorService : IApplicationService
{
    Task<Guid?> FindOrCreateAsync(
        Guid? id,
        string? nombre,
        Guid usuarioId,
        Dictionary<string, object>? additionalData = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio base para buscar o crear entidades Cliente.
/// </summary>
public interface IClienteFinderOrCreatorService : IApplicationService
{
    Task<Guid?> FindOrCreateAsync(
        Guid? id,
        string? nombre,
        Guid usuarioId,
        Dictionary<string, object>? additionalData = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio base para buscar o crear entidades Proveedor.
/// </summary>
public interface IProveedorFinderOrCreatorService : IApplicationService
{
    Task<Guid?> FindOrCreateAsync(
        Guid? id,
        string? nombre,
        Guid usuarioId,
        Dictionary<string, object>? additionalData = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio base para buscar o crear entidades Persona.
/// </summary>
public interface IPersonaFinderOrCreatorService : IApplicationService
{
    Task<Guid?> FindOrCreateAsync(
        Guid? id,
        string? nombre,
        Guid usuarioId,
        Dictionary<string, object>? additionalData = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio base para buscar o crear entidades Cuenta.
/// </summary>
public interface ICuentaFinderOrCreatorService : IApplicationService
{
    Task<Guid?> FindOrCreateAsync(
        Guid? id,
        string? nombre,
        Guid usuarioId,
        Dictionary<string, object>? additionalData = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio base para buscar o crear entidades FormaPago.
/// </summary>
public interface IFormaPagoFinderOrCreatorService : IApplicationService
{
    Task<Guid?> FindOrCreateAsync(
        Guid? id,
        string? nombre,
        Guid usuarioId,
        Dictionary<string, object>? additionalData = null,
        CancellationToken cancellationToken = default);
}
