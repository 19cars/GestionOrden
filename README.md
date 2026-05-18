# Catálogo y órdenes (Full Stack)

Solución de prueba técnica: **Angular** (frontend), **API principal .NET 8**, **servicio interno .NET 8** (HTTP) y **PostgreSQL** (Evolve para migraciones SQL, EF Core + Npgsql para acceso a datos). Los **rutas y nombres de endpoints** están en **español** (por ejemplo `api/categorias`, `interno/ordenes/validar-y-calcular`).

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/) (servidor accesible en red o localhost)
- [Node.js 20+](https://nodejs.org/) y npm (para el frontend Angular)

## Puesta en marcha (localhost)

### 1) Base de datos y semilla

1. Cree en PostgreSQL una base vacía (y un usuario con permisos sobre ella) acorde a `DB-CONNECTION` en `src/CatalogoOrdenes.Api/.env` (por defecto: base `catalogo_ordenes`, usuario y contraseña `catalogo` en `127.0.0.1:5432`). Ejemplo en `psql` como superusuario:

   ```sql
   CREATE USER catalogo WITH PASSWORD 'catalogo';
   CREATE DATABASE catalogo_ordenes OWNER catalogo;
   ```

2. Cadena de conexión: edite `DB-CONNECTION` en la API y `DB-CONNECTION` en el servicio interno (misma base que la API, solo lectura de productos).

Al iniciar la API se aplican las migraciones SQL con **[Evolve](https://github.com/lecaillon/Evolve)** (misma cadena PostgreSQL que EF Core) desde `src/CatalogoOrdenes.Database/Migrations/`, incluyendo la semilla de datos de ejemplo para usuarios, roles, categorías y productos. El servicio interno no ejecuta Evolve; usa la misma base ya migrada. **Arranque la API (paso 2) al menos una vez** antes de usar el interno con datos reales. Si cambió de un esquema antiguo y hay conflictos, use una base vacía o elimine las tablas antes de volver a levantar la API.

### 2) API principal (puerto 5080)

En una terminal:

```bash
cd GestionOrden.App/src/GestionOrden.Api
dotnet run
```

Swagger (desarrollo): `https://localhost:7125/index`

## Pruebas backend

```bash
cd GestionOrden.Tests
dotnet test
```

## Seguridad y secretos

- **JWT**: configure `Jwt:SECRET` (mínimo 32 caracteres recomendado) mediante **variables de entorno** o **User Secrets** en entornos reales.
- **PostgreSQL**: no deje credenciales de producción en `appsettings.json`; use **User Secrets** o variables (`ConnectionStrings__BaseDatos`, etc.).
