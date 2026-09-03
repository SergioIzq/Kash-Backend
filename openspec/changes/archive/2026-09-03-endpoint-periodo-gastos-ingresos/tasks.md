## 1. Application (Gastos): query, handler y contrato de repositorio

- [x] 1.1 Crear `GetGastosPorPeriodoQuery` en `Kash.Application/Features/Gastos/Queries/GetPeriodo/` (record propio, no hereda de `AbsGetPagedListQuery`) con `UsuarioId`, `FechaInicio`, `FechaFin`, `Page`, `PageSize`. Verificar que compila.
- [x] 1.2 Crear `IGastoPeriodoRepository` en `Kash.Application/Interfaces/` con `Task<PagedList<GastoDto>> GetPagedByPeriodoAsync(Guid usuarioId, DateTime fechaInicio, DateTime fechaFin, int page, int pageSize, CancellationToken cancellationToken = default)`. Verificar que compila.
- [x] 1.3 Crear `GetGastosPorPeriodoQueryHandler` que valide `FechaInicio <= FechaFin` (mismo mensaje/patrón de error que `GetGastosExcelQueryHandler`) devolviendo `Error.Validation` si no se cumple, y en caso contrario delegue en `IGastoPeriodoRepository.GetPagedByPeriodoAsync`. Verificar con un test unitario (o prueba manual) que `fechaInicio > fechaFin` devuelve `Result.Failure` sin llegar al repositorio.

## 2. Infrastructure (Gastos): repositorio Dapper y registro DI

- [x] 2.1 Implementar `GastoPeriodoRepository : IGastoPeriodoRepository` en `Kash.Infrastructure/Persistence/Query/`, reutilizando el mismo `SELECT`/`JOIN`/columnas que `GastoExportRepository`, con `WHERE g.id_usuario = @UsuarioId AND g.fecha BETWEEN @FechaInicio AND @FechaFin`, más una consulta `COUNT(*)` para `TotalCount` y `LIMIT @PageSize OFFSET @Offset` para la página solicitada. Verificar manualmente contra la base de datos de desarrollo que devuelve el número de filas y el total esperados para un rango conocido.
- [x] 2.2 Registrar `services.AddScoped<ApplicationInterface.IGastoPeriodoRepository, GastoPeriodoRepository>()` en `Kash.Infrastructure/DependencyInjection.cs`, junto a los demás "Repositorios Manuales". Verificar que la app arranca sin errores de resolución de dependencias.

## 3. Api (Gastos): endpoint

- [x] 3.1 Añadir `[HttpGet("periodo")]` en `GastosController` con `[FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin, [FromQuery] int page = 1, [FromQuery] int pageSize = 10`, usando `RequireCurrentUserId` (mismo patrón que `GetPagedList`/`GetSugerencia`/`GetHabituales`) y `SendAndHandleAsync`. Verificar con una petición real (`GET /api/gastos/periodo?fechaInicio=...&fechaFin=...&pageSize=1000`) autenticada que devuelve 200 con el envelope `{ value: { items: [...], page, pageSize, totalCount, ... } }`.
- [x] 3.2 Verificar manualmente que `GET /api/gastos/{id}` con un `id` real sigue funcionando sin cambios (confirmar que no hay colisión de rutas tras añadir `periodo`).
- [x] 3.3 Verificar manualmente que `fechaInicio` posterior a `fechaFin` devuelve 400 con el error de validación, y que un rango sin resultados devuelve 200 con `items: []`.

## 4. Application (Ingresos): mismo patrón que Gastos

- [x] 4.1 Aplicar el mismo cambio que 1.1-1.3 a Ingresos: `GetIngresosPorPeriodoQuery`, `IIngresoPeriodoRepository`, `GetIngresosPorPeriodoQueryHandler` en sus rutas equivalentes bajo `Kash.Application/Features/Ingresos/`. Verificar que compila y que el mismo test de validación de rango de fechas pasa para Ingresos.

## 5. Infrastructure (Ingresos): repositorio Dapper y registro DI

- [x] 5.1 Implementar `IngresoPeriodoRepository : IIngresoPeriodoRepository` en `Kash.Infrastructure/Persistence/Query/`, reutilizando el `SELECT`/`JOIN`/columnas de `IngresoExportRepository` más `COUNT`/`LIMIT`/`OFFSET`. Verificar manualmente contra la base de datos de desarrollo igual que en 2.1.
- [x] 5.2 Registrar `IIngresoPeriodoRepository`/`IngresoPeriodoRepository` en `DependencyInjection.cs`. Verificar que la app arranca sin errores.

## 6. Api (Ingresos): endpoint

- [x] 6.1 Añadir `[HttpGet("periodo")]` en `IngresosController`, mismo patrón que 3.1. Verificar con una petición real que devuelve 200 con el envelope esperado.
- [x] 6.2 Verificar manualmente que `GET /api/ingresos/{id}` sigue funcionando sin cambios y que el rango inválido/vacío se comporta igual que en 3.3.

## 7. Verificación end-to-end con el frontend

- [x] 7.1 Con `Kash-Frontend` apuntando a este backend, comprobar en Gastos e Ingresos que los filtros Hoy/Esta semana/Este mes/rango personalizado de la tabla de "movimientos rápidos" (tareas 4.2+ del change `movimientos-rapidos-gastos-ingresos` en Kash-Frontend) cargan datos correctamente y ya no reciben 400.
