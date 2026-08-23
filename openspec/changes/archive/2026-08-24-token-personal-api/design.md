## Context

Ver proposal.md - Why. Restricciones técnicas relevantes descubiertas al explorar el código:

- La autenticación JWT actual se configura con `builder.Services.AddKernelJwtAuthentication(...)`, que vive en el paquete NuGet externo `SergioIzq.AspNetCore.Kernel` (v0.2.6) — **no es código de este repo**, así que no se puede modificar directamente. Cualquier vía de autenticación adicional debe añadirse en `Kash.Api`/`Kash.Infrastructure` sin tocar ese paquete.
- Los controladores usan helpers `RequireCurrentUserId` / `GetCurrentUserId` de `AbsController` (mismo paquete externo) para leer el usuario autenticado desde los claims. La nueva vía de autenticación debe producir un `ClaimsPrincipal` con la misma forma de claims que el JWT del Kernel para que esos helpers sigan funcionando sin cambios.
- `Usuario` (`Kash.Domain/Usuarios/Usuario.cs`) ya tiene el patrón de "token + expiración" para `TokenRecuperacion`/`TokenConfirmacion`, aunque esos se guardan en claro (son de un solo uso y expiran en 1h). El token de API es una credencial de larga duración con alcance total de cuenta, así que se hashea antes de persistir — desviación deliberada de ese patrón existente, justificada por el mayor impacto de una fuga.

## Goals / Non-Goals

**Goals:**
- Permitir generar/rotar un token de API personal reutilizando la sesión web normal.
- Aceptar ese token como alternativa al JWT en cualquier endpoint ya protegido, sin duplicar lógica de autorización por controlador.
- Mantener el cambio contenido en `Kash.Api` + `Kash.Domain`/`Kash.Infrastructure`, sin tocar el paquete Kernel externo.

**Non-Goals:**
- Scopes o permisos reducidos para el token (p. ej. "solo crear gastos"): el token tiene el mismo alcance que la sesión completa. Queda como posible trabajo futuro.
- Múltiples tokens con nombre por integración: el diseño es deliberadamente de **un único token por usuario**, sin tabla ni gestión individual (decisión explícita del usuario).
- Notificaciones (email) al generar/rotar el token.

## Decisions

**Almacenamiento: campos en `Usuario`, no tabla nueva.**
Se añaden `ApiTokenHash` (string?, hash SHA-256 en hex/base64) y `ApiTokenCreatedAt` (DateTime?, UTC) directamente al agregado `Usuario`, siguiendo el mismo patrón que `TokenRecuperacion`/`TokenRecuperacionExpiracion`. Alternativa descartada: tabla `PersonalAccessTokens` 1:N — se descartó explícitamente por el usuario a favor de la opción simple de un único token.

**Formato del token: valor aleatorio de alta entropía, no JWT.**
Se genera con un generador criptográficamente seguro (256 bits), codificado en base64url, con un prefijo reconocible (p. ej. `kash_pat_`) para poder distinguirlo de un JWT sin decodificar nada y para que sea identificable si aparece accidentalmente en un log. Se persiste solo su hash SHA-256; no necesita un hash lento tipo bcrypt/argon2 porque no es una contraseña de baja entropía elegida por un humano — es aleatorio y no memorizable, por lo que un ataque de diccionario no aplica.

**Regenerar = revocar el anterior.**
`GenerarTokenApi()` en el dominio sobrescribe `ApiTokenHash`/`ApiTokenCreatedAt`. No hay borrado lógico ni histórico de tokens previos: el anterior deja de ser válido en el mismo momento porque su hash ya no coincide con nada almacenado.

**Autenticación combinada sin tocar el paquete Kernel: policy scheme + handler propio.**
En `Program.cs` (Kash.Api), tras `AddKernelJwtAuthentication`, se añade:
1. Un `AuthenticationHandler` propio (`ApiTokenAuthenticationHandler`) que lee el `Bearer`, calcula su hash y busca un usuario cuyo `ApiTokenHash` coincida; si lo encuentra, construye un `ClaimsPrincipal` con los mismos claims que emite el JWT del Kernel (a confirmar su forma exacta inspeccionando el paquete en implementación — ver Open Questions).
2. Un `AddPolicyScheme` como esquema por defecto que inspecciona el header `Authorization`: si el valor tiene la forma de un JWT (3 segmentos separados por `.`), reenvía al esquema JWT del Kernel; si no, reenvía al esquema de token de API. Así `[Authorize]` sigue funcionando igual en todos los controladores, sin anotar cada endpoint con dos esquemas.

