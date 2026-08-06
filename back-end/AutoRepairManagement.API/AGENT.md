# AGENT.md

Instructions for AI coding agents (Claude Code, Codex, Copilot, Cursor, etc.) working in this repository.

## Project overview

AutoRepairManagement.API is an ASP.NET Core Web API (.NET 10) for managing an auto repair shop. The project is in an early/skeleton stage: only the default template wiring (EF Core + SQLite, FluentValidation, OpenAPI) is in place — no domain entities, controllers, or endpoints have been added yet.

## Tech stack

- **.NET 10** / ASP.NET Core Web API (`Microsoft.NET.Sdk.Web`)
- **Entity Framework Core 10** with **SQLite** (`Data/AppDbContext.cs`, connection string in `appsettings.json`)
- **FluentValidation** (registered via `AddValidatorsFromAssemblyContaining<Program>()`)
- **OpenAPI** (`Microsoft.AspNetCore.OpenApi`, exposed only in Development via `MapOpenApi()`)
- Nullable reference types and implicit usings are enabled project-wide

## Project structure

- `Program.cs` — minimal hosting/startup: service registration and HTTP pipeline
- `Data/AppDbContext.cs` — EF Core `DbContext`; add `DbSet<T>` properties here as entities are introduced
- `appsettings.json` / `appsettings.Development.json` — configuration, including `ConnectionStrings:DefaultConnection`
- `Properties/launchSettings.json` — local run profiles (`http` → `http://localhost:5044`, `https` → `https://localhost:7130`)
- `AutoRepairManagement.API.http` — manual HTTP request scratchpad for the Rider/VS Code REST client
- `Dockerfile` / `compose.yaml` — multi-stage container build (SDK image builds/publishes, ASP.NET image runs)

## Build, run, test

```bash
dotnet restore
dotnet build
dotnet run                 # uses Properties/launchSettings.json profiles
dotnet test                 # no test project exists yet — create one before adding tests
```

Docker:

```bash
docker compose up --build
```

## Conventions for changes

- Keep `Nullable` and `ImplicitUsings` enabled; don't disable them to silence warnings — fix the nullability issue instead.
- New EF Core entities get a `DbSet<T>` on `AppDbContext` and a migration (`dotnet ef migrations add <Name>`); the `Microsoft.EntityFrameworkCore.Design` package is already referenced for this.
- Validate incoming request models with FluentValidation validators rather than manual `if` checks, consistent with the validator registration already in `Program.cs`.
- Favor minimal API endpoints or controllers consistent with whatever pattern is first established in `Program.cs` — don't mix both styles without reason.
- There is no test project yet. When adding one, follow standard .NET testing conventions (xUnit is the common default for ASP.NET Core projects) and wire it into the solution file.

## Out of scope / do not assume

- No authentication/authorization is configured yet — don't assume an auth scheme exists.
- No domain entities (vehicles, work orders, customers, etc.) exist yet — check `Data/AppDbContext.cs` before assuming a model is present.
