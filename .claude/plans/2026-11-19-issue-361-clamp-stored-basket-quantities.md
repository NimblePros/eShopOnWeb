# Issue #361: Clamp stored basket quantities above BasketItem.MaxQuantity

https://github.com/NimblePros/eShopOnWeb/issues/361 (follow-up from #360)

Rows saved before #360 can hold `BasketItem.Quantity` above `BasketItem.MaxQuantity` (999).
Checkout then returns 400, and the basket page renders an input whose value exceeds its `max`.
The fix is a data-only EF Core migration that clamps those rows.

## Assumptions

- A data migration on `CatalogContext` is enough. SQL Server deployments apply it through
  `CatalogContextSeed` (`Database.Migrate()`). The in-memory provider has no migrations, but it
  starts empty on every run and so never holds pre-#360 rows.
- The private parameterless `BasketItem` constructor stays. Without it EF Core binds the public
  constructor, whose guard would throw while loading any row that is still over the max (for
  example, one written by an old instance during a rolling deploy, before the migration runs).
  It costs nothing, and dropping it is optional in the issue.
- `Down` is a no-op: the original quantities are not recoverable.
- No model change, so `CatalogContextModelSnapshot` is unchanged.

## Phase 1: Migration

- [x] Add `20261119130000_ClampBasketItemQuantityToValidRange`. `dotnet ef migrations add` failed with a
      `MissingMethodException` (tool/runtime EF version mismatch), so the migration is hand-written
      and its designer's target model is copied from `CatalogContextModelSnapshot`.
- [x] `Up`: `UPDATE [BasketItems] SET [Quantity] = 999 WHERE [Quantity] > 999`. The migration keeps
      999 as its own literal so it never changes behavior if `BasketItem.MaxQuantity` changes; a
      test pins it to the current `MaxQuantity`, so a change to the constant flags the need for a
      new migration.
- [x] Also clamp negative quantities (left by the #354 `AddQuantity` overflow) to 0. They fail
      checkout's `[Range(0, 999)]` the same way. A zero line is valid and `SetQuantities` removes it.
- [x] `Down`: intentionally empty, with a comment explaining why.

## Phase 2: Automated tests (IntegrationTests)

- [x] `Migrations/ClampBasketItemQuantityToValidRangeMigration`:
  - `Up` holds exactly the two clamping SQL operations, pinned to the current `MaxQuantity`.
  - `Down` holds no operations.
  - `CatalogContext` (SQL Server provider, no connection opened) lists the migration, and the
    model has no pending changes, so the snapshot is in sync.

## Phase 3: Independent review

- [x] A separate agent with fresh context reviews the full diff.
- [x] Address every significant finding, or record why it was skipped:
  - Negative quantities not clamped: fixed (second `UPDATE`, migration renamed to
    `ClampBasketItemQuantityToValidRange`).
  - The discovery test asserted the migration was the *last* one, which breaks as soon as a new
    migration is added: changed to `Assert.Contains`.
  - Skipped: the SQL is never executed against SQL Server. The test suite has no SQL Server or
    Testcontainers infrastructure, and adding it is out of scope. The reviewer checked the
    table and column names against the snapshot.
  - Skipped: nothing verifies the designer's target model. The reviewer confirmed it is
    byte-identical to the snapshot apart from the method name, and the model is unchanged.
  - Skipped: the full-table scan and lock escalation, and rows written above the max by an old
    instance during a rolling deploy. Both are low risk for a basket table (see Assumptions).

## Phase 4: Verification

- [x] `dotnet ./BuildTestFormat.cs` passes after the last code change.
