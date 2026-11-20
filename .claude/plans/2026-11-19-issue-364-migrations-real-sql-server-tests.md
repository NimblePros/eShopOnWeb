# Issue #364 — Run EF Core migrations against a real SQL Server in tests

https://github.com/NimblePros/eShopOnWeb/issues/364 (follow-up from #361)

## Assumptions

- Use Testcontainers (`Testcontainers.MsSql`) rather than Aspire: the fixture needs one bare
  SQL Server, not the whole AppHost graph.
- The tests live in `tests/IntegrationTests` and require a container runtime, which the
  README already lists as a prerequisite. They fail (not skip) without one, so a missing
  runtime in CI can never silently pass.
- Scope is `CatalogContext` migrations, as the issue states. `AppIdentityDbContext` is out of scope.
- "Designer and model drift" is checked by comparing the schema produced by `Migrate()` with
  the schema produced by `EnsureCreated()` (built straight from the model) on a second database.

## Phase 1 — Fixture

- [x] Add `Testcontainers.MsSql` to `Directory.Packages.props` and the IntegrationTests project.
- [x] Add a shared xUnit v3 assembly fixture that starts one SQL Server container and hands out
      fresh, uniquely named databases (connection strings) per test.

## Phase 2 — Tests (IntegrationTests/Migrations)

- [x] All `CatalogContext` migrations apply to an empty database and leave no pending migrations.
- [x] Data migration: migrate to `20250310153034_Updates`, insert basket lines with quantities
      5000, 999, 1, 0 and -3, migrate to `ClampBasketItemQuantityToValidRange`, and assert
      999, 999, 1, 0, 0.
- [x] Model drift: the migrated schema (columns, types, nullability, lengths, indexes, foreign
      keys, sequences) equals the `EnsureCreated()` schema.
- [x] `CatalogContextSeed.SeedAsync` runs against the migrated database.

## Phase 3 — Review

- [x] Independent review of the full diff by a separate agent with fresh context.
- [x] Address significant findings, or record why each was skipped.

### Review outcome

- Fixed (significant): `MsSqlBuilder.Build()` already requires Docker, and an xUnit v3
  assembly fixture is created before any test runs. Without Docker, all 18 tests would have
  failed. The builder now runs inside the lazy start too.
- Fixed: the schema comparison now also covers table schemas, identity seed and increment,
  computed columns, collations, unique constraints, descending and included index columns,
  FK update actions, check constraints, and sequence min/max/cycle/cache.
- Fixed: the drift test asserts that every catalog query returns rows, not just the columns query.
- Fixed: the seed test passes `retry: 10`, so a failed migration surfaces at once instead of
  after 11 silent attempts.
- Skipped: multiset comparison. Every line includes its object name, so duplicate lines can't occur.
- Skipped: pinning the SQL Server image. It matches `docker-compose.yml` (`2022-latest`);
  pinning one without the other would let the tests drift from what the app runs on.
- Skipped: running `Down` against the database. Down is empty, and the existing test asserts that.

### Found while testing

- The drift test found 10 `DEFAULT` constraints that older migrations left behind and that
  the model does not declare. They are allowlisted, and the test fails once they're dropped.
  Filed #381 to drop them.

## Phase 4 — Verify

- [x] `dotnet ./BuildTestFormat.cs` passes after the last code change.
