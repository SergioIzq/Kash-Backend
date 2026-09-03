## Why

Las columnas `nombre` de los catálogos (categorías, conceptos, cuentas, formas de pago, proveedores, clientes, personas) usan una collation de MySQL sensible a mayúsculas/minúsculas. Todo el código que pide `ORDER BY nombre ASC` (listados paginados, autocompletado de búsqueda y el endpoint "recent" recién corregido para presentar en orden alfabético) ya hace lo correcto, pero MySQL ordena por valor de byte: los nombres que empiezan en mayúscula quedan siempre antes que los que empiezan en minúscula, en vez de mezclarse en el orden alfabético que un usuario espera. Verificado en vivo: con categorías `IA`, `Ocio`, `Sin clasificar`, `categoria`, tanto el listado de Categorías como el desplegable de Categoría en los modales de crear/editar Gasto/Ingreso muestran ese mismo orden — `categoria` queda al final en vez de ir primero.

## What Changes

- Se cambia la collation de la columna `nombre` (`VARCHAR(100)`) a `utf8mb4_0900_ai_ci` (case-insensitive y accent-insensitive, soportada por el MySQL 8.0.43 en uso) en las 7 tablas de catálogo: `categorias`, `conceptos`, `cuentas`, `formas_pago`, `proveedores`, `clientes`, `personas`.
- Se aplica mediante un script `schema.sql` (este repo no usa EF Core Migrations; los cambios de esquema se documentan y aplican a mano, como en `openspec/changes/archive/2026-08-24-token-personal-api/schema.sql`).
- Ningún cambio de código: `AbsReadRepository` (kernel) ya construye correctamente `ORDER BY nombre ASC` en los listados paginados, en `SearchForAutocompleteAsync` y en `GetRecentAsync`; el cambio de collation hace que ese `ASC` se comporte de forma alfabética case-insensitive sin tocar la consulta.
- **Efecto colateral (deseado, no un objetivo en sí):** las comprobaciones de duplicados por nombre (`ExistsWithSameNameAsync` / `ExistsWithSameNameExceptAsync` en cada `*ReadRepository`, usadas al crear/renombrar un elemento de catálogo) pasan a considerar iguales dos nombres que solo difieren en mayúsculas/minúsculas o acentos (p. ej. "Ocio" y "ocio"). No hay índice `UNIQUE` a nivel de base de datos sobre `nombre` en ninguna de las 7 tablas (solo índices no únicos `(usuario, nombre)` para rendimiento), así que la migración no puede fallar por colisión de unicidad — pero si ya existen registros duplicados solo por mayúsculas/minúsculas, ambos seguirán existiendo (la collation no fusiona filas), y a partir de ahí la app empezará a tratarlos como el mismo nombre al validar duplicados.

## Capabilities

### New Capabilities
- `orden-alfabetico-catalogos`: orden alfabético case-insensitive al listar, buscar o previsualizar los catálogos por nombre (categorías, conceptos, cuentas, formas de pago, proveedores, clientes, personas).

### Modified Capabilities
(ninguna — no hay spec existente que documente el orden de estos catálogos)

## Impact

- **Esquema de base de datos**: `schema.sql` de este change, con `ALTER TABLE ... MODIFY nombre VARCHAR(100) COLLATE utf8mb4_0900_ai_ci` para `categorias`, `conceptos`, `cuentas`, `formas_pago`, `proveedores`, `clientes`, `personas`. Aplicar manualmente sobre la base de datos (dev y producción); no requiere despliegue de código.
- **Código**: ninguno. No se toca `Kash.Infrastructure`, `Kash.Application` ni el paquete `SergioIzq.Infrastructure.Kernel`.
- **Consumidores**: `Kash-Frontend` no requiere cambios — ya pide orden alfabético explícito en el catálogo paginado y consume tal cual el orden de `/recent` y `/search`; con la collation corregida, los desplegables de crear/editar Gasto/Ingreso (y las pantallas de gestión de cada catálogo) mostrarán orden alfabético humano sin más cambios.
