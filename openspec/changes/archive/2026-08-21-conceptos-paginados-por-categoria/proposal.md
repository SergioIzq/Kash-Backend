## Why

El frontend (`Kash-Frontend`, change `selectores-catalogo-completo`) quiere que los selectores de catálogo (Concepto, Categoría, Cuenta, Forma de Pago, Proveedor/Cliente, Persona) dejen de limitarse a "recientes" o a resultados de búsqueda por texto, y permitan recorrer con scroll el catálogo completo del usuario, reutilizando los endpoints paginados ya existentes. Para 6 de los 7 catálogos, el endpoint paginado ya sirve tal cual. Para Concepto no: cuando el usuario ya tiene una Categoría elegida, el selector de Concepto necesita mostrar solo los Conceptos de esa Categoría, y el endpoint paginado de Conceptos (`GET /api/conceptos`) no soporta ese filtro hoy — solo lo soportan `search` y `recent`.

## What Changes

- Añadir un parámetro opcional `categoriaId` a `GET /api/conceptos` (listado paginado), que cuando se informa restringe los Conceptos devueltos a los de esa Categoría, además de la paginación/búsqueda/orden ya existentes.
- Sin cambios en `search`/`recent` de Conceptos (ya soportan `categoriaId` desde antes) ni en ningún otro catálogo (Categoria, Cuenta, FormaPago, Proveedor, Persona, Cliente no necesitan cambios: su endpoint paginado ya sirve "todos los del usuario" sin depender de ningún filtro secundario).

## Capabilities

### New Capabilities
- `conceptos-filtrado-por-categoria-en-listado`: filtrado opcional por `categoriaId` en el listado paginado de Conceptos.

### Modified Capabilities
(ninguna — no existe spec previa que capture el comportamiento actual del listado paginado de Conceptos)

## Impact

- **`Kash.Api`**: `ConceptosController.GetPagedList` gana un parámetro `[FromQuery] string? categoriaId`.
- **`Kash.Application`**: `GetConceptosPagedListQuery` gana una propiedad `CategoriaId` opcional; `GetConceptosPagedListQueryHandler` (hoy 100% genérico, sin `ApplyFiltersAsync` propio) necesita aplicar ese filtro.
- **`Kash.Domain`/`Kash.Infrastructure`**: el método del kernel del que depende hoy el paginado (`IReadRepository.GetPagedReadModelsByUserAsync`) no acepta filtros extra (a diferencia de `GetRecentAsync`/`SearchForAutocompleteAsync`, confirmado por reflexión sobre `SergioIzq.Domain.Kernel`); se necesita un método propio en `ConceptoReadRepository` (Dapper), mismo patrón que los métodos custom ya existentes (`GetHabitualesAsync`, `GetUltimoUsoAsync` de la propuesta `sugerencias-y-habituales-transacciones`), sin tocar el paquete kernel compartido.
- **Consumidor externo**: `Kash-Frontend`, change `selectores-catalogo-completo` (repo separado, sin acoplamiento de código — solo de contrato HTTP).
