# transacciones-habituales Specification

## Purpose

Exponer las combinaciones completas de gasto/ingreso (concepto, categoría, cuenta, forma de pago y tercero) que un usuario registra con más frecuencia, para que un cliente pueda ofrecerlas como accesos rápidos de un solo toque.

## Requirements

### Requirement: Consulta de gastos habituales
El sistema SHALL exponer un endpoint que devuelva, para el usuario autenticado, un listado de hasta `limit` combinaciones completas de gasto (concepto, categoría, cuenta, forma de pago, proveedor, persona) ordenadas por número de veces registradas de forma descendente, usando la fecha de uso más reciente como criterio de desempate.

#### Scenario: Usuario con combinaciones repetidas
- **WHEN** un usuario autenticado tiene varios gastos con la misma combinación de concepto/categoría/cuenta/forma de pago/proveedor/persona
- **THEN** el sistema incluye esa combinación en el resultado junto con el número de veces que se ha registrado, ordenada según su frecuencia relativa al resto de combinaciones devueltas

#### Scenario: Usuario sin combinaciones repetidas
- **WHEN** un usuario autenticado no tiene ninguna combinación de gasto que se repita (todas sus combinaciones son distintas entre sí)
- **THEN** el sistema devuelve una lista vacía, sin error

#### Scenario: Límite de resultados
- **WHEN** un usuario autenticado solicita el listado de gastos habituales indicando un `limit`
- **THEN** el sistema devuelve como máximo esa cantidad de combinaciones, priorizando las de mayor frecuencia

### Requirement: Consulta de ingresos habituales
El sistema SHALL exponer el endpoint equivalente para ingresos, con el mismo comportamiento que `Requirement: Consulta de gastos habituales`, agrupando por concepto, categoría, cuenta, forma de pago, cliente y persona.

#### Scenario: Usuario con combinaciones de ingreso repetidas
- **WHEN** un usuario autenticado tiene varios ingresos con la misma combinación de concepto/categoría/cuenta/forma de pago/cliente/persona
- **THEN** el sistema incluye esa combinación en el resultado junto con el número de veces registrada, con el mismo criterio de orden y desempate que gastos habituales

### Requirement: Aislamiento entre usuarios
El sistema SHALL calcular las combinaciones habituales únicamente a partir de las transacciones del usuario autenticado, sin incluir combinaciones de otros usuarios aunque coincidan en concepto, categoría, cuenta o forma de pago.

#### Scenario: Coincidencia de nombres entre usuarios
- **WHEN** dos usuarios distintos tienen conceptos con el mismo nombre y combinaciones similares
- **THEN** las combinaciones habituales devueltas a cada usuario reflejan únicamente su propio histórico
