# Issue #381 — Drop leftover default constraints from CatalogContext migrations

https://github.com/NimblePros/eShopOnWeb/issues/381 (found while working on #364)

## Assumptions

- The fix is a new migration; existing migrations are not edited, because databases that
  already applied them must converge on the same schema.
- `AlterColumn` with the same type/nullability and no `defaultValue` is used to drop each
  default. EF's SQL Server generator always drops the column's existing default constraint
  (looked up by name in `sys.default_constraints`) before altering, so it works whatever
  name SQL Server gave the constraint. None of the columns is indexed, so the `ALTER COLUMN`
  does not need index rebuilds.
- The model and snapshot do not change; the migration's designer is the previous one's
  target model under the new migration id.
- `Down()` restores the defaults exactly as they were, so a rollback reproduces the old schema.

## Phase 1 — Migration

- [x] Add `20261119140000_DropLeftoverDefaultConstraints` (+ designer) to
      `src/Infrastructure/Data/Migrations`, dropping the 10 defaults listed in the issue.
- [x] `Down()` re-adds the same defaults.

## Phase 2 — Tests (tests/IntegrationTests/Migrations)

- [x] Remove `KnownLeftoverDefaults` from `MigratedSchemaMatchesTheSchemaBuiltFromTheModel`,
      so the migrated schema must match the model's schema exactly.
- [x] Add a test that rolls the new migration back and re-applies it, asserting the defaults
      come back on `Down()` and are gone again after `Up()`.

## Phase 3 — Review

- [x] Independent review of the full diff by a separate agent with fresh context.
- [x] Address significant findings, or record why each was skipped.

## Phase 4 — Verify

- [x] `dotnet ./BuildTestFormat.cs` passes after the last code change.

### Review outcome

- No significant findings. The reviewer confirmed that `AlterColumn` drops the default by
  looking it up in `sys.default_constraints`, that no affected column is indexed, and that
  the types and lengths in `Down()` match the earlier migrations.
- Nits skipped: the defaults map now lives in the new migration test only (intended), and
  the insert test hard-codes `SYSDATETIMEOFFSET()` for `OrderDate`, which matches the model.

### Verification

- `dotnet ./BuildTestFormat.cs`: build OK. Tests: 173 unit, 83 functional, 20 integration,
  158 public API, 6 AppHost and 2/3 end-to-end passing (1 skipped by design). Formatting
  clean.
- Mutation check: with the string-column drops removed from `Up()`, 3 integration tests fail.
