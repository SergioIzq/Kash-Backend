## Why

El botón "Exportar" de Gastos e Ingresos genera hoy un CSV en el propio navegador a partir de los datos ya cargados en la tabla — como las tablas son paginadas en servidor (10-30 filas), en la práctica solo se exporta la página visible, nunca el histórico completo, y no hay forma de acotar la exportación por fecha, concepto, categoría o tercero. El frontend (`Kash-Frontend`, change `dialogo-exportar-excel-gastos-ingresos`) quiere ofrecer un diálogo de exportación con filtros combinables (rango de fechas, concepto, categoría, proveedor/cliente, persona, o la búsqueda actual de la tabla) que descargue un Excel real generado en el backend con el conjunto completo de resultados que cumpla esos filtros.

## What Changes

- Nuevos endpoints `GET /api/gastos/excel` y `GET /api/ingresos/excel` que aceptan filtros opcionales y combinables: `searchTerm`, `fechaInicio`/`fechaFin`, `conceptoIds` (lista), `categoriaIds` (lista), `proveedorIds`/`clienteIds` (lista), `personaIds` (lista). Los filtros se combinan en AND entre sí; dentro de una misma lista (p. ej. varios `conceptoIds`), en OR. Sin ningún filtro informado, exporta todo el histórico del usuario.
- Los resultados no se paginan: se exporta el conjunto completo que cumple los filtros, no una página.
- El archivo devuelto es un libro Excel (.xlsx) real (no CSV), con más columnas que el CSV actual: Fecha, Concepto, Categoría, Proveedor/Cliente, Persona, Cuenta, Forma de Pago, Importe y Descripción.
- Reutiliza el patrón ya existente en `Kash-Backend` para generar Excel (`PresupuestoExcelGenerator` con ClosedXML, servido como `File(...)` desde un controller).

## Capabilities

### New Capabilities
- `exportacion-excel-gastos-ingresos`: exportación a Excel de Gastos e Ingresos con filtrado combinable por fecha, concepto, categoría, proveedor/cliente, persona y búsqueda de texto.

### Modified Capabilities
(ninguna — no hay spec previa que capture el comportamiento de exportación de Gastos/Ingresos)

## Impact

- **`Kash.Api`**: `GastosController` gana `GET /gastos/excel`; `IngresosController` gana `GET /ingresos/excel`.
- **`Kash.Application`**: nuevas queries `GetGastosExcelQuery`/`GetIngresosExcelQuery` (sin paginación) con sus handlers; nuevas interfaces `IGastoExportRepository`/`IIngresoExportRepository` en `Interfaces/` (mismo motivo que `IConceptoPaginadoRepository`: el paginado genérico del kernel no admite un `WHERE` multi-filtro).
- **`Kash.Infrastructure`**: nuevos repositorios Dapper `GastoExportRepository`/`IngresoExportRepository` (`Persistence/Query/`) que arman el `WHERE` dinámico según los filtros informados; nuevos `GastoExcelGenerator`/`IngresoExcelGenerator` (`Reporting/`, ClosedXML, mismo patrón que `PresupuestoExcelGenerator`); registro en DI.
- **Consumidor externo**: `Kash-Frontend`, change `dialogo-exportar-excel-gastos-ingresos` (repo separado, sin acoplamiento de código — solo de contrato HTTP).
- Sin cambios breaking: el botón "Exportar" actual (CSV client-side) se sustituye en el frontend, pero eso es alcance de ese otro change; este cambio solo añade endpoints nuevos.
