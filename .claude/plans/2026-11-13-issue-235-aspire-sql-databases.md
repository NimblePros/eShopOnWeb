# Issue #235 — Move databases to .NET Aspire

Issue: https://github.com/NimblePros/eShopOnWeb/issues/235

## Goal

The `Catalog` and `Identity` databases currently live in a Windows-only LocalDB instance
(connection strings in `src/Web/appsettings.json` and `src/PublicApi/appsettings.json`).
Move them into the Aspire AppHost as a SQL Server container so the sample runs on Linux,
macOS, and Windows, and remove every LocalDB reference from the repo.

## Assumptions

- One SQL Server container (`sqlserver`) hosts both databases (`catalogdb` →
  `Microsoft.eShopOnWeb.CatalogDb`, `identitydb` → `Microsoft.eShopOnWeb.Identity`). The
  apps keep their existing connection-string keys (`CatalogConnection`,
  `IdentityConnection`) via `WithReference(db, connectionName: ...)`, so no app code needs
  to learn new names.
- The container uses a persistent lifetime and a data volume so data survives AppHost
  restarts (matching the existing persistent Seq container). The SA password is the
  Aspire-generated parameter, stored in the AppHost's user secrets.
- Schema creation stays where it is today: `CatalogContextSeed`/`AppIdentityDbContextSeed`
  call `Database.Migrate()` at startup. No separate migration service.
- Running Web/PublicApi directly (without the AppHost) is still possible either with
  `UseOnlyInMemoryDatabase=true` or by supplying connection strings (user secrets / env
  vars). A missing connection string now fails with a clear message instead of a vague
  SqlClient error. EF design-time tooling (`dotnet ef migrations add`) keeps working
  without a connection string.
- Docker Compose (`appsettings.Docker.json`) and the Azure (`azd`) production path are
  unchanged — they already use their own SQL Server.

## Phase 1 — AppHost

- [ ] Add `Aspire.Hosting.SqlServer` (13.6.5) to `Directory.Packages.props` and the AppHost.
- [ ] Add `sqlserver` container (persistent lifetime + data volume) with `catalogdb` and
      `identitydb` databases.
- [ ] Reference both databases from `web` and `publicapi` as `CatalogConnection` /
      `IdentityConnection`, and `WaitFor` them.

## Phase 2 — Remove LocalDB

- [ ] Remove LocalDB connection strings from `src/Web/appsettings.json` and
      `src/PublicApi/appsettings.json`.
- [ ] `Infrastructure/Dependencies.cs`: drop the LocalDB comment; throw a descriptive
      `InvalidOperationException` when a connection string is missing (outside EF design time).
- [ ] Update README "Running the sample locally" / SQL Server section and the dev container
      readme to describe running via the AppHost.
- [ ] Update the remaining LocalDB mention in the EndToEnd `BrowserFixture` comment.

## Phase 3 — Automated tests

- [ ] New `tests/AppHostTests` project (xunit.v3 + `Aspire.Hosting.Testing`) that builds the
      AppHost model (no Docker required) and asserts:
  - a SQL Server resource exists with the two databases and expected database names;
  - `web` and `publicapi` get `ConnectionStrings__CatalogConnection` /
    `ConnectionStrings__IdentityConnection` pointing at those databases;
  - both projects wait for both databases.
- [ ] Unit tests (`tests/UnitTests`) for `ConfigureLocalDatabaseContexts`: missing
      connection string → descriptive `InvalidOperationException`; provided connection
      string → SQL Server provider configured.
- [ ] Add the new test project to `eShopOnWeb.slnx`.

## Phase 4 — Independent review

- [ ] Separate agent (fresh context) reviews the full diff against the issue: correctness,
      missed requirements, test gaps, regressions, security, performance, accessibility.
- [ ] Address all significant findings (or record why not, below).

## Phase 5 — Verification

- [ ] `dotnet build && dotnet test && dotnet format --verify-no-changes` runs clean.
- [ ] Manually run the AppHost and confirm the SQL container starts and the site serves the
      catalog (if Docker is available locally).

## Review notes

_(to be filled in)_
