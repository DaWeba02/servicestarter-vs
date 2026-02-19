# How to Start the Service Starter

## Prerequisites
- .NET 8 SDK
- Docker (for SQL Server locally, Testcontainers, and docker compose)
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

## Run locally
1. `dotnet restore`
2. Update `ConnectionStrings:Default` in `src/ServiceStarter.Api/appsettings.json` if needed (SQL reachable).
3. Start API: `dotnet run --project src/ServiceStarter.Api` (migrations auto-apply in Development/Testing or when `Database:ApplyMigrationsOnStartup=true`).
4. Swagger UI: `http://localhost:5171/swagger`; ping `/ping`; health `/health/live` and `/health/ready`.

## Run with Docker
1. `docker compose up --build`
2. API `http://localhost:8080`; Swagger `http://localhost:8080/swagger`; SQL Server `localhost:1433` (SA password `Your_password123`).

## Migration strategy
- Dev/Testing: migrations run on startup by default.
- Production: opt-in by setting `Database:ApplyMigrationsOnStartup=true` (config or env var).

## Add a new feature slice
1. Create folder `src/ServiceStarter.Api/Features/<Area>/<Feature>/`.
2. Add `Request`, `Response`, `Validator`, and `Endpoints` (route mapping) plus handler/service as needed.
3. Ensure `Map*Endpoints` is invoked from `Program.cs`; use `ValidationFilter<T>` and ProblemDetails responses.
4. Add unit tests (validators) and integration tests when persistence/auth is involved; add EF migration if data changes.

## Run tests
- Unit: `dotnet test tests/ServiceStarter.UnitTests`
- Integration (Docker required): `dotnet test tests/ServiceStarter.IntegrationTests` (spins up Testcontainers SQL Server).
- Full suite: `dotnet test -c Release`

## Renaming the template
1. Rename solution and folders: update `ServiceStarter.VS.sln`, root folder, and `src/ServiceStarter.Api` / `tests/*` project folders to your new name.
2. Update project files: change `<AssemblyName>`/`<RootNamespace>` if desired in `*.csproj`; rename `ServiceStarter.Api.csproj` and adjust references in both test projects.
3. Namespace swap: search/replace `ServiceStarter` with your new root namespace in `src` and `tests`.
4. Docker image/tag: adjust `docker/Dockerfile` image name (if referenced) and any tags in `docker-compose.yml`.
5. Configuration: update connection strings and `Jwt` settings in `appsettings*.json` and compose environment vars.
6. Verify: run `dotnet test -c Release` (requires Docker for integration tests).
