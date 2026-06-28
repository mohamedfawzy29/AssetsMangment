# Asset Management System

A REST API for managing internet-facing assets as part of an Attack Surface Monitoring (ASM) platform. Built with ASP.NET Core 10 and PostgreSQL.

> **Note on stack deviation:** The task suggested Python · FastAPI · PostgreSQL. This implementation uses C# · ASP.NET Core 10 · PostgreSQL instead, as it better reflects my primary backend expertise. The API design, data model, and all mandatory requirements are fully implemented.

---

## Quick Start

### Prerequisites
- [Docker](https://www.docker.com/products/docker-desktop) and Docker Compose

### Run with a single command

```bash
git clone https://github.com/mohamedfawzy29/AssetsMangment.git
cd AssetsMangment
docker compose up --build
```

The API will be available at `http://localhost:8080`

API docs (Scalar UI): `http://localhost:8080/scalar/v1`

---

## Environment Variables

All configuration is passed via `docker-compose.yml`. For local development outside Docker, create `appsettings.Development.json`:

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | `Host=db;Port=5432;Database=AssetManagementDB;Username=postgres;Password=2929` |
| `Jwt__Key` | Secret key for JWT signing | See `appsettings.json` |
| `Jwt__Issuer` | JWT issuer | `AssetsManagementAPI` |
| `Jwt__Audience` | JWT audience | `AssetsManagementAPIUsers` |
| `Jwt__ExpireMinutes` | Token expiry in minutes | `60` |

> **Security note:** The JWT key and DB password in this repo are for development only. In production, inject secrets via environment variables and never commit them.

---

## API Documentation

Interactive docs are auto-generated and available at:

**`http://localhost:8080/scalar/v1`**

### Authentication

Write operations (POST, PUT, DELETE, PATCH) require a JWT token.

**1. Register:**
```http
POST /api/auth/register
Content-Type: application/json

{
  "userName": "admin",
  "password": "yourpassword"
}
```

**2. Login to get token:**
```http
POST /api/auth/login
Content-Type: application/json

{
  "userName": "admin",
  "password": "yourpassword"
}
```

**3. Use token in requests:**
```http
Authorization: Bearer <your_token>
```

### Endpoints Summary

#### Assets (`/api/assets`)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/assets` | No | List assets with filtering, sorting, pagination |
| POST | `/api/assets` | Yes | Create or update an asset |
| GET | `/api/assets/{id}` | No | Get asset by ID |
| PUT | `/api/assets/{id}` | Yes | Update asset |
| DELETE | `/api/assets/{id}` | Yes | Delete asset |
| PATCH | `/api/assets/{id}/stale` | Yes | Mark asset as stale |
| GET | `/api/assets/{id}/graph` | No | Get asset with its relationships |
| POST | `/api/assets/bulk` | Yes | Bulk import assets |

#### Query Parameters for GET `/api/assets`
| Parameter | Type | Description |
|-----------|------|-------------|
| `type` | enum | Filter by asset type: `domain`, `subdomain`, `ip_address`, `service`, `certificate`, `technology` |
| `status` | enum | Filter by status: `active`, `stale`, `archived` |
| `source` | enum | Filter by source: `import`, `scan`, `manual` |
| `tag` | string | Filter by tag |
| `search` | string | Search by value (contains) |
| `sortBy` | string | Sort by: `value`, `firstSeen`, `lastSeen`, `status` |
| `descending` | bool | Sort descending (default: false) |
| `page` | int | Page number (default: 1) |
| `pageSize` | int | Page size (default: 10) |

#### Relationships (`/api/assetrelationships`)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/assetrelationships` | No | List relationships |
| POST | `/api/assetrelationships` | Yes | Create relationship |
| GET | `/api/assetrelationships/{id}` | No | Get relationship by ID |
| DELETE | `/api/assetrelationships/{id}` | Yes | Delete relationship |
| GET | `/api/assetrelationships/asset/{assetId}` | No | Get all relationships for an asset |
| POST | `/api/assetrelationships/bulk` | Yes | Bulk create relationships |

---

## Design Decisions & Assumptions

### Deduplication
Assets are deduplicated by `(type, normalizedValue)`. On re-import, `last_seen` is updated, tags are merged (union), and metadata keys are merged (new values overwrite existing ones for the same key). This handles the case where the same asset appears from multiple sources with different metadata.

### Value Normalization
Asset values are normalized before storage (trimmed, lowercased where applicable) to prevent duplicates from case differences or whitespace.

### Re-appearing Assets
A stale asset that is re-imported automatically returns to `active` status with an updated `last_seen`.

### Bulk Import
The bulk import endpoint processes assets individually and reports `imported`, `duplicates`, and `failed` counts. A failure on one asset does not abort the rest of the batch.

### Relationships
Relationships are directional (source → target) with a `type` enum. The graph endpoint returns all incoming and outgoing relationships for an asset with direction indicated.

### Authentication
JWT-based authentication is used for all write operations. Read operations are public. Token expiry defaults to 60 minutes.

### Migrations
Database migrations run automatically on startup via `db.Database.Migrate()`. No manual migration steps are required.

### Pagination
Default page size is 10. All list endpoints support `page` and `pageSize` parameters to prevent large payloads.

---

## Running the Tests

```bash
cd AssetsManagement.Tests
dotnet test
```

Or with Docker:

```bash
docker compose run --rm api dotnet test AssetsManagement.Tests/AssetsManagement.Tests.csproj
```

---

## Project Structure

```
AssetsMangment/
├── Controllers/         # API controllers
├── Data/                # DbContext and EF Core configuration
├── DTOs/                # Request and response models
│   ├── Request/
│   └── Response/
├── Migrations/          # EF Core database migrations
├── Models/              # Domain models (Asset, AssetRelationship, User)
├── Services/            # Business logic
├── Utilities/           # Helpers (JWT, asset normalization, OpenAPI config)
├── Program.cs           # App startup and DI configuration
└── appsettings.json     # Configuration

AssetsManagement.Tests/  # Unit and integration tests
Dockerfile               # Multi-stage Docker build
docker-compose.yml       # API + PostgreSQL setup
```

---

## Useful Commands

```bash
# Start everything
docker compose up --build

# Stop and remove containers (keep data)
docker compose down

# Stop and remove containers + database data
docker compose down -v

# View logs
docker compose logs api
docker compose logs db

# Rebuild after code changes
docker compose down
docker compose build --no-cache
docker compose up
```
