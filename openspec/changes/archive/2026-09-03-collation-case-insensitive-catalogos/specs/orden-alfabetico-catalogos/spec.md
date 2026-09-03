## Purpose

Garantizar que los catálogos de nombre libre (categorías, conceptos, cuentas, formas de pago, proveedores, clientes, personas) se listen, busquen y previsualicen en orden alfabético case-insensitive, tal y como lo esperaría una persona, en vez de por el valor de byte del nombre.

## ADDED Requirements

### Requirement: Orden alfabético case-insensitive de catálogos por nombre
Cuando el sistema devuelve elementos de un catálogo (categorías, conceptos, cuentas, formas de pago, proveedores, clientes o personas) ordenados por nombre, el orden SHALL ser alfabético case-insensitive y accent-insensitive: dos nombres que solo difieren en mayúsculas/minúsculas o en acentos SHALL intercalarse según su forma base, no según el valor de byte de sus caracteres.

#### Scenario: Listado paginado de un catálogo con nombres de distinta capitalización
- **WHEN** un usuario tiene categorías `IA`, `Ocio`, `Sin clasificar` y `categoria`, y solicita el listado paginado de categorías ordenado por nombre ascendente
- **THEN** el orden devuelto es `categoria`, `IA`, `Ocio`, `Sin clasificar`

#### Scenario: Vista previa "recientes" de un catálogo con nombres de distinta capitalización
- **WHEN** un usuario abre el selector de categoría en el formulario de crear/editar Gasto o Ingreso, que precarga el catálogo completo vía el endpoint de "recientes"
- **THEN** las categorías se presentan en el mismo orden alfabético case-insensitive que el listado paginado, no por fecha de creación ni por valor de byte del nombre

#### Scenario: Búsqueda por texto en un catálogo con nombres de distinta capitalización
- **WHEN** un usuario busca un texto que coincide con varios nombres de catálogo que difieren solo en mayúsculas/minúsculas
- **THEN** los resultados se devuelven en orden alfabético case-insensitive
