## 1. Tipo compartido

- [x] 1.1 Añadir `public sealed record PeriodoResult<T>(PagedList<T> Pagina, decimal SumaImporte);` en `Kash.Shared.Application.Dtos` y verificar que el proyecto compila (`dotnet build Kash.Shared.Application`).

## 2. Gastos

- [x] 2.1 Cambiar `IGastoPeriodoRepository.GetPagedByPeriodoAsync` para devolver `Task<PeriodoResult<GastoDto>>` en vez de `Task<PagedList<GastoDto>>`, y verificar que el proyecto de interfaces compila.
- [x] 2.2 En `GastoPeriodoRepository`, extender la query de conteo existente para calcular también `SUM(g.importe)` reutilizando el mismo `whereClause` (una sola consulta, con `COALESCE(SUM(...), 0)` para el caso sin filas, mapeada a un record `TotalesPeriodo(int TotalCount, decimal SumaImporte)` en vez de un `ValueTuple` porque Dapper no soporta binding posicional a tuplas para una fila plana), construir el `PeriodoResult<GastoDto>` resultante, y verificar manualmente contra la base de datos de desarrollo que el sumatorio coincide con `SELECT SUM(importe) FROM gastos WHERE ...` para un usuario y rango de fechas conocidos.
- [x] 2.3 Cambiar `GetGastosPorPeriodoQuery` de `IQuery<PagedList<GastoDto>>` a `IQuery<PeriodoResult<GastoDto>>` y verificar que compila.
- [x] 2.4 Actualizar `GetGastosPorPeriodoQueryHandler` (tipo de retorno `Result<PeriodoResult<GastoDto>>`, el resto del handler no cambia) y verificar que compila.

## 3. Ingresos (réplica simétrica de Gastos)

- [x] 3.1 Cambiar `IIngresoPeriodoRepository.GetPagedByPeriodoAsync` para devolver `Task<PeriodoResult<IngresoDto>>` y verificar que compila.
- [x] 3.2 En `IngresoPeriodoRepository`, aplicar el mismo cambio de query que en 2.2 (`SUM(i.importe)` junto al `COUNT(*)` existente, mismo `whereClause`), y verificar manualmente el sumatorio contra `SELECT SUM(importe) FROM ingresos WHERE ...` para un usuario y rango conocidos.
- [x] 3.3 Cambiar `GetIngresosPorPeriodoQuery` a `IQuery<PeriodoResult<IngresoDto>>` y verificar que compila.
- [x] 3.4 Actualizar `GetIngresosPorPeriodoQueryHandler` y verificar que compila.

## 4. Verificación end-to-end

- [x] 4.1 Compilar toda la solución (`dotnet build`) y verificar que no hay errores en `Kash.Api`, `Kash.Application`, `Kash.Infrastructure` ni `Kash.Shared.Application`.
- [x] 4.2 Levantar la API en local y probar `GET /api/gastos/periodo` y `GET /api/ingresos/periodo` (p. ej. vía Swagger o curl) con distintos `page`/`pageSize` para el mismo rango de fechas, y verificar que `value.sumaImporte` es idéntico en todas las combinaciones y coincide con la suma real de los registros del rango.
- [x] 4.3 Probar un rango de fechas sin registros para el usuario autenticado y verificar que la respuesta trae `value.pagina.items` vacío y `value.sumaImporte` en `0`, sin error.
