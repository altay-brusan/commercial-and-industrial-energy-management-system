# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
# Build
dotnet build -tl

# Run (with hot reload)
cd src/Web && dotnet watch run
# API available at https://localhost:5001

# Run all tests
dotnet test

# Run single test
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"

# Run specific test project
dotnet test ./tests/Application.FunctionalTests/Application.FunctionalTests.csproj
```

## Database Migrations

```bash
# Add migration
dotnet ef migrations add MigrationName --project src/Infrastructure --startup-project src/Web

# Update database
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

## Code Scaffolding

From `src/Application/` directory:
```bash
# New command
dotnet new ca-usecase --name CreateEntity --feature-name Entities --usecase-type command --return-type int

# New query
dotnet new ca-usecase -n GetEntities -fn Entities -ut query -rt EntitiesVm
```

If template not found: `dotnet new install Clean.Architecture.Solution.Template::9.0.8`

## Architecture Overview

Clean Architecture solution with CQRS pattern (MediatR) for a multi-tenant energy management system (DERMS). .NET 9, PostgreSQL, Minimal APIs.

```
src/
├── Domain/          # Entities, events, value objects (no dependencies)
├── Application/     # Commands, queries, validators, interfaces
├── Infrastructure/  # EF Core, Identity, external services, background jobs
├── Web/             # Minimal API endpoints, middleware, SignalR hubs
└── Proto/           # gRPC/Protobuf definitions (defined but currently disabled in DI)
```

**Layer Dependencies:** Web → Application + Infrastructure → Domain

**Build Configuration:** `Directory.Build.props` sets `TreatWarningsAsErrors=true` across all projects. All NuGet package versions are managed centrally in `Directory.Packages.props` — never put a version in a `.csproj`.

## Key Patterns & Conventions

### CQRS Structure
Commands and queries live in `Application/{Feature}/Commands/` and `Application/{Feature}/Queries/`. Each has:
- **Command/query record + handler in the same `.cs` file** (e.g., `CreateFacility.cs` contains both `CreateFacilityCommand` and `CreateFacilityCommandHandler`)
- Validator in a separate file in the same folder (e.g., `CreateFacilityCommandValidator.cs`)

### Entity Inheritance
```
BaseEntity (Id: Guid, DomainEvents)
  └── BaseAuditableEntity (Created, CreatedBy, LastModified, LastModifiedBy)
        └── All business entities

DeviceDataBase (Id: long — separate hierarchy for time-series telemetry)
  └── DeviceBatteryData, DeviceSolarData, DevicePcsData, DeviceSmartMeterData
```

Device telemetry entities use `long Id` (not Guid) for high-volume time-series data. They store a `Kind` discriminator string and a `Payload` JSON blob (PostgreSQL JSONB) rather than typed columns. Don't use `FindAsync<T>(Guid)` patterns with these.

### Endpoint Pattern
Endpoints inherit `EndpointGroupBase` and use Minimal APIs. **No manual registration needed** — `WebApplicationExtensions.MapEndpoints()` uses reflection to discover and instantiate all `EndpointGroupBase` subclasses automatically.

`MapGroup(this)` auto-derives the URL from the class name (`Facilities` → `/api/Facilities`):
```csharp
public class Entities : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetEntity, "{id}")
            .MapPost(CreateEntity);
    }
}
```

**Exception:** The Auth endpoint (`Auth.cs`) directly uses `UserManager`, `SignInManager`, `IJwtTokenService`, and `ApplicationDbContext` — it does not go through MediatR. The DeviceData ingest endpoint uses a custom route (`/api/v1/DeviceData/Ingest`) with `ApiKeyEndpointFilter` instead of `RequireAuthorization()`.

### Two-Level Authorization
Authorization goes on the **command/query record**, not on the endpoint. Endpoints just call `.RequireAuthorization()` at the group level.

1. **Role check** via `[Authorize(Roles = "Administrator,Tenant.Admin")]` — Identity roles
2. **Permission check** via `[RequirePermission(Permissions.FacilityCreate)]` — feature-based RBAC with dot-notation strings like `"Facility.Create"`, `"Scada.Control"`

Both are enforced by `AuthorizationBehaviour` in the MediatR pipeline. Administrators bypass all permission checks. TenantAdmins bypass within their plan's features.

