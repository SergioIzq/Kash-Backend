## 1. Script de esquema

- [x] 1.1 Escribir `schema.sql` con `ALTER TABLE ... MODIFY nombre VARCHAR(100) COLLATE utf8mb4_0900_ai_ci` para `categorias`, `conceptos`, `cuentas`, `formas_pago`, `proveedores`, `clientes` y `personas`, siguiendo el formato de `openspec/changes/archive/2026-08-24-token-personal-api/schema.sql`.

## 2. Aplicación en desarrollo y verificación

- [x] 2.1 Aplicar `schema.sql` contra la base de datos de desarrollo local y verificar con `SHOW FULL COLUMNS FROM <tabla>` que la columna `nombre` de las 7 tablas queda con collation `utf8mb4_0900_ai_ci`.
- [x] 2.2 Con datos de prueba que mezclen mayúsculas/minúsculas (p. ej. categorías `IA`, `Ocio`, `Sin clasificar`, `categoria`), verificar en la app que el listado paginado de Categorías (`/categorias`) las muestra en orden alfabético case-insensitive (`categoria`, `IA`, `Ocio`, `Sin clasificar`).
- [x] 2.3 Verificar en los modales de crear/editar Gasto e Ingreso (`Kash-Frontend`) que los desplegables de Categoría, Concepto, Cuenta, Forma de Pago, Proveedor/Cliente y Persona muestran el catálogo completo en el mismo orden alfabético case-insensitive al abrirlos en blanco.
- [x] 2.4 Verificar que una búsqueda por texto que coincida con nombres de distinta capitalización (autocompletado) devuelve los resultados en orden alfabético case-insensitive.

## 3. Aplicación en producción

- [x] 3.1 Aplicar `schema.sql` contra la base de datos de producción en una ventana de bajo tráfico y repetir la verificación de collation (`SHOW FULL COLUMNS`) para las 7 tablas.
