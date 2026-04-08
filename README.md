# System Design Playground (SDP)

System Design Playground is a single-user architecture modeling and simulation tool for reasoning about microservice designs before writing production code.

## What it does
- Create projects with service nodes, dependency links, and traffic scenarios
- Simulate deterministic load propagation for a scenario
- Compute per-service utilization and bottlenecks
- Compute path-level total latency
- Visualize topology in a Blazor graph editor and highlight bottlenecks

## Architecture
- `src/Domain`: entities and enums
- `src/Application`: DTOs, contracts, and simulation service
- `src/Infrastructure`: EF Core + PostgreSQL persistence and repositories
- `src/Api`: ASP.NET Core Web API + validation + rate limiting + problem details
- `src/Web`: Blazor Server UI
- `tests/Unit`: simulation invariants
- `tests/Integration`: API lifecycle and simulation integration tests

## Simulation model limits
This project is intentionally deterministic and heuristic.

Non-goals:
- packet/network realism
- Kubernetes-level infra simulation
- auto-scaling behavior
- advanced queueing-theory analysis

## Local run (Docker)
```bash
docker compose up --build
```

Apps:
- Web UI: `http://localhost:8081`
- API Swagger: `http://localhost:8080/swagger`

## Local run (.NET SDK)
Prerequisites:
- .NET 8 SDK
- PostgreSQL 16+

```bash
dotnet restore SDP.sln
dotnet build SDP.sln
dotnet test tests/Unit/SDP.UnitTests.csproj
dotnet test tests/Integration/SDP.IntegrationTests.csproj
```

## Migrations
```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/Api
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

## Demo walkthrough
1. Create/open a project.
2. Add 5-10 services with capacities and base latency.
3. Add directed links.
4. Create a scenario with entry service and incoming RPS.
5. Run simulation and inspect service table + path latency + bottleneck highlights.
