## Why

El frontend ya tiene desde hace tiempo `GastoService.getGastosPorPeriodo` e `IngresoService.getIngresosPorPeriodo`, apuntando a `GET /api/gastos/periodo` y `GET /api/ingresos/periodo`, pero esas rutas nunca se implementaron en el backend. Al no existir `[HttpGet("periodo")]`, la petición cae en `[HttpGet("{id}")]` e intenta convertir el literal `"periodo"` a `Guid`, devolviendo 400. Esto bloquea la nueva tabla de "movimientos rápidos" (filtrable por Hoy/Esta semana/Este mes/rango personalizado) del change `movimientos-rapidos-gastos-ingresos` en Kash-Frontend, cuyas tareas 4.2 en adelante dependen de este endpoint.

## What Changes

- Añadir `GET /api/gastos/periodo` y `GET /api/ingresos/periodo`, con la firma exacta que ya consume el frontend: `fechaInicio`, `fechaFin` (fecha, obligatorias) y `pageSize`/`page` opcionales (mismos defaults que el listado paginado).
- El filtrado es por el campo `Fecha` de la transacción (la fecha que introduce el usuario al crear/editar), no por ninguna fecha de auditoría/creación del registro.
- Ambos endpoints van scoped al usuario autenticado, con el mismo patrón `RequireCurrentUserId`/`UsuarioId` que ya usan `GetPagedList`, `GetSugerencia` y `GetHabituales` en estos controllers.
- La respuesta usa el mismo envelope que el listado paginado existente: `Result<PagedList<GastoDto>>` / `Result<PagedList<IngresoDto>>`, para que el frontend siga pudiendo leer `response.value.items` sin cambios en su lado.
- Internamente, esto requiere una query CQRS nueva por entidad (no reutilizar `AbsGetPagedListQuery`/`GetPagedListQueryHandler` del kernel, cuyo `IReadRepository.GetPagedReadModelsByUserAsync` no acepta rango de fechas), junto con un repositorio manual nuevo por entidad que reutilice el patrón SQL ya usado en la exportación a Excel (`WHERE fecha BETWEEN @FechaInicio AND @FechaFin`, mismas columnas/joins que `GastoReadRepository`/`IngresoReadRepository`), añadiendo paginación real (`COUNT` + `LIMIT`/`OFFSET`).

## Capabilities

### New Capabilities
- `movimientos-por-periodo`: listado paginado de Gastos o Ingresos del usuario autenticado filtrado por rango de fechas sobre el campo `Fecha` de la transacción.

### Modified Capabilities
(ninguna — no se modifica el comportamiento de los requisitos existentes de listado paginado ni de exportación a Excel)

## Impact

- **Kash.Api**: `GastosController`, `IngresosController` (nuevo endpoint `periodo` en cada uno).
- **Kash.Application**: nuevas queries `GetGastosPorPeriodoQuery`/`GetIngresosPorPeriodoQuery` + handlers, nuevas interfaces de repositorio manual (p. ej. `IGastoPeriodoRepository`/`IIngresoPeriodoRepository`).
- **Kash.Infrastructure**: nuevas implementaciones de esos repositorios (Dapper, mismo patrón que `GastoExportRepository`/`IngresoExportRepository`), registro en `DependencyInjection.cs` (sección de "Repositorios Manuales").
- **Kash-Frontend**: ninguno — la firma de la petición y del envelope ya está escrita y no cambia; solo desbloquea las tareas pendientes (4.2+) del change `movimientos-rapidos-gastos-ingresos`.
