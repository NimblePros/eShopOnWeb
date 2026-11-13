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

- [x] Add `Aspire.Hosting.SqlServer` (13.6.5) to `Directory.Packages.props` and the AppHost.
- [x] Add `sqlserver` container (persistent lifetime + data volume) with `catalogdb` and
      `identitydb` databases.
- [x] Reference both databases from `web` and `publicapi` as `CatalogConnection` /
      `IdentityConnection`. `web` waits for both databases; `publicapi` waits for `web`
      (healthy on `/alive`) — see review finding 1.

## Phase 2 — Remove LocalDB

- [x] Remove LocalDB connection strings from `src/Web/appsettings.json` and
      `src/PublicApi/appsettings.json`.
- [x] `Infrastructure/Dependencies.cs`: drop the LocalDB comment; throw a descriptive
      `DatabaseNotConfiguredException` when a connection string is missing (outside EF design time).
- [x] Update README "Running the sample locally" / SQL Server section and the dev container
      readme to describe running via the AppHost.
- [x] Update the remaining LocalDB mention in the EndToEnd `BrowserFixture` comment.

## Phase 3 — Automated tests

- [x] New `tests/AppHostTests` project (xunit.v3 + `Aspire.Hosting.Testing`) that builds the
      AppHost model (no Docker required) and asserts:
  - a SQL Server resource exists with the two databases and expected database names;
  - `web` and `publicapi` get `ConnectionStrings__CatalogConnection` /
    `ConnectionStrings__IdentityConnection` pointing at those databases;
  - `web` waits for both databases, and `publicapi` waits for `web`, which has a health check.
- [x] Unit tests (`tests/UnitTests`) for `ConfigureLocalDatabaseContexts`: missing
      connection string → `DatabaseNotConfiguredException`; provided connection string → SQL
      Server provider configured; EF design time → no connection string needed; in-memory option.
- [x] Add the new test project to `eShopOnWeb.slnx`.

## Phase 4 — Independent review

- [x] Separate agent (fresh context) reviews the full diff against the issue: correctness,
      missed requirements, test gaps, regressions, security, performance, accessibility.
- [x] Address all significant findings (or record why not, below).

## Phase 5 — Verification

- [x] `dotnet build && dotnet test && dotnet format --verify-no-changes` — run against
      `eShopOnWeb.slnx` explicitly, because the bare commands fail with MSB1011 (the repo root has
      both `docker-compose.dcproj` and `eShopOnWeb.slnx`). Build clean, all tests pass.
      `dotnet format --verify-no-changes` reports ~70 files with existing violations on `main`;
      none are in files this change adds or modifies.
- [x] Manually run the AppHost and confirm the SQL container starts and the site serves the
      catalog (if Docker is available locally).

## Review notes

Independent review (fresh-context agent) findings and outcomes:

1. **High — Web and PublicApi raced to seed the same empty database.** Confirmed: the first
   real AppHost run had 24 catalog items instead of 12. Fixed: Web maps `/alive` (liveness
   only) and has `WithHttpHealthCheck("/alive")`; PublicApi `WaitFor(web)`. Kestrel only
   listens after `SeedDatabaseAsync`, so PublicApi starts after migration + seeding. Re-run on
   a fresh volume: 12 items, 5 brands, 3 users. Covered by an AppHost test.
2. **Medium — missing connection string was swallowed by the seeding catch-all.** Fixed:
   new `DatabaseNotConfiguredException`; both `SeedDatabaseAsync` methods let it propagate.
   Verified Web exits with the message when run without config.
3. **Medium — clearing AppHost user secrets locks you out of the persisted volume.**
   Documented in the README (secret name and reset steps).
4. **Low — connection names duplicated.** Unit tests now use `Dependencies.*_CONNECTION_NAME`;
   AppHost tests assert the literal names the apps read. The AppHost can't reference
   Infrastructure (project references there are resource metadata), so literals remain.
5. **Low — dev container readme only mentioned Web.** Fixed (both projects + CLI flag).
   Adding docker-in-docker to the dev container is out of scope for this issue.
6. **Low — in-memory mode gives each app separate data.** Documented in the README.
7. **Low — EF design-time branch untested.** Added a unit test; README notes that commands
   which connect need `--connection`.
8. **Nit — double BOM in AppHost.cs.** Fixed.
9. **Nit — stale `dotnet ef database update` link in docs/walkthroughs/vs-for-mac.md.** Fixed.
