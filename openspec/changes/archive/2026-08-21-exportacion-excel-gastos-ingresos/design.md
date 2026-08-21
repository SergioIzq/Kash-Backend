## Context

Ver `proposal.md - Why` para la motivación (consumidor: `Kash-Frontend`, change `dialogo-exportar-excel-gastos-ingresos`). Puntos de partida verificados en este repo:

- `GastoReadRepository`/`IngresoReadRepository` (`Kash.Infrastructure/Persistence/Data/`) ya definen, vía `ReadRepositoryConfiguration.WithJoins(...)`, las columnas/joins/`searchableColumns` que usa hoy el listado paginado: alias `g`/`i`, joins a `conceptos`→`categorias`, `proveedores`/`clientes`, `personas`, `cuentas`, `formas_pago`; `searchableColumns` = descripción + nombre de concepto/categoría/proveedor-cliente/persona/cuenta. El filtro de texto de la exportación debe buscar en las mismas columnas para que el resultado sea coherente con lo que el usuario ve al escribir en el buscador del listado.
- El paginado genérico del kernel (`GetPagedReadModelsByUserAsync`) no acepta filtros extra — mismo motivo ya documentado en la propuesta archivada `conceptos-paginados-por-categoria` — así que un `WHERE` multi-filtro (fecha + varios conceptos + varias categorías + varios terceros + varias personas, todo combinable) tampoco puede montarse sobre ese método.
- Ya existe en este repo un patrón completo y más simple que el paginado genérico para "generar y descargar un Excel": `GetPresupuestoExcelQuery`/`GetPresupuestoExcelQueryHandler` (`Features/Reportes/Queries/GetPresupuestoExcel/`), que **no** hereda de la infraestructura de listado paginado — es un `IQuery<TResult>`/`IQueryHandler` normal (patrón CQRS del kernel de mensajería, no el de listados) que: valida la petición, pide los datos ya filtrados a un repositorio, genera el `.xlsx` con `IPresupuestoExcelGenerator` (ClosedXML) y devuelve `Result.Success(new PresupuestoArchivoDto(nombre, excel))`. `ReportesController` lo sirve con `File(result.Value.Contenido, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.Value.NombreArchivo)`. Este es el patrón a replicar, no el de `GetPagedListQueryHandler`.

## Goals / Non-Goals

**Goals:**
- Exportar a Excel el conjunto completo (sin paginar) de Gastos o Ingresos del usuario que cumple los filtros indicados.
- Soportar filtros combinables: rango de fechas, Concepto(s), Categoría(s), Proveedor(es)/Cliente(s), Persona(s), texto de búsqueda — todos opcionales, combinados en AND entre filtros distintos y en OR dentro de un mismo filtro multi-valor.
- Mantener el aislamiento por usuario ya garantizado en el resto de endpoints.

**Non-Goals:**
- No se toca el listado paginado existente (`GET /gastos`, `GET /ingresos`) ni su contrato.
- No se añade exportación a Excel para Inversiones ni para ningún otro catálogo (fuera de alcance, ver `proposal.md`).
- No se generaliza `GetPagedListQueryHandler` ni el kernel compartido con un hook de filtros — se resuelve de forma local a Gastos/Ingresos, mismo criterio que `conceptos-paginados-por-categoria`.
- No se pagina la respuesta ni se limita el número de filas del Excel (si el usuario no filtra, exporta todo su histórico).

## Decisions

**1. Se replica el patrón de `GetPresupuestoExcelQuery` (CQRS simple), no el de `GetPagedListQueryHandler`.**
`GetGastosExcelQuery(UsuarioId, FechaInicio?, FechaFin?, SearchTerm?, ConceptoIds?, CategoriaIds?, ProveedorIds?, PersonaIds?) : IQuery<GastoExcelArchivoDto>` (e `GetIngresosExcelQuery` equivalente, con `ClienteIds` en vez de `ProveedorIds`). El handler pide al repositorio nuevo la lista completa de `GastoDto`/`IngresoDto` que cumple los filtros, genera el Excel y devuelve `(NombreArchivo, Contenido)`, igual que `PresupuestoArchivoDto`.
*Alternativa descartada*: extender `GetGastosPagedListQuery`/su handler para que, con un parámetro `formato=excel`, devuelva Excel en vez de JSON paginado — mezclaría dos responsabilidades muy distintas (listar paginado vs. exportar completo) en el mismo endpoint y query, y complicaría el contrato HTTP existente sin necesidad.

