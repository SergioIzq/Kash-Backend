namespace Kash.Application.Features.Movimientos.Commands.Import.Models;

/// <summary>Error de importación asociado a una línea concreta del extracto (0 = error global).</summary>
public sealed record MovimientoImportError(int Linea, string Contenido, string Razon);