### Multi-Tenancy
- Entities implement `IMultiTenantEntity` with `TenantId` property
- `MultiTenantMiddleware` extracts `tenant_id` claim from JWT on every request
- Admins can impersonate tenants via `X-Selected-Tenant-Id` request header
- `MultiTenantService` resolves effective tenant: regular users are always scoped; admins without the header see all data
- **EF Core global query filters** in `ApplicationDbContext` auto-scope data by tenant (set at DbContext construction time). You never need to manually filter by tenant. **Warning:** calling `IgnoreQueryFilters()` bypasses tenant isolation.
- Tenant scoping chains through navigation properties (e.g., Device → Rtu → Facility → Firm → TenantId)
- Global entities (no tenant filter): `Tariff`, `MarketOperator`, `DayAheadNetEnergyPriceHistory`, `Plan`
- For mutations, `GetTenantIdForCreation(requestedTenantId?)` resolves: regular user → own tenant; admin → header > request body > error

### Domain Events
Add events in entity methods or handlers, dispatched automatically by `DispatchDomainEventsInterceptor` on `SaveChangesAsync`. **Events fire before the DB transaction commits** (within `SavingChangesAsync`, before `base.SavingChangesAsync`).
```csharp
entity.AddDomainEvent(new EntityCreatedEvent(entity));
```

## Pipeline Behaviors (Execution Order)

1. `UnhandledExceptionBehaviour` — Exception logging
2. `AuthorizationBehaviour` — Role/policy checks from `[Authorize]` + permission checks from `[RequirePermission]`
3. `ValidationBehaviour` — FluentValidation execution (all validators run concurrently)
4. `PerformanceBehaviour` — Logs warning if request exceeds 500ms

## Authentication & API Security

- **User auth**: JWT Bearer tokens (HS256, 15-min expiry) with refresh token rotation (`RefreshToken` entity stores SHA-256 hash). Refresh token set as `HttpOnly; Secure; SameSite=Strict` cookie.
- **Device data ingestion**: API key auth via `ApiKeyEndpointFilter` checking `X-API-Key` header against `IngestSettings:ApiKey` config
- **Rate limiting**: `auth-login` (5 req/15min), `auth-forgot-password` (3 req/15min). Relaxed in Test environment.
- **Default dev credentials**: `administrator@localhost` / `***REMOVED***` and `admin@oklidis.com.tr` / `Admin@123`
- **Exception handling**: `CustomExceptionHandler` maps `ValidationException` → 400, `NotFoundException` → 404, `UnauthorizedAccessException` → 401, `ForbiddenAccessException` → 403

## MQTT / Device Data Ingestion

There is no direct MQTT client. EMQX broker is configured with webhook rules that POST to this API:

- `POST /api/v1/DeviceData/Ingest` — single message
- `POST /api/v1/DeviceData/Ingest/Batch` — batch of messages

**Ingest flow** (`IngestDeviceDataHandler`):
1. `DeviceDataCategorizer.Categorize(kind)` maps the `Kind` string prefix to a `DeviceDataCategory` enum (`ContainerBattery*`/`Chiller*`/`HVCU*` → Bess, `Pcs*` → Pcs, `Solar*`/`Pv*`/`Irradiance*` → Solar, `Meter*`/`Grid*`/`SmartMeter*` → SmartMeter)
2. Looks up `Rtu` by `RtuSerialNumber` and `Device` by `SerialNumber` (both 5-min memory cached)
3. Creates the typed entity (`DeviceBatteryData`/`DevicePcsData`/`DeviceSolarData`/`DeviceSmartMeterData`)
4. Updates `LastSeenUtc` on both `Rtu` and `Device` as side effects

## Background Jobs

Hangfire with PostgreSQL storage (own `hangfire` schema). Dashboard at `/hangfire/dashboard`. Disabled in Test environment.

- `EnergyPriceFetchWorker` runs twice daily (00:01 and 13:01 UTC), fetches day-ahead energy prices from ENTSO-E (XML API) and EPIAS (Turkish market, CAS ticket auth) for all active `FirmMarketOperator` configurations.

## Key Infrastructure Services

