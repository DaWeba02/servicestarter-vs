# C4 Container Diagram

```mermaid
flowchart LR
    client[Client / Browser] --> api[ServiceStarter API\n.NET 8 Minimal API];
    api --> db[(SQL Server\nEF Core + Migrations)];
    api -->|JWT| auth[Auth - JWT bearer];
    api --> logs[(Serilog Sinks)];

    subgraph Docker Compose
        api
        db
    end
```
