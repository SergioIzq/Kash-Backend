## 1. Contrato de la query

- [x] 1.1 Añadida propiedad `CategoriaId` opcional (`string?`, mismo tipo que ya usan `SearchConceptosQuery`/`GetRecentConceptosQuery`) a `GetConceptosPagedListQuery`
- [x] 1.2 Añadido parámetro `[FromQuery] string? categoriaId` a `ConceptosController.GetPagedList`, pasado a la query

## 2. Repositorio de agregación (capability: conceptos-filtrado-por-categoria-en-listado)

- [x] 2.1 Creada `IConceptoPaginadoRepository` en `Kash.Application/Interfaces/` con `GetPagedByCategoriaAsync(Guid usuarioId, Guid categoriaId, int page, int pageSize, string? searchTerm, string? sortColumn, string? sortOrder, CancellationToken ct)`, mismo patrón que `IGastoHabitualesRepository`
- [x] 2.2 Implementado `ConceptoPaginadoRepository` en `Kash.Infrastructure/Persistence/Query/` con Dapper: mismas columnas/joins que `ConceptoReadRepository.ConfigureRepository()` (alias `c`), `WHERE c.id_usuario = @UsuarioId AND c.id_categoria = @CategoriaId`, `searchTerm` opcional sobre `c.nombre` (`LIKE`), whitelist de columnas sortables (`Nombre`/`CategoriaNombre`/`FechaCreacion`, igual que `ConfigureRepository()`) para no exponer el `ORDER BY` a la query string, `LIMIT/OFFSET` según `page`/`pageSize`, más una consulta de conteo total; devuelve `PagedList<ConceptoDto>` (confirmado por reflexión sobre `SergioIzq.Domain.Kernel` que su constructor es `(List<T> items, int page, int pageSize, int totalCount)`); registrado en DI (`Kash.Infrastructure/DependencyInjection.cs`)
- [x] 2.3 Verificado contra `AhorroLandTest` (BD de test) con el usuario real de prueba, que tiene un Concepto en cada una de sus 4 Categorías (IA/Claude, Ocio/Gimnasio, Sin clasificar/Importado del banco, categoria/concepto): `GET /api/conceptos?categoriaId=...` para cada una de las 4 Categorías devuelve únicamente su propio Concepto (`allMatchCategoria: true` en los 4 casos), sin mezclar Conceptos de otras Categorías; paginación (`page`/`pageSize`/`hasNextPage`) correcta; una Categoría sin Conceptos (GUID inexistente) devuelve página vacía sin error

## 3. Handler

- [x] 3.1 Sobrescrito `ApplyFiltersAsync` en `GetConceptosPagedListQueryHandler` (archivo `GetClientesPagedListQueryHandler.cs`, nombre ya mal puesto de antes, no corregido aquí): si `query.CategoriaId` es nulo/vacío, mantiene el comportamiento actual (`_dtoRepository.GetPagedReadModelsByUserAsync(...)`, mismo patrón que `GetGastosPagedListQueryHandler`); si viene informado, llama a `IConceptoPaginadoRepository.GetPagedByCategoriaAsync(...)` parseando `query.CategoriaId` a `Guid`
- [x] 3.2 Verificado contra `AhorroLandTest`: `GET /api/conceptos?page=1&pageSize=50` sin `categoriaId` sigue devolviendo los 4 Conceptos del usuario (mismo `totalCount` y mismos ítems que antes de este cambio, camino genérico `_dtoRepository.GetPagedReadModelsByUserAsync` sin tocar)

## 4. Aislamiento y validación final

- [ ] 4.1 Verificar manualmente que un `categoriaId` perteneciente a otro usuario no devuelve Conceptos de ese otro usuario (el filtro por `id_usuario` sigue aplicándose siempre, no solo cuando no hay `categoriaId`)
- [x] 4.2 `dotnet build` de `Kash.Application`/`Kash.Infrastructure` (aislados) y `dotnet build Kash.sln` completo: compilación sin errores ni avisos
- [x] 4.3 Confirmado con `Kash-Frontend` (change `selectores-catalogo-completo`, `concepto.service.ts`): el parámetro `categoriaId` y la forma de la respuesta (`items`/`page`/`pageSize`/`totalCount`/`hasNextPage`/`hasPreviousPage`, `PaginatedList<T>` del frontend) coinciden con `PagedList<ConceptoDto>` del backend
- [x] 4.4 `openspec validate conceptos-paginados-por-categoria --strict`: "Change 'conceptos-paginados-por-categoria' is valid"
