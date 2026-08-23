namespace Kash.Shared.Application.Dtos;

/// <summary>
/// Estado del token de API personal del usuario. Nunca incluye el valor del token en sí.
/// </summary>
public record ApiTokenStatusDto(bool Existe, DateTime? CreadoEn);