**2. Nuevo repositorio Dapper por catálogo (`IGastoExportRepository`/`IIngresoExportRepository`), reutilizando las mismas columnas/joins que `GastoReadRepository`/`IngresoReadRepository`.**
Igual que `ConceptoPaginadoRepository` de la propuesta ya archivada: interfaz en `Kash.Application/Interfaces/`, implementación Dapper en `Kash.Infrastructure/Persistence/Query/`, registrada manualmente en DI (no vía el escaneo automático de Scrutor, que cubre las interfaces marcador de Domain). El método (`GetForExportAsync(usuarioId, filtro, ct)`) arma el `WHERE` dinámicamente: siempre `id_usuario = @UsuarioId`; añade `fecha BETWEEN @FechaInicio AND @FechaFin` si hay rango; `id_concepto IN @ConceptoIds` si hay conceptos; `id_categoria IN @CategoriaIds` (sobre la categoría del concepto, vía el join ya existente) si hay categorías; `id_proveedor IN @ProveedorIds` (`id_cliente IN @ClienteIds` en Ingresos) si hay terceros; `id_persona IN @PersonaIds` si hay personas; y una condición `OR` sobre las mismas `searchableColumns` que ya usa el listado paginado si hay `searchTerm`. Sin `LIMIT`/`OFFSET` ni consulta de conteo — devuelve todo lo que matchea.
*Alternativa descartada*: construir el filtro con Entity Framework (LINQ dinámico) en vez de Dapper — el resto de repositorios de lectura de este proyecto ya usan Dapper de forma consistente (rendimiento, control total del SQL); introducir EF aquí rompería ese patrón sin aportar nada.

**3. `GastoExcelGenerator`/`IngresoExcelGenerator` (ClosedXML), mismo patrón visual que `PresupuestoExcelGenerator`.**
Una sola hoja con cabecera de columnas (Fecha, Concepto, Categoría, Proveedor/Cliente, Persona, Cuenta, Forma de Pago, Importe, Descripción), formato de fecha/moneda igual que el generador de Presupuesto (`dd/mm/yyyy`, `#,##0.00 €`), fila de cabecera con estilo. No hace falta ninguna hoja de resumen/KPIs (a diferencia de Presupuesto) porque esto es una exportación de detalle, no un informe agregado.

**4. Los filtros de catálogo (`ConceptoIds`, `CategoriaIds`, `ProveedorIds`/`ClienteIds`, `PersonaIds`) se validan solo por pertenencia implícita al `WHERE id_usuario`, no con una consulta previa de existencia.**
Si el frontend envía un id de catálogo que no pertenece al usuario (o no existe), el `WHERE id_usuario = @UsuarioId AND id_concepto IN (...)` simplemente no encuentra filas para ese filtro — mismo comportamiento (y mismo riesgo aceptado) que ya se documentó y verificó para `categoriaId` en Conceptos. No se añade una validación de existencia adicional que ya cubre implícitamente el filtro por usuario.
*Alternativa descartada*: devolver un error 400 si algún id de catálogo no existe o no pertenece al usuario — más estricto, pero requeriría una consulta extra por cada catálogo filtrado solo para dar un mensaje de error más específico ante un caso (ids inválidos enviados por el propio cliente) que ya se comporta de forma segura sin esa validación.

## Risks / Trade-offs

- **[Riesgo] Exportaciones sin ningún filtro sobre un historial muy grande pueden generar un Excel pesado y tardar.** Aceptado por ahora: no hay límite de filas en el alcance de este cambio (ver Non-Goals); si en producción resulta un problema real, se puede añadir un límite o una exportación asíncrona en un cambio posterior.
- **[Riesgo] `WHERE ... IN @Lista` con listas vacías.** Mitigación: el repositorio solo añade la condición `IN` al `WHERE` cuando la lista de ids correspondiente tiene al menos un elemento; una lista vacía o no informada no añade ninguna restricción para ese filtro (equivale a "no filtrar por esto"), evitando el error de SQL de un `IN ()` vacío.
- **[Riesgo] Dos repositorios de exportación casi idénticos (Gastos/Ingresos) en vez de uno genérico.** Aceptado: mismo criterio que el resto del código de Kash (`GastoReadRepository`/`IngresoReadRepository` ya están duplicados por catálogo); generalizar ahora sería especular con una abstracción que no se ha necesitado hasta hoy.

## Migration Plan

No aplica migración de datos. Cambio aditivo: dos endpoints nuevos (`GET /gastos/excel`, `GET /ingresos/excel`); no se modifica ningún endpoint existente. Sin rollback especial más allá de revertir el despliegue si hiciera falta.
