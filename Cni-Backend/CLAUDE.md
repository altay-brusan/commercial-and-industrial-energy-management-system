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

## Code Scaffolding

From `src/Application/` directory:
```bash
# New command
dotnet new ca-usecase --name CreateEntity --feature-name Entities --usecase-type command --return-type int

# New query
dotnet new ca-usecase -n GetEntities -fn Entities -ut query -rt EntitiesVm
```

If template not found: `dotnet new install Clean.Architecture.Solution.Template::9.0.8`

## Database Migrations

```bash
# Add migration
dotnet ef migrations add MigrationName --project src/Infrastructure --startup-project src/Web

# Update database
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

## Architecture Overview

Clean Architecture solution with CQRS pattern (MediatR) for a multi-tenant energy management system (DERMS).

```
src/
├── Domain/          # Entities, events, value objects (no dependencies)
├── Application/     # Commands, queries, validators, interfaces
├── Infrastructure/  # EF Core, Identity, external services
├── Web/             # Minimal API endpoints, middleware
└── Proto/           # gRPC/Protobuf definitions
```

**Layer Dependencies:** Web → Application + Infrastructure → Domain

## Key Patterns

### CQRS Structure
Commands and queries are organized by feature in `Application/{Feature}/Commands/` and `Application/{Feature}/Queries/`. Each command/query has:
- Record class implementing `IRequest<T>`
- Handler implementing `IRequestHandler<TRequest, TResponse>`
- Validator (optional) implementing `AbstractValidator<T>`

### Entity Inheritance
```
BaseEntity (Id, DomainEvents)
  └── BaseAuditableEntity (Created, CreatedBy, LastModified, LastModifiedBy)
        └── All business entities
```

### Endpoint Pattern
Endpoints inherit from `EndpointGroupBase` and use Minimal APIs:
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

### Authorization
Apply `[Authorize]` attributes directly on command/query records:
```csharp
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record CreateEntityCommand : IRequest<int> { }
```

### Multi-Tenancy
- Entities implement `IMultiTenantEntity` with `TenantId` property
- Query filters in `ApplicationDbContext` auto-scope data by tenant
- `IMultiTenantService` provides current tenant context

### Domain Events
Add events in entity methods, dispatched automatically on `SaveChangesAsync`:
```csharp
entity.AddDomainEvent(new EntityCreatedEvent(entity));
```

## Pipeline Behaviors (Execution Order)

1. `UnhandledExceptionBehaviour` - Exception logging
2. `AuthorizationBehaviour` - Role/policy checks from `[Authorize]`
3. `ValidationBehaviour` - FluentValidation execution
4. `PerformanceBehaviour` - Long-running request logging

## Database

- **Production:** PostgreSQL via Npgsql
- **Testing:** In-memory SQLite
- **ORM:** Entity Framework Core 9.0

## Testing

- **Framework:** NUnit with FluentAssertions and Moq
- **Functional tests** use `CustomWebApplicationFactory` with in-memory SQLite
- Test utilities in `Testing.cs`: `SendAsync()`, `RunAsDefaultUserAsync()`, `FindAsync()`

## Key Domain Entities

- **Tenant/Firm/Facility** - Multi-tenant organizational hierarchy
- **Device/DeviceType** - IoT devices at facilities
- **TopologyNode/TopologyEdge** - Facility electrical topology graphs
- **App*Data** (Solar, BESS, PCS, SmartMeter) - SCADA telemetry data
