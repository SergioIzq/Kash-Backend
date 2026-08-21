## Context

Ver `proposal.md - Why` para la motivación (consumidor: `Kash-Frontend`, change `selectores-catalogo-completo`). Puntos de partida verificados en este repo:

- `ConceptosController.GetPagedList` (`GET /api/conceptos`) delega en `GetConceptosPagedListQuery` → `GetConceptosPagedListQueryHandler` (el archivo está mal nombrado como `GetClientesPagedListQueryHandler.cs`, copia-pega de otro catálogo — no se corrige aquí, fuera de alcance de este cambio). El handler hereda de `GetPagedListQueryHandler<Concepto, ConceptoId, ConceptoDto, GetConceptosPagedListQuery>` **sin sobrescribir `ApplyFiltersAsync`**, así que usa el comportamiento por defecto del kernel (paginación directa vía `GetPagedReadModelsByUserAsync`).
- `GetGastosPagedListQueryHandler` (mismo kernel) sí sobrescribe `ApplyFiltersAsync` para casos donde hace falta lógica específica, llamando directamente a `_dtoRepository.GetPagedReadModelsByUserAsync(usuarioId, page, pageSize, searchTerm, sortColumn, sortOrder, ct)` — es el precedente a seguir para Conceptos.
- `IReadRepository<T,TDto,TId>.GetPagedReadModelsByUserAsync(usuarioId, page, pageSize, searchTerm, sortColumn, sortOrder, ct)` (kernel, `SergioIzq.Domain.Kernel`) **no acepta un diccionario de filtros extra** — confirmado por reflexión sobre el ensamblado del paquete (a diferencia de `GetRecentAsync`/`SearchForAutocompleteAsync`, que sí lo aceptan). No hay forma de inyectar `categoriaId` a través de ese método sin ampliar el kernel compartido.
- `SearchConceptosQueryHandler`/`GetRecentConceptosQueryHandler` ya resuelven el mismo problema para `search`/`recent` sobrescribiendo `GetCustomFilters()`, que sí es un hook soportado por `GetRecentQueryHandler`/`SearchForAutocompleteQueryHandler` — pero ese hook no existe en `GetPagedListQueryHandler`.
- El cambio `sugerencias-y-habituales-transacciones` (ya archivado en este repo) estableció el patrón para estos casos: un repositorio de agregación/consulta propio en `Kash.Application/Interfaces` (no en `Kash.Domain`, que no referencia `Kash.Shared.Application` donde viven los DTOs), implementado con Dapper en `Kash.Infrastructure/Persistence/Query`, registrado manualmente en DI.
- `ConceptoReadRepository.ConfigureRepository()` (`Kash.Infrastructure/Persistence/Data/Conceptos/`) ya define alias `c` para la tabla `conceptos`, con joins/columnas ya usados por `search`/`recent` — mismo alias y columnas que se necesitan para el filtro paginado.

## Goals / Non-Goals

**Goals:**
- Permitir filtrar por `categoriaId` el listado paginado de Conceptos, manteniendo `searchTerm`/orden/paginación existentes.
- No romper el comportamiento actual cuando no se informa `categoriaId`.

**Non-Goals:**
- No se corrige el nombre de archivo mal puesto (`GetClientesPagedListQueryHandler.cs` para el handler de Conceptos) — fuera de alcance, señalado pero no tocado.
- No se amplía `IReadRepository`/`GetPagedListQueryHandler` del paquete kernel compartido con un hook de filtros extra genérico — se resuelve de forma local a Conceptos, igual que ya se hizo para `habituales`/`sugerencia`.
- No se añade filtro por categoría a ningún otro catálogo — ninguno de los otros 6 lo necesita (confirmado en `Kash-Frontend`, ninguno de sus stores pasa un filtro secundario a `search`).

## Decisions

**1. `GetConceptosPagedListQueryHandler` sobrescribe `ApplyFiltersAsync`: delega en el comportamiento genérico si no hay `categoriaId`, y en un método Dapper propio si lo hay.**
Cuando `query.CategoriaId` es nulo/vacío, se comporta exactamente igual que hoy (llamando a `_dtoRepository.GetPagedReadModelsByUserAsync(...)` directamente, mismo patrón que `GetGastosPagedListQueryHandler`). Cuando `categoriaId` viene informado, se llama a un método nuevo (`IConceptoPaginadoRepository.GetPagedByCategoriaAsync(usuarioId, categoriaId, page, pageSize, searchTerm, sortColumn, sortOrder, ct)`) implementado con Dapper en `ConceptoReadRepository`-equivalente en `Kash.Infrastructure/Persistence/Query/`, filtrando por `c.id_categoria = @CategoriaId` además de `c.id_usuario = @UsuarioId`, reutilizando `searchTerm` sobre `c.nombre` igual que el listado genérico.
*Alternativa descartada*: ampliar `GetPagedReadModelsByUserAsync` del kernel para aceptar un diccionario de filtros extra (como ya tiene `GetRecentAsync`) — sería la solución "correcta" a largo plazo, pero toca un paquete compartido fuera de este repo (`SergioIzq.Domain.Kernel`/`SergioIzq.Infrastructure.Kernel`), con impacto en todos los consumidores de esa librería, no solo Kash. Se descarta para este cambio; si en el futuro aparecen más casos similares, valorar proponerlo en el kernel entonces.

**2. El nuevo repositorio (`IConceptoPaginadoRepository`) vive en `Kash.Application/Interfaces`, no extiende `IConceptoReadRepository` de `Kash.Domain`.**
Mismo motivo que en `sugerencias-y-habituales-transacciones`: `Kash.Domain.csproj` no referencia `Kash.Shared.Application` (donde vive `ConceptoDto`), así que una interfaz de Domain no puede declarar un método que devuelva `PagedList<ConceptoDto>`. Se sigue el patrón ya usado por `IGastoHabitualesRepository`/`IGastoSugerenciaRepository`: interfaz en Application, implementación Dapper en Infrastructure, registro manual en DI (no vía el escaneo automático de Scrutor que cubre las interfaces marcador de Domain).

**3. Sin caché para este endpoint, igual que el listado paginado de Conceptos ya no la tiene hoy.**
El listado paginado genérico (`GetPagedListQueryHandler`) no aplica caché de lista completa como sí hace `GetRecentQueryHandler` (30s) — no se introduce ninguna donde no la había, para no cambiar el comportamiento de frescura ya esperado por el listado paginado.

## Risks / Trade-offs

- **[Riesgo] Solución específica a Conceptos, no reutilizable si aparece un caso similar en otro catálogo.** Aceptado: hoy ningún otro catálogo necesita un filtro secundario en su paginado (verificado en el frontend), así que generalizar ahora sería especular sobre una necesidad que no existe todavía.
- **[Riesgo] Dos caminos de código en el mismo handler** (`ApplyFiltersAsync` con y sin `categoriaId`) en vez de un único camino — algo más de código que un solo método, pero evita cualquier cambio de comportamiento para el caso ya existente (sin `categoriaId`), que es el más usado hoy (pantalla de listado de Conceptos).
- **[Riesgo] Nombre de archivo del handler ya está equivocado** (`GetClientesPagedListQueryHandler.cs` para Conceptos) — no se corrige aquí para no mezclar un cambio de higiene de código con este cambio funcional; queda anotado para quien quiera limpiarlo en otro momento.

## Migration Plan

No aplica migración de datos ni cambios de esquema: el filtro opera sobre la columna `id_categoria` ya existente en `conceptos`. Sin rollback especial: el parámetro es opcional y aditivo; el comportamiento sin `categoriaId` no cambia.
