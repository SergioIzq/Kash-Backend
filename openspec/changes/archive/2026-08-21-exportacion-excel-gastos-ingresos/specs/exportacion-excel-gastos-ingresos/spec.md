## Purpose

Permitir exportar a un libro Excel el listado de Gastos o de Ingresos del usuario autenticado, con filtros combinables por fecha, concepto, categoría, proveedor/cliente, persona y búsqueda de texto, devolviendo siempre el conjunto completo de resultados que cumple los filtros (sin paginar).

## ADDED Requirements

### Requirement: Exportación a Excel del listado completo de Gastos o Ingresos
El sistema SHALL permitir al usuario autenticado descargar un libro Excel (.xlsx) con sus Gastos, o alternativamente con sus Ingresos, incluyendo Fecha, Concepto, Categoría, Proveedor o Cliente (según corresponda), Persona, Cuenta, Forma de Pago, Importe y Descripción de cada registro.

#### Scenario: Exportar sin ningún filtro
- **WHEN** el usuario autenticado solicita la exportación a Excel de sus Gastos (o Ingresos) sin indicar ningún filtro
- **THEN** el sistema devuelve un libro Excel con todos los Gastos (o Ingresos) del usuario, sin límite de página

#### Scenario: El resultado no está paginado
- **WHEN** el usuario autenticado solicita la exportación y el conjunto de resultados es mayor que el tamaño de página usado en el listado habitual
- **THEN** el sistema incluye en el Excel la totalidad de los resultados que cumplen los filtros, no solo una página

### Requirement: Filtrado combinable de la exportación
El sistema SHALL aceptar, en la exportación a Excel, filtros opcionales por rango de fechas, uno o varios Conceptos, una o varias Categorías, uno o varios Proveedores (Gastos) o Clientes (Ingresos), una o varias Personas, y un texto de búsqueda equivalente al usado en el listado paginado. Los filtros informados SHALL combinarse entre sí en conjunción (deben cumplirse todos los filtros activos); dentro de un mismo filtro con varios valores (p. ej. varios Conceptos), SHALL bastar con que se cumpla al menos uno de esos valores.

#### Scenario: Filtro por rango de fechas
- **WHEN** el usuario solicita la exportación indicando una fecha de inicio y una fecha de fin
- **THEN** el sistema incluye únicamente los registros cuya fecha está dentro de ese rango (ambos extremos incluidos)

#### Scenario: Filtro con varios valores del mismo tipo
- **WHEN** el usuario solicita la exportación indicando más de un Concepto (o más de una Categoría, Proveedor/Cliente, o Persona)
- **THEN** el sistema incluye los registros que coinciden con cualquiera de los valores indicados para ese filtro

#### Scenario: Combinación de filtros de distinto tipo
- **WHEN** el usuario solicita la exportación indicando a la vez, por ejemplo, un rango de fechas y una o varias Categorías
- **THEN** el sistema incluye únicamente los registros que cumplen el rango de fechas Y pertenecen a alguna de las Categorías indicadas

#### Scenario: Filtro por búsqueda de texto
- **WHEN** el usuario solicita la exportación indicando un texto de búsqueda
- **THEN** el sistema incluye únicamente los registros que ese mismo texto encontraría en el listado paginado habitual

#### Scenario: Filtros sin resultados
- **WHEN** los filtros indicados no coinciden con ningún registro del usuario
- **THEN** el sistema devuelve un libro Excel válido sin filas de datos, sin error

### Requirement: Aislamiento entre usuarios en la exportación
El sistema SHALL restringir siempre la exportación a los Gastos o Ingresos del usuario autenticado, sin exponer registros de otros usuarios aunque los filtros (p. ej. un `conceptoId` o `categoriaId`) coincidan con catálogo de otro usuario.

#### Scenario: Filtro con un identificador de catálogo de otro usuario
- **WHEN** el usuario autenticado solicita la exportación filtrando por un Concepto, Categoría, Proveedor/Cliente o Persona que pertenece a otro usuario
- **THEN** el sistema no incluye registros de ese otro usuario; se comporta igual que si ese filtro no tuviera ningún resultado propio
