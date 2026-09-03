## ADDED Requirements

### Requirement: Sumatorio de importes del periodo
El sistema SHALL devolver, junto al listado paginado de Gastos o Ingresos por periodo, la suma del `Importe` de todos los registros del usuario autenticado cuya `Fecha` está dentro del rango `[fechaInicio, fechaFin]` solicitado, calculada sobre el total de registros que cumplen el filtro y no solo sobre los de la página actual.

#### Scenario: Sumatorio independiente de la página o tamaño de página solicitados
- **WHEN** el usuario solicita sus Gastos (o Ingresos) por periodo con cualquier combinación de `page` y `pageSize`
- **THEN** el sumatorio devuelto es la suma del `Importe` de todos los registros del rango de fechas que cumplen el filtro, sin importar cuántos de ellos caben en la página solicitada

#### Scenario: Sumatorio de un rango sin resultados
- **WHEN** el rango de fechas indicado no coincide con ninguna `Fecha` de transacción del usuario
- **THEN** el sistema devuelve un sumatorio de cero, junto con el resultado paginado vacío

#### Scenario: Aislamiento entre usuarios en el sumatorio
- **WHEN** existen Gastos (o Ingresos) de otros usuarios cuya `Fecha` cae dentro del rango solicitado, pero el usuario autenticado no tiene ninguno propio en ese rango
- **THEN** el sumatorio devuelto es cero, sin incluir importes de otros usuarios
