[![CI](https://github.com/DaWeba02/servicestarter-vs/actions/workflows/ci.yml/badge.svg)](https://github.com/DaWeba02/servicestarter-vs/actions/workflows/ci.yml)

# ServiceStarter.VS (.NET 8 Minimal API)

Starter template with vertical-slice Minimal API, JWT auth, EF Core + SQL Server, Serilog, FluentValidation, ProblemDetails, health checks, Swagger (Dev), and Testcontainers-based integration tests—ready for local or Docker workflows.

## What’s included
- Minimal API slices (`Features/Auth/Login`, `Features/Users/Create`)
- JWT bearer auth with Admin/User roles; AdminOnly policy
- EF Core + SQL Server migrations; Serilog request logging; global ProblemDetails; health checks; Swagger in Development
- Unit and integration tests (Testcontainers SQL Server)

## Quickstart: Local
- Prereqs: .NET 8 SDK; SQL reachable (adjust `ConnectionStrings:Default` in `src/ServiceStarter.Api/appsettings.json` if needed).
- Run: `dotnet restore && dotnet run --project src/ServiceStarter.Api`
- URLs: API `http://localhost:5171`, Swagger `http://localhost:5171/swagger`, ping `GET /ping`, health `GET /health/live` and `/health/ready`.

## Quickstart: Docker
- `docker compose up --build`
- URLs: API `http://localhost:8080`, Swagger `http://localhost:8080/swagger` (Dev), DB `localhost:1433` (password `Your_password123`). Uses the bundled SQL container.

## Admin Token
- `POST /auth/login` with `{"email":"someone@admin.local","password":"password123"}` → `{ "accessToken": "..." }`.
- Any `@admin.local` email gets Admin role; other domains get User role.

## Migrations strategy
- Development / Testing: migrations apply automatically on startup.
- Production: opt-in by setting `Database:ApplyMigrationsOnStartup=true` (or equivalent env var); default is off.

## Add a Feature Slice
- Folder: `src/ServiceStarter.Api/Features/<Area>/<Action>/`.
- Include: `<Action>Request`, `<Action>RequestValidator`, `<Action>Response`, `<Action>Endpoints` (route mapping), plus handlers/services as needed.
- Use `ValidationFilter<T>` on endpoints, return ProblemDetails for errors, add migrations when data changes, and cover with unit + integration tests.

## Tests
- Unit: `dotnet test tests/ServiceStarter.UnitTests`
- Integration (Docker daemon required): `dotnet test tests/ServiceStarter.IntegrationTests` (WebApplicationFactory + Testcontainers SQL Server `mcr.microsoft.com/mssql/server:2022-latest`).
- Full suite: `dotnet test -c Release`

## Renaming the template
- Rename solution/project folders and `*.csproj` files; update project references.
- Replace namespaces from `ServiceStarter` to your name (src + tests).
- Update docker image/tag names and compose references; adjust connection strings and JWT settings.
- Verify with `dotnet test -c Release`.

## Project Layout
- `src/ServiceStarter.Api` — app, slices, infrastructure, migrations.
- `tests/ServiceStarter.UnitTests` — validators and small units.
- `tests/ServiceStarter.IntegrationTests` — WebApplicationFactory + Testcontainers (auth, users, health, swagger, problem details).
- `docker/` — Dockerfile, `docker-compose.yml`; `docs/` — reference material.
