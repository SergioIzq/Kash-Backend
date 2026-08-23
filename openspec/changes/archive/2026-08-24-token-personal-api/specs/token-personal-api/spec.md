## Purpose

Permite a un usuario autenticado generar un token de API personal de larga duración para autenticar integraciones no interactivas (como un Atajo de iOS) que crean gastos o ingresos sin depender del JWT de sesión, que caduca a las 12 horas.

## ADDED Requirements

### Requirement: Generación de token de API personal
El sistema SHALL permitir a un usuario autenticado con su sesión normal (JWT o cookie de sesión) generar un token de API personal, devolviendo su valor en texto claro únicamente en la respuesta de esa solicitud de generación.

#### Scenario: Primera generación de token
- **WHEN** un usuario autenticado que no tiene ningún token de API previo solicita generar uno
- **THEN** el sistema crea un token nuevo, lo persiste únicamente como hash y devuelve el valor en claro en la respuesta

#### Scenario: El valor no se puede recuperar después de generarlo
- **WHEN** un usuario consulta el estado de su token de API en cualquier momento posterior a su generación
- **THEN** el sistema nunca vuelve a devolver el valor en claro del token, solo metadatos sobre su existencia

### Requirement: Regenerar el token invalida el anterior
El sistema SHALL sustituir cualquier token de API existente del usuario al generar uno nuevo, invalidando el anterior de forma inmediata, como único mecanismo de revocación de este diseño.

#### Scenario: Regenerar token con uno previo activo
- **WHEN** un usuario que ya tiene un token de API activo solicita generar uno nuevo
- **THEN** el sistema sustituye el token anterior por el nuevo y devuelve el nuevo valor en claro

#### Scenario: El token anterior deja de aceptarse tras la rotación
- **WHEN** una petición usa un token de API que fue sustituido por una regeneración posterior
- **THEN** el sistema rechaza la petición como no autenticada, igual que si el token nunca hubiera existido

### Requirement: Consulta del estado del token sin revelar su valor
El sistema SHALL exponer un endpoint que indique, para el usuario autenticado, si tiene un token de API activo y desde cuándo, sin revelar en ningún caso el valor del token.

#### Scenario: Consultar estado con token existente
- **WHEN** un usuario autenticado con un token de API activo consulta su estado
- **THEN** el sistema devuelve que existe un token activo junto con su fecha de creación, sin incluir el valor del token

#### Scenario: Consultar estado sin token generado
- **WHEN** un usuario autenticado que nunca ha generado un token de API consulta su estado
- **THEN** el sistema indica que no existe ningún token activo

### Requirement: Autenticación de peticiones mediante token de API
El sistema SHALL aceptar el token de API personal como credencial `Bearer` alternativa al JWT de sesión en cualquier endpoint protegido, resolviendo el usuario propietario del token igual que si se hubiera autenticado con su JWT de sesión.

#### Scenario: Petición autenticada con token de API válido
- **WHEN** una petición a un endpoint protegido incluye un `Bearer` que coincide con el token de API activo de un usuario
- **THEN** el sistema autentica la petición como ese usuario y la procesa con normalidad

#### Scenario: Petición con token de API inválido o inexistente
- **WHEN** una petición a un endpoint protegido incluye un `Bearer` que no es un JWT válido ni coincide con ningún token de API activo
- **THEN** el sistema rechaza la petición como no autenticada

### Requirement: El token de API concede el mismo acceso que la sesión del usuario
El sistema SHALL conceder a las peticiones autenticadas por token de API el mismo acceso a endpoints y datos que tendría el usuario autenticado con su sesión normal, sin restringir permisos en esta fase.

#### Scenario: Acceso equivalente a sesión de usuario
- **WHEN** un usuario autenticado por token de API solicita un recurso al que también podría acceder con su sesión normal
- **THEN** el sistema le concede el mismo acceso, con los mismos datos, que si hubiera usado su sesión normal

### Requirement: Aislamiento entre usuarios
El sistema SHALL validar el token de API exclusivamente contra su propietario, sin permitir que el token de un usuario autentique peticiones en nombre de otro usuario.

#### Scenario: Token de un usuario no autentica como otro usuario
- **WHEN** se usa el token de API válido de un usuario
- **THEN** el sistema únicamente concede acceso a los datos y recursos de ese usuario propietario, nunca de otro
