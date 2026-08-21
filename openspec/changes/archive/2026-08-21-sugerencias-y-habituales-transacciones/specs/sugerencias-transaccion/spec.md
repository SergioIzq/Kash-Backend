## Purpose

Exponer, para un concepto dado, la combinación de campos (cuenta, forma de pago, importe y tercero) de la transacción más reciente registrada por el usuario para ese concepto, de modo que un cliente pueda pre-rellenar un formulario de alta sin que el usuario tenga que volver a introducir esos datos.

## ADDED Requirements

### Requirement: Consulta de sugerencia de gasto por concepto
El sistema SHALL exponer un endpoint que, dado un `conceptoId` y el usuario autenticado, devuelva los datos del gasto más reciente registrado por ese usuario para ese concepto (cuenta, forma de pago, importe, proveedor, persona, categoría), o una respuesta vacía si no existe ningún gasto previo con ese concepto.

#### Scenario: Concepto con gastos previos
- **WHEN** un usuario autenticado solicita la sugerencia para un `conceptoId` que ya tiene al menos un gasto registrado por ese usuario
- **THEN** el sistema devuelve los datos (cuenta, forma de pago, importe, proveedor, persona, categoría) del gasto más reciente de ese usuario para ese concepto

#### Scenario: Concepto sin gastos previos
- **WHEN** un usuario autenticado solicita la sugerencia para un `conceptoId` que no tiene ningún gasto registrado por ese usuario
- **THEN** el sistema devuelve una respuesta vacía, sin error, indicando que no hay sugerencia disponible

#### Scenario: Aislamiento entre usuarios
- **WHEN** un usuario autenticado solicita la sugerencia para un `conceptoId` que sí tiene gastos registrados, pero pertenecientes a otro usuario
- **THEN** el sistema no devuelve datos de gastos de otros usuarios; se comporta igual que si no hubiera histórico

#### Scenario: Varios gastos del mismo concepto en la misma fecha
- **WHEN** un usuario tiene registrados varios gastos con el mismo `conceptoId` y la misma fecha de transacción
- **THEN** el sistema devuelve el gasto creado más recientemente entre ellos (desempate por momento de creación, no por un criterio arbitrario u orden no determinista)

### Requirement: La sugerencia refleja inmediatamente el histórico más reciente
El sistema SHALL calcular la sugerencia a partir del estado actual de los datos en cada solicitud, sin devolver un resultado obsoleto de una transacción creada previamente.

#### Scenario: Sugerencia justo después de crear una transacción
- **WHEN** un usuario crea un gasto (o ingreso) para un concepto y a continuación, en una nueva alta, selecciona ese mismo concepto
- **THEN** la sugerencia devuelta refleja los datos de la transacción que se acaba de crear, sin ningún retraso perceptible por almacenamiento en caché

### Requirement: Consulta de sugerencia de ingreso por concepto
El sistema SHALL exponer el endpoint equivalente para ingresos, con el mismo comportamiento que la sugerencia de gasto mostrado en `Requirement: Consulta de sugerencia de gasto por concepto`, devolviendo cuenta, forma de pago, importe, cliente y persona del ingreso más reciente para ese concepto.

#### Scenario: Concepto con ingresos previos
- **WHEN** un usuario autenticado solicita la sugerencia de ingreso para un `conceptoId` que ya tiene al menos un ingreso registrado por ese usuario
- **THEN** el sistema devuelve los datos (cuenta, forma de pago, importe, cliente, persona, categoría) del ingreso más reciente de ese usuario para ese concepto

#### Scenario: Varios ingresos del mismo concepto en la misma fecha
- **WHEN** un usuario tiene registrados varios ingresos con el mismo `conceptoId` y la misma fecha de transacción
- **THEN** el sistema devuelve el ingreso creado más recientemente entre ellos (mismo criterio de desempate que gastos)

### Requirement: Autenticación requerida
El sistema SHALL requerir un usuario autenticado para consultar la sugerencia de gasto o de ingreso, y SHALL calcular la sugerencia únicamente sobre los datos de ese usuario.

#### Scenario: Solicitud sin autenticar
- **WHEN** se solicita cualquiera de los dos endpoints de sugerencia sin credenciales válidas
- **THEN** el sistema rechaza la solicitud sin devolver datos de ningún usuario
