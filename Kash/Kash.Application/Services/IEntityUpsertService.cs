using Kash.Domain;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Services;

/// <summary>
/// Servicio para encontrar o crear entidades de forma inteligente.
/// Busca primero si existe (case-insensitive), si no existe la crea.
/// </summary>
public interface IEntityUpsertService
{
    /// <summary>
    /// Encuentra o crea un Cliente.
    /// Busca por nombre (case-insensitive), si no existe lo crea.
    /// </summary>
    /// <param name="clienteId">ID proporcionado por el frontend</param>
    /// <param name="nombre">Nombre del cliente (requerido para crear si no existe)</param>
    /// <param name="usuarioId">ID del usuario propietario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>ClienteId de la entidad encontrada o creada</returns>
    Task<Result<ClienteId>> FindOrCreateClienteAsync(
        Guid? clienteId, 
        string? nombre, 
        UsuarioId usuarioId, 
        CancellationToken cancellationToken);

    /// <summary>
    /// Encuentra o crea una Persona.
    /// Busca por nombre (case-insensitive), si no existe la crea.
    /// </summary>
    Task<Result<PersonaId>> FindOrCreatePersonaAsync(
        Guid? personaId, 
        string? nombre, 
        UsuarioId usuarioId, 
        CancellationToken cancellationToken);

    /// <summary>
    /// Encuentra o crea un Concepto.
    /// Busca por nombre (case-insensitive), si no existe lo crea.
    /// </summary>
    Task<Result<ConceptoId>> FindOrCreateConceptoAsync(
        Guid conceptoId, 
        string? nombre, 
        CategoriaId categoriaId, 
        UsuarioId usuarioId, 
        CancellationToken cancellationToken);
}
