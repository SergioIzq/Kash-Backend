## Purpose

Permitir filtrar por Categoría el listado paginado de Conceptos, para que un cliente pueda recorrer con scroll el catálogo completo de Conceptos de una Categoría concreta, en vez de limitarse a los resultados de búsqueda por texto o a los más recientes.

## ADDED Requirements

### Requirement: Filtro opcional por categoría en el listado paginado de Conceptos
El sistema SHALL aceptar un parámetro opcional `categoriaId` en el listado paginado de Conceptos que, cuando se informa, restringe los Conceptos devueltos a los que pertenecen a esa Categoría, manteniendo la paginación, el orden y la búsqueda por texto ya existentes.

#### Scenario: Listado filtrado por categoría
- **WHEN** un usuario autenticado solicita el listado paginado de Conceptos indicando un `categoriaId`
- **THEN** el sistema devuelve únicamente los Conceptos de ese usuario que pertenecen a esa Categoría, paginados según lo solicitado

#### Scenario: Listado sin filtro de categoría
- **WHEN** un usuario autenticado solicita el listado paginado de Conceptos sin indicar `categoriaId`
- **THEN** el sistema devuelve todos los Conceptos de ese usuario, exactamente igual que antes de este cambio

#### Scenario: Categoría sin conceptos
- **WHEN** un usuario autenticado solicita el listado paginado de Conceptos filtrado por una Categoría que no tiene ningún Concepto asociado
- **THEN** el sistema devuelve una página vacía, sin error

### Requirement: Aislamiento entre usuarios
El sistema SHALL aplicar el filtro por categoría únicamente sobre los Conceptos del usuario autenticado, sin exponer Conceptos de otros usuarios aunque coincida el `categoriaId`.

#### Scenario: categoriaId de otro usuario
- **WHEN** un usuario autenticado solicita el listado paginado de Conceptos filtrado por un `categoriaId` que pertenece a otro usuario
- **THEN** el sistema no devuelve Conceptos de ese otro usuario; se comporta igual que si la categoría no tuviera Conceptos propios
