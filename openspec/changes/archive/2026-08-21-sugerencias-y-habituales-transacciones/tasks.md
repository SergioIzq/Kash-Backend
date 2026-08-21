## 1. Sugerencia de gasto (capability: sugerencias-transaccion)

- [x] 1.1 **[Reescrita en el grupo 7]** `GetSugerenciaGastoQuery` ya no hereda de `GetRecentQuery` — ver grupo 7 para el motivo y la implementación final
- [x] 1.2 **[Reescrita en el grupo 7]** `GetSugerenciaGastoQueryHandler` ya no hereda de `GetRecentQueryHandler` ni usa `GetCustomFilters` — ver grupo 7
- [x] 1.3 Añadir `GET /api/gastos/sugerencia` a `GastosController` (`[FromQuery] Guid conceptoId`), siguiendo el mismo patrón `RequireCurrentUserId` + `SendAndHandleAsync` que el resto de acciones del controller; verificar manualmente con un `conceptoId` que tiene gastos previos del usuario autenticado (devuelve 1 elemento) y uno que no tiene ninguno (devuelve lista vacía)
- [x] 1.4 Aislamiento por `usuarioId` por diseño: `GetRecentAsync(usuarioId, limit, extraFilters, ct)` recibe `usuarioId` como parámetro obligatorio (no opcional) y es el mismo método ya usado en producción para aislar Conceptos/Categorías/Proveedores/etc. — no se decompiló el cuerpo del método (solo su firma vía reflexión), y no se ejecutó contra datos reales en esta sesión; recomendable una prueba en vivo antes de archivar

## 2. Sugerencia de ingreso (capability: sugerencias-transaccion)

- [x] 2.1 **[Reescrita en el grupo 7]** `GetSugerenciaIngresoQuery` ya no hereda de `GetRecentQuery` — ver grupo 7
- [x] 2.2 **[Reescrita en el grupo 7]** `GetSugerenciaIngresoQueryHandler` ya no hereda de `GetRecentQueryHandler` — ver grupo 7
- [x] 2.3 Añadir `GET /api/ingresos/sugerencia` a `IngresosController`; verificar igual que 1.3 con datos de ingresos

## 3. Gastos habituales (capability: transacciones-habituales)

- [x] 3.1 Crear `GastoHabitualDto` en `Kash.Shared.Application/Dtos/` con los campos de la combinación (conceptoId/Nombre, categoriaId/Nombre, cuentaId/Nombre, formaPagoId/Nombre, proveedorId/Nombre, personaId/Nombre) más `Veces: int` y `UltimoUso: DateTime`
- [x] 3.2 **[Corregido durante la implementación]** `Kash.Domain` no referencia `Kash.Shared.Application` (confirmado en `Kash.Domain.csproj`), así que un DTO no puede formar parte de `IGastoReadRepository`. En su lugar: `IGastoHabitualesRepository.GetHabitualesAsync(Guid usuarioId, int limit, CancellationToken ct)` en `Kash.Application/Interfaces/`, mismo patrón que `IDashboardRepository`/`IReporteRepository` (repositorio de agregación registrado manualmente en DI, no vía Scrutor)
- [x] 3.3 Implementar `GetHabitualesAsync` en `GastoHabitualesRepository` (`Kash.Infrastructure/Persistence/Query/`, mismo patrón que `DashboardRepository`) con Dapper: `_dbConnectionFactory.CreateConnection()` + SQL crudo, agrupando por `id_concepto, id_cuenta, id_forma_pago, id_proveedor, id_persona` (y sus columnas de nombre, para evitar depender de la detección de dependencia funcional de `ONLY_FULL_GROUP_BY`), `COUNT(*) AS Veces`, `MAX(fecha) AS UltimoUso`, `HAVING COUNT(*) > 1`, `ORDER BY Veces DESC, UltimoUso DESC`, `LIMIT @Limit`; registrado en DI en `Kash.Infrastructure/DependencyInjection.cs`. Verificación con datos reales pendiente (ver informe de aplicación)
- [x] 3.4 Crear `GetHabitualesGastosQuery`/`GetHabitualesGastosQueryHandler` (MediatR, sin heredar de bases del kernel) en `Kash.Application/Features/Gastos/Queries/Habituales/`, llamando a `IGastoHabitualesRepository.GetHabitualesAsync`; caché de 30s vía `ICacheService.GetAsync`/`SetAsync` con clave única por usuario `gasto_habituales:{usuarioId}` (cachea top-20, recorta a `limit` en memoria) — formato revisado en el grupo 6 para permitir invalidación exacta, ver `design.md` Decisión 3
- [x] 3.5 Añadido `GET /api/gastos/habituales` a `GastosController` (`[FromQuery] int limit = 6`); build correcto y SQL revisada (`HAVING COUNT(*) > 1` garantiza lista vacía sin combinaciones repetidas). Sin ejecución contra datos reales en esta sesión; recomendable una prueba en vivo antes de archivar

## 4. Ingresos habituales (capability: transacciones-habituales)

- [x] 4.1 Repetir 3.1 para `IngresoHabitualDto`, con `clienteId/Nombre` en vez de `proveedorId/Nombre` (los ingresos usan `id_cliente`, no `id_proveedor` — confirmado en `IngresoReadRepository.cs`)
- [x] 4.2 Repetir 3.2/3.3 con la misma corrección de ubicación: `IIngresoHabitualesRepository` (`Kash.Application/Interfaces/`) + `IngresoHabitualesRepository` (`Kash.Infrastructure/Persistence/Query/`), agrupando por `id_concepto, id_cuenta, id_forma_pago, id_cliente, id_persona`
- [x] 4.3 Repetir 3.4 para `GetHabitualesIngresosQuery`/Handler
- [x] 4.4 Añadido `GET /api/ingresos/habituales` a `IngresosController`; mismo nivel de verificación que 3.5 (build + revisión de SQL, sin ejecución contra datos reales)

