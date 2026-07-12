namespace Kash.Shared.Application.Dtos.Reportes;

/// <summary>Archivo PDF generado, listo para descarga (contenido + nombre sugerido).</summary>
public sealed record PresupuestoArchivoDto(string NombreArchivo, byte[] Contenido);
