-- token-personal-api: columnas nuevas en `usuarios` para el token de API personal.
-- Aplicar manualmente sobre la base de datos MySQL (este repo no usa EF Core Migrations).

ALTER TABLE usuarios
    ADD COLUMN api_token_hash VARCHAR(64) NULL AFTER avatar,
    ADD COLUMN api_token_created_at DATETIME NULL AFTER api_token_hash;

ALTER TABLE usuarios
    ADD UNIQUE INDEX idx_usuario_api_token_hash (api_token_hash);
