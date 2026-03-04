namespace Kash.Shared.Application.Interfaces;

/// <summary>
/// ?? Interfaz marcadora para servicios que deben ser registrados automáticamente.
/// Todos los servicios que implementen esta interfaz serán registrados como Scoped.
/// </summary>
public interface IApplicationService
{
}

/// <summary>
/// ?? Interfaz marcadora para servicios transient (rápidos, sin estado).
/// </summary>
public interface ITransientService
{
}

/// <summary>
/// ?? Interfaz marcadora para servicios singleton (estado compartido).
/// </summary>
public interface ISingletonService
{
}
