# Catálogo y órdenes (Full Stack)

Solución de prueba técnica: **Angular** (frontend), **API principal .NET 8**, **servicio interno .NET 8** (HTTP) y **PostgreSQL** (Evolve para migraciones SQL, EF Core + Npgsql para acceso a datos). Los **rutas y nombres de endpoints** están en **español** (por ejemplo `api/categorias`, `interno/ordenes/validar-y-calcular`).

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/) (servidor accesible en red o localhost)
- [Node.js 20+](https://nodejs.org/) y npm (para el frontend Angular)

## Puesta en marcha (localhost)

### 1) Base de datos y semilla

1. Cree en PostgreSQL una base vacía (y un usuario con permisos sobre ella) acorde a `ConnectionStrings:BaseDatos` en `src/CatalogoOrdenes.Api/appsettings.json` (por defecto: base `catalogo_ordenes`, usuario y contraseña `catalogo` en `127.0.0.1:5432`). Ejemplo en `psql` como superusuario:

   ```sql
   CREATE USER catalogo WITH PASSWORD 'catalogo';
   CREATE DATABASE catalogo_ordenes OWNER catalogo;
   ```

2. Cadena de conexión: edite `ConnectionStrings:BaseDatos` en la API y `ConnectionStrings:Lectura` en el servicio interno (misma base que la API, solo lectura de productos). Opcionalmente puede usar la variable de entorno `CATALOGO_CADENA_POSTGRES` (misma cadena para ambos) o `CATALOGO_CADENA_POSTGRES_LECTURA` solo para el interno.

Al iniciar la API se aplican las migraciones SQL con **[Evolve](https://github.com/lecaillon/Evolve)** (misma cadena PostgreSQL que EF Core) desde `src/CatalogoOrdenes.Api/db/migrations/`, incluyendo la semilla de datos de ejemplo para usuarios, roles, categorías y productos. El servicio interno no ejecuta Evolve; usa la misma base ya migrada. **Arranque la API (paso 2) al menos una vez** antes de usar el interno con datos reales. Si cambió de un esquema antiguo (`EnsureCreated` / SQLite) y hay conflictos, use una base vacía o elimine las tablas antes de volver a levantar la API.

### 2) API principal (puerto 5080)

En una terminal:

```bash
cd GestionCatalogoOrdenes/src/CatalogoOrdenes.Api
dotnet run
```

Swagger (desarrollo): `http://localhost:5080/swagger`

### 3) Servicio interno (puerto 5081)

En otra terminal:

```bash
cd GestionCatalogoOrdenes/src/CatalogoOrdenes.ServicioInterno
dotnet run
```

Debe quedar escuchando en `http://localhost:5081`. Protección: cabecera `X-Clave-Interna` (valor por defecto alineado con la API, configurable por `appsettings` / variables de entorno).

### 4) Frontend Angular (puerto 4200)

```bash
cd GestionCatalogoOrdenes/frontend/catalogo-ordenes-ui
npm install
npm start
```

Abrir `http://localhost:4200`. El front **solo** llama a `http://localhost:5080` (configurable en `src/environments/environment.ts`).

> **Importante:** para **registrar órdenes** debe estar en marcha el **servicio interno**; la API lo invoca por HTTP antes de persistir.

## Usuarios de demostración

| Rol           | Usuario               | Contraseña     |
| ------------- | --------------------- | -------------- |
| Administrador | `admin@demo.local`    | `Admin123!`    |
| Operador      | `operador@demo.local` | `Operador123!` |
| Consulta      | `consulta@demo.local` | `Consulta123!` |

## Pruebas backend

```bash
cd GestionCatalogoOrdenes
dotnet test
```

## Seguridad y secretos

- **JWT**: configure `Jwt:ClaveFirma` (mínimo 32 caracteres recomendado) mediante **variables de entorno** o **User Secrets** en entornos reales.
- **PostgreSQL**: no deje credenciales de producción en `appsettings.json`; use **User Secrets** o variables (`ConnectionStrings__BaseDatos`, etc.).
- **Servicio interno**: `ServicioInternoOrdenes:ValorClave` y cabecera `X-Clave-Interna` deben coincidir entre API e interno.

## Documentación adicional

- `docs/ADR.md` — decisiones de arquitectura.
- `docs/USO_IA.md` — transparencia sobre uso de IA.
- `catalogo.api.http` — ejemplos de llamadas HTTP.

## Flujo principal

1. El usuario inicia sesión en el front (JWT en `localStorage`).
2. Administra categorías/productos (roles con permisos acotados).
3. Operador/Administrador registra una orden con ítems.
4. La API valida entrada, llama al **servicio interno** para validar stock/cantidades y **calcular totales**.
5. Si el interno aprueba, la API persiste orden + detalle y descuenta stock en transacción.