Alternativa descartada: intentar validar primero como JWT y, si falla, reintentar como token de API dentro del mismo handler — más simple conceptualmente pero mezclaría dos formatos de credencial en un único `AuthenticationHandler`, dificultando el mantenimiento futuro si el Kernel cambia su implementación de JWT.

## Risks / Trade-offs

- **[Riesgo] El token da acceso total a la cuenta, sin expiración salvo regeneración manual** → Mitigación: es una decisión consciente del usuario (opción "simple" elegida); el mecanismo de rotación cubre el caso "he perdido el móvil", siempre que el usuario recuerde regenerarlo desde la web.
- **[Riesgo] La forma exacta de los claims que emite el JWT del Kernel no está confirmada desde este repo** (el paquete es una dependencia externa) → Mitigación: durante la implementación, inspeccionar el JWT emitido por `/api/auth/login` (decodificar su payload) antes de escribir `ApiTokenAuthenticationHandler`, para replicar exactamente esos claims.
- **[Riesgo] Detección de "parece un JWT" por número de segmentos es una heurística, no una validación** → Mitigación: es solo para el enrutado del `PolicyScheme`; si un token de API generado por casualidad tuviera 3 segmentos separados por `.` (extremadamente improbable con base64url de 256 bits aleatorios), el esquema JWT lo rechazaría igualmente por firma inválida y el usuario debería regenerarlo — riesgo aceptado por su probabilidad despreciable.
- **[Riesgo] Un token filtrado (log, captura de pantalla del Atajo, etc.) compromete la cuenta hasta que se regenera** → Mitigación: documentar claramente en la guía de configuración del Atajo que el token no debe compartirse ni pegarse en sitios públicos.

## Migration Plan

1. Migración de EF Core añadiendo `ApiTokenHash` (nullable) y `ApiTokenCreatedAt` (nullable) a la tabla `usuarios`. Sin backfill: todos los usuarios existentes empiezan sin token, lo generan bajo demanda.
2. Sin cambios de comportamiento para usuarios que no generen el token: el login/JWT actual sigue funcionando exactamente igual.
3. Rollback: revertir la migración (drop de las dos columnas) y quitar el `PolicyScheme`/handler de `Program.cs`; no hay estado externo que limpiar.

## Open Questions

- Forma exacta de los claims del JWT emitido por el Kernel (nombre del claim de `UsuarioId`, tipo, etc.) — se resuelve inspeccionando un JWT real durante la implementación, no cambia ni el enfoque ni las specs.
- Nombre final del prefijo del token (`kash_pat_` es una propuesta) — puramente cosmético, no afecta a las specs.

## Guía de configuración del Atajo de iOS (fuera del alcance de este repo)

Una vez implementado el backend, la configuración en el iPhone (no es código de Kash-Backend, se documenta aquí como referencia):

1. En el perfil de la web, generar el token de API y copiarlo (aparece una única vez).
2. Crear un Atajo nuevo → añadir el token como texto fijo en una variable al principio del Atajo (o pegarlo directamente en la cabecera `Authorization`).
3. Acción "Obtener contenido de URL" → `GET https://<api>/api/gastos/habituales?limit=8`, cabecera `Authorization: Bearer <token>`.
4. Acción "Elegir de una lista", usando los nombres de concepto/forma de pago del resultado anterior como opciones.
5. Acción "Obtener contenido de URL" → `GET https://<api>/api/gastos/sugerencia?conceptoId=<id elegido>`, misma cabecera, para obtener el importe por defecto.
6. Acción "Preguntar por texto" con el importe de la sugerencia como valor por defecto, para confirmar o corregir.
7. Acción "Obtener contenido de URL" → `POST https://<api>/api/gastos`, cuerpo JSON con `conceptoId`, `importe`, `cuentaId`, `formaPagoId`, `categoriaId`, `proveedorId`/`personaId` (de los pasos 3 y 5) y `fecha` = ahora.
8. Añadir el Atajo a la pantalla de inicio o al Botón de Acción; duplicar el proceso para ingresos apuntando a `/api/ingresos/*`.
