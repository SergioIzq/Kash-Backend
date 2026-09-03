-- collation-case-insensitive-catalogos: collation case-insensitive/accent-insensitive
-- para la columna `nombre` de los catálogos de nombre libre, para que ORDER BY nombre ASC
-- (listados paginados, autocompletado y "recientes") ordene alfabéticamente como espera
-- un humano, en vez de por valor de byte (mayúsculas antes que minúsculas).
-- Aplicar manualmente sobre la base de datos MySQL (este repo no usa EF Core Migrations).
-- Collation utf8mb4_0900_ai_ci: nativa de MySQL 8.0 (servidor en uso: 8.0.43).

ALTER TABLE categorias
    MODIFY nombre VARCHAR(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL;

ALTER TABLE conceptos
    MODIFY nombre VARCHAR(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL;

ALTER TABLE cuentas
    MODIFY nombre VARCHAR(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL;

ALTER TABLE formas_pago
    MODIFY nombre VARCHAR(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL;

ALTER TABLE proveedores
    MODIFY nombre VARCHAR(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL;

ALTER TABLE clientes
    MODIFY nombre VARCHAR(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL;

ALTER TABLE personas
    MODIFY nombre VARCHAR(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL;
