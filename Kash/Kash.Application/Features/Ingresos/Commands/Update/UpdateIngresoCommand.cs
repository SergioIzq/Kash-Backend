using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Ingresos.Commands;

/// <summary>
/// Representa la solicitud para actualizar un Ingreso.
/// 🔥 Soporta auto-creación: Si Concepto/Cliente/Persona/Cuenta/FormaPago no existen, se crean automáticamente usando los campos *Nombre.
/// </summary>
public sealed record UpdateIngresoCommand : AbsUpdateCommand<Ingreso, IngresoId, IngresoDto>
{
    public required decimal Importe { get; init; }
    public required DateTime Fecha { get; init; }
    
    // 🔥 Categoría: Puede buscarse por ID o crearse con Nombre
    public required Guid CategoriaId { get; init; }
    public string? CategoriaNombre { get; init; } // 🔥 Para auto-creación
    
    // 🔥 Concepto: Puede buscarse por ID o crearse con Nombre (usa CategoriaId)
    public required Guid ConceptoId { get; init; }
    public string? ConceptoNombre { get; init; } // 🔥 Para auto-creación
    
    // 🔥 Cliente: Puede buscarse por ID o crearse con Nombre (opcional)
    public required Guid? ClienteId { get; init; }
    public string? ClienteNombre { get; init; } // 🔥 Para auto-creación
    
    // 🔥 Persona: Puede buscarse por ID o crearse con Nombre (opcional)
    public required Guid? PersonaId { get; init; }
    public string? PersonaNombre { get; init; } // 🔥 Para auto-creación
    
    // 🔥 Cuenta: Puede buscarse por ID o crearse con Nombre
    public required Guid CuentaId { get; init; }
    public string? CuentaNombre { get; init; } // 🔥 Para auto-creación
    
    // 🔥 FormaPago: Puede buscarse por ID o crearse con Nombre
    public required Guid FormaPagoId { get; init; }
    public string? FormaPagoNombre { get; init; } // 🔥 Para auto-creación
    
    public required Guid UsuarioId { get; init; }
    public required string? Descripcion { get; init; }
}