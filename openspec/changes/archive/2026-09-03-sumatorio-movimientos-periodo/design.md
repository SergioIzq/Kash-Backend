## Context

Ver `proposal.md` - Why. Estado actual relevante para el diseño:

- `GastoPeriodoRepository`/`IngresoPeriodoRepository` (Dapper, SQL crudo) ya ejecutan un `SELECT COUNT(*) ... WHERE {whereClause}` antes del `SELECT` paginado, reutilizando el mismo `whereClause` (usuario + rango de fechas) para ambas queries.
- `IGastoPeriodoRepository`/`IIngresoPeriodoRepository` y sus handlers (`GetGastosPorPeriodoQueryHandler`/`GetIngresosPorPeriodoQueryHandler`) son réplicas exactas salvo el tipo de DTO (`GastoDto`/`IngresoDto`).
- `AbsController.SendAndHandleAsync<T>`/`HandleResult<T>` (kernel) son completamente genéricos sobre `T` - cualquier record serializable funciona como valor de `Result<T>`, no hay acoplamiento a `PagedList<T>`.
- El proyecto ya tiene el patrón de records ad-hoc para resultados compuestos de un query/command (`BulkCreateResult`, `PreviewMovimientosResult`, `ImportarMovimientosResult`).

## Goals / Non-Goals

**Goals:**
- Definir la forma exacta del nuevo tipo de retorno que envuelve `PagedList<T>` + el sumatorio.
- Definir cómo se calcula el sumatorio en SQL sin añadir una ida adicional a base de datos.
- Mantener Gastos e Ingresos estructuralmente idénticos (mismo tipo de wrapper, mismo cambio de firma, mismo patrón SQL).

**Non-Goals:**
- Adaptar el frontend (`Kash-Frontend`) al nuevo contrato - trabajo aparte en su propio repo/store.
- Tocar el `PagedList<T>` genérico del kernel (`SergioIzq.Domain.Kernel`) - se sigue usando tal cual, solo se envuelve.
- Implementar o reactivar `/gastos/resumen` - no existe en el backend hoy (ver hallazgo de la fase de exploración) y no forma parte de este change.
- Cambiar el comportamiento de paginación en sí (page/pageSize/orden) - solo se añade el sumatorio junto a lo que ya existe.

## Decisions

**1. Un record genérico compartido, no uno por feature.**
`Kash.Shared.Application.Dtos` gana un nuevo tipo:
```csharp
public sealed record PeriodoResult<T>(PagedList<T> Pagina, decimal SumaImporte);
```
Alternativa considerada: dos records ad-hoc casi idénticos (`GastosPorPeriodoResult`, `IngresosPorPeriodoResult`) siguiendo literalmente el patrón `BulkCreateResult`/`PreviewMovimientosResult`. Se descarta porque, a diferencia de esos casos (cada uno con campos propios y significado distinto), aquí la forma es idéntica letra por letra entre Gastos e Ingresos - duplicarla no aporta nada y diverge en cuanto alguien tenga que recordar mantener los dos en sync. Un genérico en `Shared.Application.Dtos` (ya el lugar de `GastoDto`/`IngresoDto`) es la opción más simple que sigue evitando tocar `PagedList<T>` del kernel.

**2. El repositorio devuelve `PeriodoResult<T>` directamente, no una tupla.**
`GetPagedByPeriodoAsync` pasa de `Task<PagedList<GastoDto>>` a `Task<PeriodoResult<GastoDto>>`. El repositorio ya depende de `Kash.Shared.Application.Dtos` (para `GastoDto`), así que depender también de `PeriodoResult<T>` no añade una dependencia nueva. El handler sigue siendo un passthrough de una línea (`Result.Success(resultado)`), igual que hoy - no gana lógica propia.

**3. SUM(importe) en la misma consulta que el COUNT(*) existente, no en una query aparte.**
El `importe` vive en la tabla base (`gastos`/`ingresos`), sin necesidad de ningún `LEFT JOIN` de los que ya hace el `SELECT` paginado, así que puede calcularse junto al conteo, reutilizando el mismo `whereClause`:
```sql
SELECT COUNT(*), COALESCE(SUM(g.importe), 0)
FROM gastos g
WHERE {whereClause}
```
mapeado con `QuerySingleAsync<(int TotalCount, decimal SumaImporte)>` (Dapper soporta proyectar una fila de N columnas a un `ValueTuple` posicional). Alternativa considerada: dos `ExecuteScalarAsync` separados (uno para `COUNT`, otro para `SUM`) - se descarta porque duplicaría una ida a base de datos que ya existe, sin ningún beneficio (misma tabla, mismo `WHERE`).

**4. Sin cambios en `GastosController`/`IngresosController`.**
`SendAndHandleAsync<T>` ya es genérico; cambiar `IQuery<PagedList<GastoDto>>` a `IQuery<PeriodoResult<GastoDto>>` en la query no requiere tocar el controller.

## Risks / Trade-offs

- **[Riesgo] El contrato JSON de `GET /gastos/periodo` y `GET /ingresos/periodo` cambia de forma** (`value.items` pasa a `value.pagina.items`) → **Mitigación**: es un cambio consciente y documentado en el proposal; el único consumidor conocido (`Kash-Frontend`) se actualiza como trabajo aparte antes de desplegar a producción, no hay otros clientes de esta API.
- **[Riesgo] `PeriodoResult<T>` es un tipo nuevo específico de este caso de uso, no reutilizable por el repositorio genérico del kernel** → **Mitigación**: aceptado explícitamente en el proposal - `PagedList<T>` sigue siendo el tipo genérico de paginación en los otros 60+ usos; `PeriodoResult<T>` es deliberadamente local a Gastos/Ingresos por periodo.

## Migration Plan

No hay migración de datos (no cambia el esquema de BBDD). Despliegue: mergear y desplegar el backend; el frontend sigue funcionando hasta que se actualice porque no se toca ningún otro endpoint - solo dejan de funcionar `getGastosPorPeriodo`/`getIngresosPorPeriodo` en el frontend hasta su propia actualización (fuera de este change). No se requiere feature flag: es un único consumidor interno, no una API pública versionada.
