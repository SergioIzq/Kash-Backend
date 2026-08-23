## Why

Los endpoints de Kash exigen un JWT de sesión (caduca a las 720 minutos / 12h) sin mecanismo de refresh. Eso hace inviable integrar clientes no interactivos de larga vida —como un Atajo de iOS que crea gastos/ingresos automáticamente— porque no hay forma de mantenerlos autenticados más de 12h sin volver a enviar la contraseña en cada ejecución. Se necesita una credencial de API estable, pensada para integraciones del propio usuario, distinta de la sesión web/app.

## What Changes

- Cada usuario puede generar, desde su perfil (autenticado con su sesión normal), un **token de API personal** de larga duración para usar en integraciones (Atajos de iOS, scripts, etc.).
- El token se muestra en claro **una única vez** en el momento de generarlo; el backend solo persiste su hash.
- Volver a solicitar la generación **sustituye** el token anterior por uno nuevo (rotación), invalidando el anterior de forma inmediata — es el mecanismo de "revocación" para este diseño simple (sin tabla de múltiples tokens con nombre/borrado individual).
- Las peticiones autenticadas con este token de API tienen acceso equivalente a un JWT de sesión (mismo usuario, mismos endpoints); no se introducen permisos reducidos en esta fase.
- Nuevo pipeline de autenticación: si el `Bearer` recibido no es un JWT válido, se comprueba contra el hash del token de API del usuario antes de rechazar la petición.

## Capabilities

### New Capabilities
- `token-personal-api`: generación, rotación y uso de un token de API personal por usuario para autenticar integraciones de larga duración (p. ej. Atajos de iOS) sin depender del JWT de sesión de 12h.

### Modified Capabilities
(ninguna — no cambia el comportamiento de login/JWT existente, solo se añade una vía de autenticación adicional)

## Impact

- **Backend**: nueva columna(s) en `Usuario` (o tabla 1:1) para `ApiTokenHash` + `ApiTokenCreatedAt`; nuevo `AuthenticationHandler`/esquema combinado junto al JWT existente en `AddKernelJwtAuthentication`; nuevos endpoints en `AuthController` (`POST /api/auth/api-token` para generar/rotar, `GET /api/auth/api-token` para saber si existe y desde cuándo, sin revelar el valor).
- **Fuera de alcance de este repo**: la configuración del Atajo de iOS en sí (Shortcuts) no es código de Kash-Backend; se documentará como guía de uso en `design.md` para que el usuario lo configure en su iPhone, pero no se implementa aquí.
- **Seguridad**: el token de API tiene el mismo alcance que la sesión completa del usuario (sin scopes reducidos); queda documentado como limitación conocida, no como bloqueo de esta fase.
