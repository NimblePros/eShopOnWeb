# Claude Code — Project Rules

## Architecture (non-negotiable)
- Follow Clean Architecture: dependencies flow **inward only** — Infrastructure and Web may depend on ApplicationCore; ApplicationCore must not reference them.
- Domain logic lives in `ApplicationCore`. Do not put business rules in Controllers, Razor Pages, or Infrastructure.
- Use the repository + Ardalis.Specification pattern for all data access. No raw DbContext queries in ApplicationCore or Web.

## Code style
- Target framework: **net10.0** across all projects.
- 4-space indent for C#, 2-space for XML/csproj (`.editorconfig` is authoritative).
- Prefer `Ardalis.GuardClauses` for guard clauses; `Ardalis.Result` for operation results.
- Use source-generated `Mediator` (not MediatR) for commands/queries in Web.

## APIs
- New REST endpoints go in `PublicApi` using **FastEndpoints** — not minimal API or MVC controllers.
- Authenticate API calls with JWT Bearer; do not expose unauthenticated mutation endpoints.

## Tests
- Unit and integration tests use **xunit.v3 + NSubstitute** — keep it that way.
- `PublicApiIntegrationTests` uses MSTest; do not migrate it.
- Integration tests use EF Core InMemory; do not swap to SQLite without discussion.
- End-to-end tests use Playwright (`EndToEndTests`).
- Run all tests with: `dotnet test`

## Packages
- All package versions are managed centrally in `Directory.Packages.props`. Never add a `Version` attribute to a `<PackageReference>` in a `.csproj`.

## Aspire
- Local dev orchestration is via `eShopWeb.AppHost`. Use Aspire service defaults (OTLP telemetry, service discovery, HTTP resilience) when adding new services.

## What NOT to do
- Do not add `Directory.Build.props` without discussion — it does not currently exist and its absence is intentional.
- Do not store secrets in source; use Azure Key Vault integration already wired in `Web`.
- Do not introduce a second DI container or a second mediator library.

See [docs/context/stack.md](docs/context/stack.md) for full stack reference.