| Interface | Implementation | Notes |
|---|---|---|
| `IApplicationDbContext` | `ApplicationDbContext` | EF Core DbSets with tenant-scoped query filters |
| `IMultiTenantService` | `MultiTenantService` | Tenant resolution (scoped) |
| `IJwtTokenService` | `JwtTokenService` | JWT generation/validation |
| `IPermissionService` | `PermissionService` | Feature-based permissions |
| `IEnergyPriceService` | `EnergyPriceService` | ENTSO-E + EPIAS integration (typed HttpClient) |
| `IVirtualClockService` | `VirtualClockService` | Virtual clock for SCADA time control (singleton) |
| `IEmailService` | `EmailService` | SMTP via MailKit (singleton) |
| `IRealTimeNotifier` | `SignalRRealTimeNotifier` | `LiveDataHub` at `/liveDataHub` (singleton) |

## Database

- **Production:** PostgreSQL via Npgsql
- **Testing:** In-memory SQLite (with `DateTimeOffset` → ISO string conversions for compatibility)
- **ORM:** Entity Framework Core 9.0
- **Interceptors:** `AuditableEntityInterceptor` (auto-sets audit fields + logs admin impersonation), `DispatchDomainEventsInterceptor` (dispatches domain events before commit)

## Configuration Sections

| Section | Purpose |
|---|---|
| `ConnectionStrings:BackendDb` | PostgreSQL connection string (also used by Hangfire) |
| `JwtSettings` | Secret, Issuer (`DERMS-Backend`), Audience (`DERMS-Frontend`), token expiry |
| `IngestSettings:ApiKey` | API key for EMQX webhook data ingestion |
| `EmailSettings` | SMTP server, port, credentials |
| `Serilog` | Structured logging to PostgreSQL `logs` table + Console |
| `AZURE_KEY_VAULT_ENDPOINT` | Env var; if set, loads secrets from Azure Key Vault |

## Testing

- **Framework:** NUnit 4 with FluentAssertions and Moq
- **Functional tests** use `CustomWebApplicationFactory` with in-memory SQLite. Mocks `IUser` and `IMultiTenantService` (wired as super admin, unscoped).
- **Test utilities in `Testing.cs`:**
  - `SendAsync(command)` — sends through the full MediatR pipeline (behaviors + handler)
  - `AddAsync(entity)` — direct DB insert bypassing pipeline (use for test setup)
  - `FindAsync<T>(keys)` — direct DB lookup by primary key
  - `RunAsAdministratorAsync()` / `RunAsDefaultUserAsync()` — creates Identity users with roles
  - `ResetState()` — called automatically in `[SetUp]` via `BaseTestFixture` (deletes all rows, fast)
- All test classes inherit `BaseTestFixture`
- CI runs only unit tests (`--filter "UnitTests"`); functional tests are skipped in CI

## Key Domain Entities

- **Tenant/Firm/Facility** — Multi-tenant organizational hierarchy
- **Device/DeviceType** — IoT devices under RTUs, with self-referential parent/child, JSON `Ports`/`Metadata`/`Specifications`
- **Rtu** — Remote Terminal Units per facility (table: `FacilityRemoteTerminalUnits`). Devices belong to RTUs, not directly to facilities.
- **DeviceLink** — Port-to-port connections between devices within a facility
- **DeviceAlarm** — Device fault/warning tracking with severity and recovery
- **DeviceDataBase** subclasses (Battery, Solar, PCS, SmartMeter) — Time-series telemetry with JSON payloads
- **MarketOperator/FirmMarketOperator/DayAheadNetEnergyPriceHistory** — Energy market price integration
- **Tariff/FacilityTariff** — Energy tariff assignment to facilities
- **TenantRole/TenantRolePermission/Feature/Permission/Plan** — RBAC and subscription management

## Useful References

- `docs/` — Detailed docs on authentication, MQTT ingestion, and tariffs/energy prices
- `scripts/` — `reset_database.sql` and `seed_market_operators.sql`

## Deployment

- **Jenkins**: Kubernetes-native pipeline → Docker build → push to private registry (`REDACTED_HOST:30500`) → `kubectl` deploy
- **Environments**: `stg` branch → `cni-stg` namespace; other branches → `cni-dev` namespace
- **Dockerfile**: Multi-stage (SDK 9.0 build → ASP.NET 9.0 runtime), non-root user, port 8080, health check at `/health`
