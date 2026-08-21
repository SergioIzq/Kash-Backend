## 1. Contrato de las queries y endpoints

- [x] 1.1 Creada `GetGastosExcelQuery(Guid UsuarioId, DateTime? FechaInicio, DateTime? FechaFin, string? SearchTerm, Guid[]? ConceptoIds, Guid[]? CategoriaIds, Guid[]? ProveedorIds, Guid[]? PersonaIds) : IQuery<PresupuestoArchivoDto>`, mismo patrón que `GetPresupuestoExcelQuery`
- [x] 1.2 Creada `GetIngresosExcelQuery` equivalente, con `Guid[]? ClienteIds` en vez de `ProveedorIds`
- [x] 1.3 **[Reutilizado]** `PresupuestoArchivoDto` (`NombreArchivo`, `Contenido`) ya tiene la forma exacta necesaria — no se crea un DTO nuevo, ambas queries devuelven ese mismo tipo
- [x] 1.4 Añadido `GET /gastos/excel` a `GastosController` con `[FromQuery] DateTime? fechaInicio, DateTime? fechaFin, string? searchTerm, Guid[]? conceptoIds, Guid[]? categoriaIds, Guid[]? proveedorIds, Guid[]? personaIds`, devolviendo `File(...)` con mime `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`
- [x] 1.5 Añadido `GET /ingresos/excel` a `IngresosController` equivalente, con `clienteIds` en vez de `proveedorIds`

## 2. Repositorios de exportación (sin paginar, WHERE dinámico)

- [x] 2.1 Creada `IGastoExportRepository` en `Kash.Application/Interfaces/` (junto con el record `GastoExportFiltro`) con `GetForExportAsync(Guid usuarioId, GastoExportFiltro filtro, CancellationToken ct)` devolviendo `IReadOnlyList<GastoDto>`
- [x] 2.2 Implementado `GastoExportRepository` en `Kash.Infrastructure/Persistence/Query/` con Dapper: mismas columnas/joins que `GastoReadRepository.ConfigureRepository()`, `WHERE g.id_usuario = @UsuarioId` siempre, más `AND g.fecha BETWEEN @FechaInicio AND @FechaFin` si hay rango, `AND g.id_concepto IN @ConceptoIds` si la lista no está vacía, `AND c.id_categoria IN @CategoriaIds` si no está vacía, `AND g.id_proveedor IN @ProveedorIds` si no está vacía, `AND g.id_persona IN @PersonaIds` si no está vacía, y `AND (searchable OR ...)` sobre las mismas `searchableColumns` que `GastoReadRepository` si hay `searchTerm`; sin `LIMIT`/`OFFSET`
- [x] 2.3 Creadas `IIngresoExportRepository`/`IngresoExportRepository` equivalentes, con `id_cliente` en vez de `id_proveedor`, mismas columnas/joins que `IngresoReadRepository.ConfigureRepository()`
- [x] 2.4 Registrados ambos repositorios en DI (`Kash.Infrastructure/DependencyInjection.cs`)
- [x] 2.5 Verificado contra `AhorroLandTest` (BD de test) con un usuario real: filtro por concepto, categoría, proveedor, cliente, rango de fechas y texto de búsqueda cada uno por separado, y una combinación concepto+proveedor, comparando siempre contra un `COUNT(*)` SQL directo independiente. Todos coinciden; el único resultado inicialmente sorprendente (búsqueda por texto "Peluquería" trayendo un Gasto de concepto "VPS") resultó ser correcto: ese Gasto tiene como Proveedor "Peluquería Kbello", y la búsqueda coincide en `prov.nombre` igual que hace el listado paginado — confirma que el `OR` entre `searchableColumns` funciona como se diseñó, no un fallo

## 3. Generadores de Excel (ClosedXML)

- [x] 3.1 Creadas `IGastoExcelGenerator`/`IIngresoExcelGenerator` en `Kash.Application/Interfaces/` con `byte[] Generar(IReadOnlyList<GastoDto> datos)` (e `IngresoDto` equivalente)
- [x] 3.2 Implementados `GastoExcelGenerator`/`IngresoExcelGenerator` en `Kash.Infrastructure/Reporting/` con ClosedXML: una hoja, cabecera (Fecha, Concepto, Categoría, Proveedor/Cliente, Persona, Cuenta, Forma de Pago, Importe, Descripción), formato `dd/mm/yyyy` y `#,##0.00 €`, filas alternas y bordes, mismos helpers que `PresupuestoExcelGenerator`
- [x] 3.3 Registrados ambos generadores en DI

## 4. Handlers

- [x] 4.1 Implementado `GetGastosExcelQueryHandler`: valida `UsuarioId` no vacío y (si hay rango) `FechaInicio <= FechaFin`, llama a `IGastoExportRepository.GetForExportAsync(...)`, genera el Excel con `IGastoExcelGenerator`, devuelve `Result.Success(new PresupuestoArchivoDto(nombre, excel))` con nombre `gastos_{yyyyMMdd}.xlsx` (fecha de generación, UTC)
- [x] 4.2 Implementado `GetIngresosExcelQueryHandler` equivalente, nombre `ingresos_{yyyyMMdd}.xlsx`

## 5. Aislamiento y validación final

- [x] 5.1 Verificado con datos reales: un `conceptoId` que sí pertenece a un usuario, pedido con un `usuarioId` distinto (aleatorio), devuelve 0 resultados tanto en Gastos como en Ingresos — el filtro `id_usuario` sigue aplicándose siempre, incluso combinado con otros filtros
- [x] 5.2 Verificado que listas vacías (`[]`, no `null`) en los 4 filtros de catálogo a la vez no rompen la consulta y devuelven el total sin restringir (mismo resultado que sin informar esos filtros); también verificado con un `conceptoId` inexistente (GUID aleatorio), que da 0 resultados sin error
- [x] 5.3 `dotnet build` de `Kash.Application`/`Kash.Infrastructure` (aislados) y `dotnet build Kash.sln` completo: compilación sin errores ni avisos
- [x] 5.4 Confirmado contra `Kash-Frontend` (change `dialogo-exportar-excel-gastos-ingresos`, `proposal.md`/`design.md`): los nombres de los query params (`fechaInicio`, `fechaFin`, `searchTerm`, `conceptoIds`, `categoriaIds`, `proveedorIds`/`clienteIds`, `personaIds`) coinciden exactamente con lo que ese cambio espera enviar
- [x] 5.5 `openspec validate exportacion-excel-gastos-ingresos --strict`: "Change 'exportacion-excel-gastos-ingresos' is valid"