## 5. Invalidación de caché al crear (pedido explícitamente por el usuario tras ver el desfase en el frontend)

- [x] 5.1 Cambiar la clave de caché de `habituales` de `{entidad}_habituales:{usuarioId}:{limit}` a `{entidad}_habituales:{usuarioId}` (sin `limit`): se cachea siempre el top-20 (`MaxCacheable`) y se recorta a `request.Limit` en memoria (`cached.Take(request.Limit)`); si `limit > 20` se consulta sin caché. Expuesto como `GetHabitualesGastosQueryHandler.CacheKey(usuarioId)` / `GetHabitualesIngresosQueryHandler.CacheKey(usuarioId)` (método estático público) para que el invalidador use exactamente la misma clave
- [x] 5.2 Investigado `ICacheService.InvalidateByPatternAsync` como alternativa (habría evitado tocar el formato de clave): confirmado por el XML doc del paquete que su implementación real (`DistributedCacheService`) es un no-op ("IDistributedCache estándar no soporta invalidación por patrón") — descartado antes de usarlo, no después
- [x] 5.3 `CreateGastoCommandHandler` sobrescribe el hook `OnEntityCreatedAsync(Gasto, Guid, CancellationToken)` de `AbsCreateCommandHandler` (confirmado `virtual` y no `sealed` por reflexión antes de usarlo) y llama a `_cacheService.RemoveAsync(GetHabitualesGastosQueryHandler.CacheKey(entity.UsuarioId.Value))`
- [x] 5.4 Mismo cambio en `CreateIngresoCommandHandler` con `GetHabitualesIngresosQueryHandler.CacheKey`
- [x] 5.5 `dotnet build` de `Kash.Application` y `Kash.Infrastructure` (aislados) sin errores; más tarde, tras el grupo 7, `dotnet build Kash.sln` completo también compiló limpio (0 errores) — el bloqueo de `Kash.Api` por un proceso en ejecución que impedía la compilación de la solución completa ya no estaba presente en una verificación posterior
- [x] 5.6 Verificado por el usuario en vivo: al crear un gasto, el chip de habituales se actualiza sin esperar 30s

## 7. Corrección de orden en sugerencia + eliminar caché (pedido explícitamente por el usuario tras probar la app)

- [x] 7.1 Diagnóstico confirmado antes de tocar código: `GetRecentAsync` (genérico) ordena según el `DefaultOrderBy` de `GastoReadRepository`/`IngresoReadRepository`, que es `fecha DESC, id DESC` — no `fecha_creacion` como asumía la Decisión 1 original. Con varios gastos en la misma fecha, el desempate por `id` (GUID) es esencialmente aleatorio, no refleja el orden de creación real. Esto explica el bug reportado
- [x] 7.2 Creadas `IGastoSugerenciaRepository`/`IIngresoSugerenciaRepository` (`Kash.Application/Interfaces/`, mismo patrón que `IGastoHabitualesRepository`) con `GetUltimoUsoAsync(usuarioId, conceptoId, ct)`
- [x] 7.3 Implementadas `GastoSugerenciaRepository`/`IngresoSugerenciaRepository` (`Kash.Infrastructure/Persistence/Query/`) con Dapper: mismas columnas/joins que `GastoReadRepository`/`IngresoReadRepository`, `WHERE id_usuario = @UsuarioId AND id_concepto = @ConceptoId`, `ORDER BY fecha DESC, fecha_creacion DESC LIMIT 1` explícito; registradas en DI
- [x] 7.4 Reescritas `GetSugerenciaGastoQuery`/`GetSugerenciaIngresoQuery` y sus handlers como `IRequest<Result<IReadOnlyList<...Dto>>>` planos (ya no heredan de `GetRecentQuery`/`GetRecentQueryHandler` del kernel) — **sin ninguna caché**, pedido explícitamente por el usuario para que crear un gasto/ingreso se refleje de inmediato en la siguiente sugerencia
- [x] 7.5 Mantenido el contrato de wire ya acordado con el frontend (200 + array de 0/1 elementos, mismas columnas que `GastoDto`/`IngresoDto`) — no requiere cambios en `Kash-Frontend`
- [x] 7.6 `dotnet build` de `Kash.Application`/`Kash.Infrastructure` (aislados) y `dotnet build Kash.sln` completo: compilación correcta, 0 errores
- [x] 7.7 Verificado por el usuario en vivo: creando dos gastos el mismo día, la sugerencia del segundo alta recoge correctamente el último creado

## 6. Validación final

- [x] 6.1 `dotnet build Kash.sln`: compilación correcta, 0 advertencias, 0 errores (los 6 proyectos, incluidos `Kash.Domain`, `Kash.Application`, `Kash.Infrastructure`, `Kash.Api`)
- [x] 6.2 No existe ningún proyecto de test en la solución (`find . -iname "*.Tests.csproj"` sin resultados) — nada que ejecutar, no aplica
- [x] 6.3 Confirmado contra `Kash-Frontend` (`agilizar-alta-transacciones/tasks.md`): las rutas y parámetros coincidían; se encontró y corrigió una discrepancia real — ese documento asumía `404` para "sin sugerencia", pero el backend (al reutilizar `GetRecentQueryHandler`) devuelve `200` con lista vacía. Corregido en `agilizar-alta-transacciones/tasks.md` (tareas 1.1-1.4 y 2.2). El propio uso de `GetRecentQueryHandler` para sugerencia fue revisado y sustituido en el grupo 7
- [x] 6.4 `openspec validate sugerencias-y-habituales-transacciones --strict` → válido
