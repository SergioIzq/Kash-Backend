## Context

MySQL 8.0.43 (ver `Kash.Infrastructure/DependencyInjection.cs`, `MySqlServerVersion(new Version(8, 0, 43))`). La conexión (`SqlDbConnectionFactory`) fija `CharacterSet = "utf8mb4"` pero no una collation — la collation efectiva de cada columna es la que tiene definida en la tabla. Las 7 columnas `nombre` de catálogo son `VARCHAR(100)` (ver `*Configuration.cs` en `Kash.Infrastructure/Persistence/Command/Configurations`) y hoy usan una collation sensible a mayúsculas/acentos (probablemente heredada del charset/collation por defecto del servidor o de la tabla en el momento de creación — este repo no versiona el DDL original, ver proposal.md). Ver proposal.md - Why para el bug observado.

Ninguna de las 7 tablas tiene un índice `UNIQUE` sobre `nombre` (confirmado revisando `Kash.Infrastructure/Persistence/Command/Configurations/{Categoria,Concepto,Cuenta,FormaPago,Proveedor,Cliente,Persona}Configuration.cs`: solo índices no únicos `(usuario, nombre)` para rendimiento de búsqueda). La deduplicación de nombres es responsabilidad de la capa de aplicación (`ExistsWithSameNameAsync` / `ExistsWithSameNameExceptAsync` en cada `*ReadRepository`).

## Goals / Non-Goals

**Goals:**
- Orden alfabético case-insensitive y accent-insensitive al ordenar por `nombre` en las 7 tablas de catálogo, sin tocar código de aplicación ni el paquete de kernel.

**Non-Goals:**
- No se cambia la collation de ninguna otra columna (descripciones, tablas de movimientos, etc.) ni el charset/collation por defecto del servidor o la base de datos.
- No se añade una restricción `UNIQUE` sobre `nombre` ni se resuelven duplicados existentes que solo difieran en mayúsculas/minúsculas o acentos — se documenta como efecto colateral en el proposal, pero deduplicar datos no es objetivo de este change.
- No se toca `AbsReadRepository` (kernel): las consultas que construye ya son correctas una vez la collation deja de ser case-sensitive.

## Decisions

**Collation elegida: `utf8mb4_0900_ai_ci`.** Es la collation Unicode "accent/case-insensitive" nativa de MySQL 8.0 (ya en uso: 8.0.43 soporta `utf8mb4_0900_ai_ci` desde 8.0.0), coherente con el `CharacterSet = utf8mb4` ya configurado en la conexión. Alternativa descartada: `utf8mb4_unicode_ci`, pensada para compatibilidad con MySQL 5.x/MariaDB — no aporta nada aquí porque ya estamos en MySQL 8 y `_0900_ai_ci` tiene mejor soporte Unicode y mejor rendimiento en 8.0.

**Alcance: las 7 columnas `nombre` de catálogo, no toda la base de datos.** Cambiar la collation por defecto del servidor/base de datos afectaría a tablas no relacionadas con este bug (movimientos, usuarios, etc.) sin necesidad — el problema observado es específicamente el orden alfabético de catálogos con nombre libre. Alternativa descartada: `ALTER DATABASE ... CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci` — cambia el default para tablas futuras pero no las existentes, así que igualmente habría que alterar cada tabla existente; se prefiere ser explícito por tabla.

**Aplicación vía `schema.sql` manual, no vía capa de aplicación.** Sigue el patrón ya establecido en este repo (`openspec/changes/archive/2026-08-24-token-personal-api/schema.sql`): no hay EF Core Migrations, los cambios de esquema se documentan en el change y se aplican a mano contra cada entorno (dev, producción).

## Risks / Trade-offs

- **[Riesgo] Filas existentes que ya son duplicados case/accent-insensitive quedan sin fusionar.** La collation solo cambia cómo se comparan/ordenan los valores en consultas futuras, no reescribe datos. → Mitigación: ninguna requerida por este change (no hay `UNIQUE` que pueda romperse); si en el futuro se quiere fusionar duplicados, sería un change aparte, con decisión explícita de negocio sobre qué fila prevalece.
- **[Riesgo] `ALTER TABLE ... MODIFY` reconstruye la tabla completa en InnoDB.** Con el volumen de datos actual (catálogos personales, no tablas de eventos) el bloqueo es breve, pero conviene aplicarlo en una ventana de bajo tráfico en producción. → Mitigación: ejecutar el script manualmente fuera de horas pico; no requiere cambio de código ni despliegue coordinado.
- **[Trade-off] `ORDER BY` sobre una columna con collation `_ai_ci` no puede aprovechar un índice que use la collation anterior para el propósito de ordenar (case-sensitive), pero sí sigue sirviendo para búsquedas por igualdad/prefijo.** Los índices existentes (`(usuario, nombre)`) son no únicos y de rendimiento moderado dado el volumen por usuario; no se considera necesario recrearlos.

## Migration Plan

1. Aplicar `schema.sql` manualmente contra la base de datos de desarrollo local; verificar en la app que los 7 catálogos listan/buscan/previsualizan en orden alfabético case-insensitive (usar el mismo caso reproducido en la exploración: categorías `IA`, `Ocio`, `Sin clasificar`, `categoria`).
2. Aplicar el mismo script contra producción en una ventana de bajo tráfico.
3. Sin rollback de código (no hay cambios de código). Rollback de esquema: volver a `COLLATE` anterior por tabla si apareciera un problema no previsto (no se espera, dado que no hay índices `UNIQUE` afectados).
