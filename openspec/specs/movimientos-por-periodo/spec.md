# movimientos-por-periodo Specification

## Purpose

Permitir consultar, de forma paginada, los Gastos o los Ingresos del usuario autenticado cuya fecha de transacción cae dentro de un rango indicado, para alimentar vistas de "movimientos rápidos" filtrables por periodo (hoy, esta semana, este mes o un rango personalizado).

## Requirements

### Requirement: Listado paginado de Gastos o Ingresos por rango de fechas
El sistema SHALL exponer, para Gastos y para Ingresos, una operación que devuelva los registros del usuario autenticado cuya `Fecha` (la fecha de la transacción introducida por el usuario, no ninguna fecha de creación/auditoría del registro) esté dentro de un rango `[fechaInicio, fechaFin]` indicado, con ambos extremos incluidos, envueltos en el mismo formato de resultado paginado que ya usa el listado general (página, tamaño de página, total de registros, indicadores de página siguiente/anterior).

#### Scenario: Rango de fechas con resultados
- **WHEN** el usuario autenticado solicita sus Gastos (o Ingresos) indicando `fechaInicio` y `fechaFin`
- **THEN** el sistema devuelve, paginados, únicamente los registros del usuario cuya `Fecha` está entre `fechaInicio` y `fechaFin` (ambos incluidos)

#### Scenario: Filtrado por la fecha de la transacción, no por fecha de creación
- **WHEN** un registro fue creado o modificado en una fecha distinta a su `Fecha` de transacción
- **THEN** el sistema decide su inclusión en el resultado exclusivamente en función de su `Fecha` de transacción, ignorando cuándo se creó o modificó el registro

#### Scenario: Rango de fechas sin resultados
- **WHEN** el rango de fechas indicado no coincide con ninguna `Fecha` de transacción del usuario
- **THEN** el sistema devuelve un resultado paginado vacío (sin registros), sin error

### Requirement: Paginación configurable del listado por periodo
El sistema SHALL aceptar parámetros opcionales de página y tamaño de página en la consulta por periodo, con los mismos valores por defecto que el listado paginado general, permitiendo solicitar tamaños de página mayores (p. ej. para obtener en una sola página todos los movimientos de un periodo acotado).

#### Scenario: Tamaño de página amplio para cubrir todo el periodo
- **WHEN** el usuario solicita un tamaño de página suficientemente grande como para que quepan todos los registros del rango indicado
- **THEN** el sistema devuelve todos esos registros en una única página, con el total de registros correctamente informado

### Requirement: Validación del rango de fechas
El sistema SHALL rechazar con un error de validación las solicitudes en las que `fechaInicio` sea posterior a `fechaFin`.

#### Scenario: Fecha de inicio posterior a la fecha de fin
- **WHEN** el usuario solicita el listado por periodo con `fechaInicio` posterior a `fechaFin`
- **THEN** el sistema devuelve un error de validación y no ejecuta la consulta

### Requirement: Aislamiento entre usuarios en el listado por periodo
El sistema SHALL restringir siempre el listado por periodo a los Gastos o Ingresos del usuario autenticado, sin exponer registros de otros usuarios.

#### Scenario: Usuario autenticado sin registros propios en el periodo
- **WHEN** existen Gastos (o Ingresos) de otros usuarios cuya `Fecha` cae dentro del rango solicitado, pero el usuario autenticado no tiene ninguno propio en ese rango
- **THEN** el sistema devuelve un resultado paginado vacío, sin incluir registros de otros usuarios
