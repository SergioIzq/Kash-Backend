## Context

Ver `proposal.md` - Why. Dos hechos técnicos, verificados directamente sobre los binarios de `SergioIzq.*.Kernel 0.2.9` (vía reflexión, ya que el kernel no tiene el código fuente en este repo) y sobre el código existente:

- `IReadRepository<TEntity,TDto,TId>.GetPagedReadModelsByUserAsync(usuarioId, page, pageSize, searchTerm, sortColumn, sortOrder, ct)` — el método que usa `GetGastosPagedListQueryHandler`/`GetIngresosPagedListQueryHandler` (vía `AbsGetPagedListQuery`/`GetPagedListQueryHandler` del kernel) — no acepta rango de fechas en ninguna de sus sobrecargas. No se puede resolver este endpoint añadiendo campos a `AbsGetPagedListQuery`.
- Ya existe un patrón de filtrado por rango de fechas sobre `Fecha` scoped a usuario: `GetGastosExcelQuery`/`GetIngresosExcelQuery`, con `IGastoExportRepository`/`IIngresoExportRepository` implementados en Dapper (`GastoExportRepository`/`IngresoExportRepository`), registrados manualmente en `DependencyInjection.cs` (sección "Repositorios Manuales"). Ese repositorio no pagina - devuelve `IReadOnlyList<GastoDto>` completo, pensado para exportar a Excel.

## Goals / Non-Goals

**Goals:**
- Reutilizar el patrón SQL/WHERE ya probado de la exportación (mismas columnas, mismos joins, mismo `fecha BETWEEN`, mismo scoping por `id_usuario`) para evitar reimplementar el mapeo de columnas.
- Añadir paginación real a nivel de base de datos (no traer todo a memoria y paginar en C#), consistente con cómo pagina el listado general.
- Mantener el contrato HTTP exacto que ya consume el frontend (`GET /api/gastos/periodo?fechaInicio=...&fechaFin=...&pageSize=...`, envelope `Result<PagedList<GastoDto>>`).

**Non-Goals:**
- No se añaden filtros adicionales (concepto, categoría, proveedor/cliente, persona, búsqueda de texto) al endpoint de periodo - el frontend actual solo necesita fecha. Si se necesitan más adelante, es una extensión futura del mismo patrón que ya usa la exportación.
- No se modifica el listado paginado general (`GetPagedList`) ni la exportación a Excel existentes.
- No se toca el paquete `SergioIzq.*.Kernel` (versión fijada en 0.2.9) para añadir soporte de fechas al repositorio genérico.

## Decisions

**Query CQRS independiente de `AbsGetPagedListQuery`.** `GetGastosPorPeriodoQuery`/`GetIngresosPorPeriodoQuery` serán records propios (no heredan de `AbsGetPagedListQuery<TEntity,TId,TDto>`), con `UsuarioId`, `FechaInicio`, `FechaFin`, `Page`, `PageSize`. Alternativa descartada: extender `AbsGetPagedListQuery` con campos de fecha - inviable porque el handler base del kernel llama a `GetPagedReadModelsByUserAsync`, que no los aceptaría; habría que reimplementar el handler base de todos modos, sin ganar nada por heredar.

**Repositorio manual nuevo por entidad, mismo patrón que el de exportación.** Nuevas interfaces `IGastoPeriodoRepository`/`IIngresoPeriodoRepository` en `Kash.Application.Interfaces`, con un método `GetPagedByPeriodoAsync(usuarioId, fechaInicio, fechaFin, page, pageSize, ct)` que devuelve `PagedList<GastoDto>`/`PagedList<IngresoDto>`. Implementación en `Kash.Infrastructure.Persistence.Query`, con el mismo `SELECT`/`JOIN`/`WHERE fecha BETWEEN` que `GastoExportRepository`/`IngresoExportRepository`, más `COUNT(*)` (para `TotalCount`) y `LIMIT @PageSize OFFSET @Offset` (Dapper sobre MySQL, mismo motor que usa el resto del proyecto). Alternativa descartada: extender `IGastoExportRepository` con un método paginado - se descarta porque esa interfaz documenta explícitamente "sin paginar" como parte de su contrato (ver `GastoExportFiltro`/`IGastoExportRepository`), y mezclar ambos usos complicaría su lectura; el coste de una interfaz nueva y pequeña es menor que el de forzar dos semánticas distintas en una misma abstracción.

**Validación de rango igual que en la exportación.** Igual que `GetGastosExcelQueryHandler`/`GetIngresosExcelQueryHandler`, el handler valida `FechaInicio <= FechaFin` y devuelve `Error.Validation` si no se cumple, antes de tocar el repositorio.

**Registro de rutas.** `[HttpGet("periodo")]` en ambos controllers, sin necesidad de reordenar respecto a `[HttpGet("{id}")]` - ASP.NET Core ya prioriza segmentos de ruta literales sobre parámetros de ruta independientemente del orden de declaración (lo confirman `sugerencia` y `habituales`, declarados después de `{id}` en el código actual y funcionando en producción).

## Risks / Trade-offs

- [Duplicación de SQL entre el repositorio de exportación y el nuevo repositorio de periodo (mismas columnas/joins en dos sitios)] → Aceptado conscientemente: es el mismo trade-off que ya existe entre `GastoReadRepository` (listado paginado) y `GastoExportRepository` (exportación) en el código actual del proyecto; no se introduce un patrón nuevo, solo se sigue el ya establecido.
- [`pageSize` sin límite superior podría permitir traer un número muy grande de filas] → El frontend siempre pide `pageSize=1000`; se puede documentar como límite recomendado en tasks, pero no es un requisito de spec nuevo (el listado general tampoco impone uno hoy).

## Migration Plan

No aplica migración de datos. Es un endpoint nuevo, aditivo, sin impacto en endpoints existentes. Despliegue estándar (build + deploy del backend); el frontend ya tiene el código consumidor escrito y solo empezará a recibir 200 en vez de 400.
