# Stack Reference

## Language & Runtime
- **C# 13 / .NET 10** (`net10.0`). SDK pinned to `10.0.0` with `rollForward: latestFeature` in `global.json`.

## Solution Structure
The solution uses the new `.slnx` format (`eShopOnWeb.slnx`). Central Package Management is enabled via `Directory.Packages.props` — all NuGet versions are declared there, never in individual `.csproj` files.

## Frameworks
| Layer | Framework |
|---|---|
| Web UI | ASP.NET Core 10 — Razor Pages + MVC |
| Admin UI | Blazor WebAssembly (`BlazorAdmin` + `BlazorShared`) |
| REST API | FastEndpoints 8.x (in `PublicApi`) |
| ORM | Entity Framework Core 10 (SQL Server for prod; InMemory for tests) |
| Identity | ASP.NET Core Identity + EF Core |
| Orchestration | .NET Aspire 13.x (`eShopWeb.AppHost`) |

## Build Tool
- **`dotnet build`** / **`dotnet publish`** (no custom build scripts).
- CI uses standard `dotnet` CLI commands.
- Docker support via `docker-compose.yml` and `.dcproj`.
- Azure deployment via Azure Developer CLI (`azure.yaml`).

## Test Runner
- **`dotnet test`** — runs all five test projects.
- Code coverage collected via `coverlet.collector`; settings in `CodeCoverage.runsettings`.

| Project | Framework | Scope |
|---|---|---|
| `UnitTests` | xunit.v3 + NSubstitute | ApplicationCore & Web units |
| `IntegrationTests` | xunit.v3 + NSubstitute | EF Core / Infrastructure |
| `FunctionalTests` | xunit.v3 + Mvc.Testing | In-process Web & API |
| `PublicApiIntegrationTests` | MSTest + Mvc.Testing | PublicApi in-process |
| `EndToEndTests` | xunit.v3 + Playwright | Full browser E2E |

## Key Libraries (ApplicationCore)
- **Ardalis.Specification 9.x** — repository specifications pattern
- **Ardalis.GuardClauses 5.x** — guard clauses
- **Ardalis.Result 10.x** — operation result type
- **Mediator 3.x** (source-generator based, not MediatR) — CQRS in Web layer
- **FluentValidation 12.x** — in `BlazorShared` for shared validators
- **NimblePros.SharedKernel 3.x** — base entity and domain event types

## Observability
Service defaults in `eShopWeb.AspireServiceDefaults`:
- OpenTelemetry (traces, metrics, logs) exported via OTLP and Prometheus
- Structured logging to **Seq** (via Aspire in local dev)
- HTTP resilience via `Microsoft.Extensions.Http.Resilience`

## Architectural Constraint — Clean Architecture
```
[Web / PublicApi / BlazorAdmin]
        ↓ depends on
  [ApplicationCore + BlazorShared]
        ↑ implements
    [Infrastructure]
```
- `ApplicationCore` has **zero** references to ASP.NET Core, EF Core, or Infrastructure.
- `Infrastructure` implements interfaces declared in `ApplicationCore`; it is wired up at startup in `Web`/`PublicApi`.
- Cross-cutting concerns (telemetry, resilience) live in `eShopWeb.AspireServiceDefaults` and are registered via extension methods, not scattered through domain code.
- No service locator. All dependencies resolved through constructor injection.
