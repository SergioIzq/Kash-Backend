## Why

El frontend (`Kash-Frontend`) tiene en marcha el cambio `agilizar-alta-transacciones` para reducir la fricción de registrar gastos/ingresos cotidianos (ej. "comprar el pan"), pre-rellenando el formulario a partir del histórico del concepto elegido y ofreciendo combinaciones habituales de un toque. Ese cambio depende de dos capacidades de lectura que hoy no existen en la API: obtener la combinación más reciente de una transacción para un concepto dado, y obtener las combinaciones completas más repetidas por el usuario. Sin estos endpoints, el frontend no tiene de dónde consumir esos datos.

## What Changes

- Nuevo endpoint `GET /api/gastos/sugerencia?conceptoId={id}` (y su equivalente `GET /api/ingresos/sugerencia?conceptoId={id}`) que devuelve la transacción más reciente registrada por el usuario para ese concepto (cuenta, forma de pago, importe, proveedor/cliente, persona), reutilizando el patrón genérico `GetRecentQueryHandler` ya usado por Conceptos/Categorías/Proveedores/Personas/Cuentas/FormasPago, sin DTO nuevo (reutiliza `GastoDto`/`IngresoDto`).
- Nuevo endpoint `GET /api/gastos/habituales?limit={n}` (y su equivalente `GET /api/ingresos/habituales?limit={n}`) que devuelve el top-N de combinaciones completas (concepto, categoría, cuenta, forma de pago, proveedor/cliente o persona) más repetidas por el usuario, con nº de usos y fecha de último uso, mediante una query/handler propios (no encajan en los patrones genéricos de paginado o "recent" de una entidad simple) y un método nuevo en el repositorio de lectura (`GetHabitualesAsync`, SQL agregado vía Dapper, mismo patrón que los métodos custom ya existentes como `ExistsWithSameNameAsync`).
- Ambos endpoints son de solo lectura; no se modifica el modelo de datos ni los comandos de creación/edición existentes de Gasto/Ingreso.

## Capabilities

### New Capabilities
- `sugerencias-transaccion`: consulta de la combinación más reciente de gasto/ingreso asociada a un concepto.
- `transacciones-habituales`: consulta de las combinaciones completas de gasto/ingreso más repetidas por el usuario.

### Modified Capabilities
(ninguna — no existen specs previas en este repo; `GastosController`/`IngresosController` se extienden con endpoints nuevos, no se cambia el comportamiento de los existentes)

## Impact

- **`Kash.Api`**: nuevas acciones en `GastosController` e `IngresosController` (`GET .../sugerencia`, `GET .../habituales`), siguiendo el patrón `[Authorize]` + `RequireCurrentUserId` + `SendAndHandleAsync` ya usado en el resto de acciones de estos controllers.
- **`Kash.Application`**: nuevas queries/handlers en `Features/Gastos/Queries/Sugerencia` y `Features/Gastos/Queries/Habituales` (y equivalentes en `Features/Ingresos`).
- **`Kash.Domain`**: `IGastoReadRepository`/`IIngresoReadRepository` ganan un método nuevo (`GetHabitualesAsync`), siguiendo el patrón ya usado por `ExistsWithSameNameAsync`/`GetByNameAsync` en otros repositorios de catálogo.
- **`Kash.Infrastructure`**: implementación Dapper de `GetHabitualesAsync` en `GastoReadRepository`/`IngresoReadRepository`; sin cambios de esquema de base de datos (no se crean tablas ni columnas nuevas).
- **`Kash.Shared.Application`**: nuevo DTO (`GastoHabitualDto`/`IngresoHabitualDto`) para la respuesta de `habituales`; el endpoint de `sugerencia` no necesita DTO nuevo.
- **Consumidor externo**: `Kash-Frontend`, change `agilizar-alta-transacciones` (repo separado, sin acoplamiento de código — solo de contrato HTTP).
