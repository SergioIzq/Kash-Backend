## Why

Las vistas de "movimientos rápidos" por periodo (Gastos e Ingresos) necesitan mostrar el importe total de todos los movimientos del rango de fechas seleccionado, no solo de la página actual. Hoy el frontend pide `pageSize=1000` para traer "todos" los registros de un periodo y podría sumarlos en cliente, pero eso no es correcto de forma general (deja de serlo en cuanto el periodo supere ese tamaño de página) ni es responsabilidad del cliente recalcular una agregación que la base de datos ya puede resolver en la misma consulta que cuenta los registros.

## What Changes

- `GET /gastos/periodo` y `GET /ingresos/periodo` devuelven, además del listado paginado actual, la suma del importe de **todos** los registros que cumplen el filtro de fecha (no solo los de la página solicitada), calculada en servidor de forma independiente de `page`/`pageSize`.
- El resultado de ambos endpoints pasa de `Result<PagedList<GastoDto>>` / `Result<PagedList<IngresoDto>>` a un nuevo record que envuelve el `PagedList<T>` existente junto al nuevo campo de suma (p. ej. `Pagina` + `SumaImporte`), siguiendo el patrón ya usado en el proyecto para resultados compuestos ad-hoc (`BulkCreateResult`, `PreviewMovimientosResult`, etc.).
- La suma se calcula en la misma consulta SQL que ya obtiene el `COUNT(*)` para la paginación, reutilizando el mismo `whereClause` (usuario + rango de fechas), para no añadir una ida adicional a base de datos.
- El cambio se aplica simétricamente a Gastos e Ingresos en el mismo change, dado que ambos endpoints son réplicas exactas a nivel de controller/query/handler/repositorio.

## Capabilities

### New Capabilities

(ninguna)

### Modified Capabilities

- `movimientos-por-periodo`: nuevo requisito — el listado paginado por periodo debe ir acompañado del sumatorio del importe de todos los registros que cumplen el filtro de fecha, no solo los de la página actual.

## Impact

- **Kash.Application**: `GetGastosPorPeriodoQuery`/`GetGastosPorPeriodoQueryHandler` y sus gemelos en Ingresos cambian su tipo de retorno de `PagedList<T>` a un nuevo record compuesto.
- **Kash.Application.Interfaces**: `IGastoPeriodoRepository`/`IIngresoPeriodoRepository` cambian la firma de `GetPagedByPeriodoAsync` para devolver también la suma.
- **Kash.Infrastructure**: `GastoPeriodoRepository`/`IngresoPeriodoRepository` extienden la consulta de conteo existente para calcular también `SUM(importe)`.
- **Kash.Api**: sin cambios de código (los controllers ya delegan el tipo de resultado a `SendAndHandleAsync`), pero el contrato JSON de `GET /gastos/periodo` y `GET /ingresos/periodo` cambia de forma (deja de tener `items` en la raíz de `value` para tenerlo bajo `value.pagina.items`).
- **Fuera de alcance de este change**: la adaptación del frontend (`Kash-Frontend`) al nuevo contrato — vive en su propio repositorio/store de OpenSpec y se abordará como trabajo aparte una vez publicado este contrato.
