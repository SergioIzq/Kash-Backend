## 1. Dominio y persistencia

- [x] 1.1 Añadir `ApiTokenHash` (string?) y `ApiTokenCreatedAt` (DateTime?) a `Usuario` (Kash.Domain/Usuarios/Usuario.cs), con un método `GenerarTokenApi(string hash)` que sobrescribe ambos campos siguiendo el patrón de `GenerarTokenRecuperacion()`; verificar compilando el proyecto y revisando manualmente que llamar dos veces sobrescribe el hash anterior (sin proyecto de tests en el repo — decisión confirmada con el usuario).
- [x] 1.2 Actualizar `UsuarioConfiguration` (EF Core) para mapear las nuevas columnas. El repo no usa EF Core Migrations (esquema gestionado a mano fuera del repo — confirmado con el usuario): en lugar de generar una migración, entregar el script `ALTER TABLE` correspondiente para que el usuario lo aplique manualmente sobre MySQL; verificar que el mapeo compila y que el script SQL es correcto para el tipo de columna elegido.
- [x] 1.3 Añadir a `IUsuarioReadRepository`/`UsuarioReadRepository` (o equivalente) un método para buscar un usuario por `ApiTokenHash`; verificar compilando el proyecto y con revisión manual de la query generada (sin proyecto de tests en el repo).

## 2. Generar y consultar el token (Application + Api)

- [x] 2.1 Crear `GenerateApiTokenCommand`/Handler en `Kash.Application/Features/Auth/Commands` que genera un valor aleatorio de 256 bits con prefijo `kash_pat_`, lo hashea (SHA-256) y llama a `Usuario.GenerarTokenApi`, devolviendo el valor en claro; verificar manualmente (Swagger/curl, llamando dos veces) que cada llamada devuelve un valor distinto.
- [x] 2.2 Crear `GetApiTokenStatusQuery`/Handler en `Kash.Application/Features/Auth/Queries` que devuelve si existe token activo y su fecha de creación, sin el valor; verificar que nunca incluye el token en el DTO de respuesta.
- [x] 2.3 Añadir `POST /api/auth/api-token` (`[Authorize]`) en `AuthController` que ejecuta `GenerateApiTokenCommand` y devuelve el valor en claro; verificar manualmente (Swagger/curl) que la respuesta incluye el token solo en esa llamada.
- [x] 2.4 Añadir `GET /api/auth/api-token` (`[Authorize]`) en `AuthController` que ejecuta `GetApiTokenStatusQuery`; verificar que devuelve `{ existe: false }` para un usuario sin token generado y `{ existe: true, creadoEn: ... }` tras generarlo.

## 3. Autenticación combinada (pipeline)

- [x] 3.1 Decodificar el payload de un JWT real emitido por `POST /api/auth/login` para confirmar el nombre/tipo exacto del claim de `UsuarioId` que usan `GetCurrentUserId`/`RequireCurrentUserId`; documentar el hallazgo en un comentario del handler nuevo.
- [x] 3.2 Implementar `ApiTokenAuthenticationHandler` (`Kash.Api` o `Kash.Infrastructure`) que lee el `Bearer`, calcula su hash, busca el usuario vía el repositorio de 1.3 y construye un `ClaimsPrincipal` con el mismo claim de `UsuarioId` confirmado en 3.1; verificar manualmente (curl) que una petición con token válido resuelve el usuario correcto y con token inválido devuelve 401.
- [x] 3.3 Registrar el esquema del handler nuevo junto al JWT del Kernel en `Program.cs` mediante `AddPolicyScheme`, enrutando por forma del header (JWT de 3 segmentos vs token de API); verificado en vivo contra la API real: `GET /api/auth/me` con el JWT de sesión → 200 (regresión), con el token de API → 200, con token inválido → 401, sin cabecera → 401.
- [x] 3.4 Prueba de extremo a extremo: generado un token de API vía 2.3 y usado como `Bearer` contra la API real para `GET /api/gastos/habituales` (200, datos reales), `GET /api/gastos/sugerencia` (200) y `POST /api/gastos` (201) + `DELETE` de limpieza; las tres responden igual que con el JWT de sesión.
- [x] 3.5 Prueba de aislamiento: verificado en vivo con dos usuarios reales (admin@admin.com y seizquie@gmail.com) — el token de cada uno resuelve `/me` como su propio usuario y `habituales` devuelve únicamente sus propios datos (la lista del otro usuario sale vacía). Rotación verificada: tras regenerar el token de admin, el token anterior devuelve 401 y el nuevo 200.

## 4. Documentación

- [x] 4.1 Añadir a la documentación de la API (README o comentario XML de Swagger en `AuthController`) una nota breve sobre el propósito del token de API y que regenerar invalida el anterior.
