# Saved Items: tasks

Source: [plan.md](plan.md), [spec.md](spec.md). No code yet.

**Implement first: T1. First slice: one AC end-to-end, <100 lines.** Covers AC4 (Anonymous: unauthenticated request to the Saved Items page is redirected to login).

**Highest-risk task: T5.** The save-race recovery (re-query after a failed insert) cannot be exercised by the in-memory provider, which does not enforce the unique index, so a mistake there can ship green and only fail on SQL Server.

Sizing: line counts are estimates of hand-written production plus test lines. The generated migration in T3 is excluded from the 100-line limit (generated files are not hand-edited, rule 8 in [copilot-instructions.md](../../.github/copilot-instructions.md)).

Order: T1, T2, T3, T4, T5, T6, T7, T8. T1 has no dependencies. T3 and T4 both need T2. T5 needs T4. T6 needs T1, T4, T5. T7 needs T5, T6. T8 needs T6.

## T1: Page shell, anonymous redirect (first slice, AC4)

- Input: the existing Razor Pages setup in `src/Web` and the cookie `LoginPath` in `ConfigureCookieSettings.cs`.
- Output: `src/Web/Pages/SavedItems/Index.cshtml` and `.cshtml.cs` with `[Authorize]` and a static "You have no saved items yet." empty state (about 25 lines), plus one functional test file (about 30 lines).
- Done: `FunctionalTests` `SavedItemsPageTests.Anonymous_Get_RedirectsToLogin` passes, asserting status 302 and a `Location` header starting with the configured login path (`/Account/Login`, see gap-log).

## T2: Entity, `DbSet`, EF configuration

- Input: `BaseEntity`, `IAggregateRoot`, `CatalogItem`, and the `BasketConfiguration` pattern.
- Output: `SavedItem` entity, `SavedItemConfiguration` (max length 256, FK with cascade, unique index on `BuyerId, CatalogItemId`), `DbSet<SavedItem>` in `CatalogContext` (about 55 lines with tests).
- Done: `UnitTests` `SavedItemTests` reject empty `BuyerId` and `CatalogItemId <= 0`, and `IntegrationTests` `SavedItemRepositoryTests.Add_Then_List_Returns_Row_With_CatalogItem` passes on the in-memory provider.

## T3: EF migration

- Input: model from T2 and the existing `Migrations/` folder.
- Output: one generated migration `AddSavedItems` and updated `CatalogContextModelSnapshot` (generated, excluded from line count).
- Done: `dotnet ef migrations script` for the new migration contains `CREATE TABLE [SavedItems]`, `nvarchar(256)`, `ON DELETE CASCADE`, and `CREATE UNIQUE INDEX [IX_SavedItems_BuyerId_CatalogItemId]`; the migration applies cleanly on the local SQL Server container.

## T4: Service core: list and basic save, DI

- Input: `IRepository<SavedItem>` (T2), `Ardalis.Result`, `BasketService` as the style reference.
- Output: `ISavedItemService` (`SaveAsync`, `ListAsync`), `SavedItemService` with nested by-buyer specification (list with `CatalogItem` included), and the scoped registration in `ConfigureCoreServices.cs` (about 70 lines with tests).
- Done: `UnitTests` `SavedItemServiceSave` passes: a new item calls `AddAsync` once and returns `Ok`; saving an item already in the buyer's list returns `Ok` and never calls `AddAsync` (AC2 at service level).

## T5: Service rules: cap, unknown item, race recovery, remove (highest risk)

- Input: `ISavedItemService` and `SavedItemService` from T4, `IRepository<CatalogItem>`.
- Output: `SaveAsync` returns `Conflict` at 100 items and `NotFound` for a missing catalog item, and recovers when `AddAsync` throws by re-querying; `RemoveAsync` is added to the interface and implementation, idempotent (about 90 lines with tests).
- Done: `UnitTests` pass for each case: 99 items saves, 100 items returns `Conflict` with no `AddAsync`, missing catalog item returns `NotFound`, `AddAsync` throws then row exists returns `Ok`, `AddAsync` throws then row missing rethrows, removing a missing row returns `Ok`.

## T6: Page list and save handlers (AC1, AC2 end-to-end)

- Input: `ISavedItemService` (T4, T5), page shell (T1).
- Output: `OnGetAsync` (list), `OnPostSaveAsync(int catalogItemId)` with 400/404/409/redirect mapping, and list markup in `Index.cshtml` (about 90 lines with tests).
- Done: `FunctionalTests` `SavedItemsPageTests` pass: save item 5 then GET shows item 5 (AC1); saving item 5 twice leaves one entry (AC2); `catalogItemId=abc` returns 400; unknown id returns 404.

## T7: Page remove handler (AC3 end-to-end)

- Input: `RemoveAsync` (T5), list view (T6).
- Output: `OnPostRemoveAsync(int catalogItemId)` and a remove form with antiforgery per row (about 40 lines with tests).
- Done: `FunctionalTests` `SavedItemsPageTests.Remove_Item_No_Longer_Listed` passes (AC3), and removing the same item twice returns the same redirect both times.

## T8: Product tile button and full-list message

- Input: `OnPostSaveAsync` contract (T6) and `_product.cshtml`.
- Output: a separate save form in `_product.cshtml` posting `catalogItemId` to the Save handler (about 10 lines), the "Saved Items is full (100 maximum)." message rendering with a 409 test, and one Playwright test (about 50 lines total).
- Done: `EndToEndTests` `SavedItemsFlow.Save_From_Catalog_Appears_On_Saved_Page` passes, and a functional test with 100 seeded items asserts status 409 and the exact message text.

## Seams

| Seam | Defined by | Used by | What they must agree on |
| --- | --- | --- | --- |
| `SavedItem` entity and `IRepository<SavedItem>` | T2 | T4, T5 | Constructor `(buyerId, catalogItemId)`, `CatalogItem` navigation populated by the list specification, and a unique-index violation surfaces as an exception from `AddAsync` that `ApplicationCore` cannot name. |
| Database schema | T2 (mapping), T3 (migration) | T4, T5 at runtime | Table `SavedItems`, `BuyerId` `nvarchar(256)`, FK with cascade delete, unique index `IX_SavedItems_BuyerId_CatalogItemId`. T3 must match the model exactly. |
| `ISavedItemService` | T4 (`SaveAsync`, `ListAsync`), T5 (`RemoveAsync`) | T6, T7 | `buyerId` is `User.Identity.Name`, never from the request; `Result` status is `Ok`, `NotFound` (save only) or `Conflict` (save only); `ListAsync` returns rows with `CatalogItem` populated; unexpected failures throw. |
| Result to HTTP mapping | T5 (statuses), T6 (mapping) | T7, T8 | `Ok` redirects to `/SavedItems`, `NotFound` is 404, `Conflict` is 409 with the page rendered, binding failure or id `<= 0` is 400. |
| Page route and handlers | T1 (route, `[Authorize]`), T6 (`Save`), T7 (`Remove`) | T8 | Route `/SavedItems`, handler names `Save` and `Remove`, single integer form field `catalogItemId`, antiforgery token required. |
| List view markup | T1 (empty state), T6 (rows) | T7 | Each row is identifiable by `CatalogItemId`, so T7 can attach a remove form without changing T6's row layout. |
| Login redirect target | T1 (test asserts it) | T6, T7 | Stays at `/Account/Login` unless the gap-log item is resolved; any change updates T1's assertion only. |
