# Plan: Saved Items

Source: [spec.md](spec.md). No code yet. Components are listed bottom-up (data to UI). Each section ends with the interface to the next component, in one sentence.

New production files (budget: 5 outside the migration):

| # | File | Component |
| --- | --- | --- |
| 1 | `src/ApplicationCore/Entities/SavedItem.cs` | Entity |
| 2 | `src/Infrastructure/Data/Config/SavedItemConfiguration.cs` | Persistence |
| 3 | `src/ApplicationCore/Interfaces/ISavedItemService.cs` | Service |
| 4 | `src/ApplicationCore/Services/SavedItemService.cs` | Service (specifications nested here, see Open questions) |
| 5 | `src/Web/Pages/SavedItems/Index.cshtml` + `.cshtml.cs` | Page (counted as one) |

Edited existing files: `CatalogContext.cs` (one `DbSet`), `ConfigureCoreServices.cs` (one registration), `_product.cshtml` (button). Plus one generated EF migration.

## 1. Entity: `SavedItem`

- `BaseEntity, IAggregateRoot` (needed for `IRepository<SavedItem>`).
- Properties: `BuyerId` (string, required, guarded non-empty, max 256), `CatalogItemId` (int, guarded `> 0`), `CatalogItem` navigation (needed to render name, picture, price).
- No behavior beyond constructor guards.
- Tests: unit tests for constructor guards (null/empty/oversized `BuyerId`, `CatalogItemId <= 0`).

Interface to Persistence: EF maps `SavedItem.BuyerId` (string), `SavedItem.CatalogItemId` (int) and the `SavedItem.CatalogItem` navigation, so the entity needs a parameterless-compatible private constructor path and no EF types.

## 2. Persistence: configuration, `DbSet`, migration

- `SavedItemConfiguration : IEntityTypeConfiguration<SavedItem>`: `BuyerId` required, `HasMaxLength(256)`; FK to `CatalogItem` with `OnDelete(Cascade)`; unique index on `(BuyerId, CatalogItemId)`.
- Add `DbSet<SavedItem> SavedItems` to `CatalogContext`. `ApplyConfigurationsFromAssembly` picks up the configuration.
- Add one EF migration (`dotnet ef migrations add AddSavedItems`), generated, not hand-edited.
- No new repository class: `EfRepository<SavedItem>` is already covered by the open-generic registration.
- Tests (`IntegrationTests`, in-memory): add, list with `CatalogItem` loaded, delete.

Interface to Service: the service sees this component only as `IRepository<SavedItem>` (`ListAsync(ISpecification<SavedItem>)`, `AddAsync`, `DeleteAsync`), and a unique-index violation reaches it as a provider exception thrown from `AddAsync` (`DbUpdateException` on SQL Server) that `ApplicationCore` cannot name.

## 3. Service: `ISavedItemService` / `SavedItemService`

Constants: `MaxSavedItems = 100`. Registered in `ConfigureCoreServices` as `AddScoped<ISavedItemService, SavedItemService>()`.

Depends on `IRepository<SavedItem>`, `IReadRepository<CatalogItem>` (or `IRepository<CatalogItem>`, as the basket page does), and `IAppLogger<SavedItemService>`.

Operations:

- `SaveAsync(buyerId, catalogItemId)`
  1. List the buyer's saved rows (one trip). If `catalogItemId` is present, return Ok without writing.
  2. If count `>= 100`, return Conflict.
  3. Get the catalog item. If null, return NotFound.
  4. Add the row. If `AddAsync` throws, re-query by `(buyerId, catalogItemId)`: row exists means a concurrent save won, return Ok; otherwise rethrow.
- `RemoveAsync(buyerId, catalogItemId)`: find by spec and delete if present. Missing row returns Ok.
- `ListAsync(buyerId)`: specification with `Include(CatalogItem)`, filtered by buyer, no paging.

The spec says the service catches `DbUpdateException`. `ApplicationCore` must not reference EF Core (rule 3), so step 4 uses the re-query form, which gives the same outcome (both racing requests return Ok) without an EF type.

Tests (`UnitTests`, NSubstitute): save new, save duplicate (no `AddAsync`), save at 99 (Ok) and at 100 (Conflict), unknown catalog item (NotFound), `AddAsync` throws then row exists (Ok), `AddAsync` throws then row missing (rethrows), remove existing, remove missing (Ok), list returns repository result.

Interface to Page: `ISavedItemService` exposes `Task<Result> SaveAsync(string buyerId, int catalogItemId)` and `Task<Result> RemoveAsync(string buyerId, int catalogItemId)` returning `ResultStatus.Ok`, `NotFound` (save only) or `Conflict` (save only), and `Task<IReadOnlyList<SavedItem>> ListAsync(string buyerId)` returning rows with `CatalogItem` populated, and throwing on infrastructure failure.

## 4. Page: `/SavedItems` (`Index.cshtml` + `Index.cshtml.cs`)

- `[Authorize]` on the page model, so every handler redirects anonymous users to login (criterion 4). Treat authenticated with null `User.Identity.Name` as unauthenticated (`Challenge()`).
- Handlers:
  - `OnGetAsync`: list and render. Empty list shows "You have no saved items yet." with a catalog link, 200.
  - `OnPostSaveAsync(int catalogItemId)`
  - `OnPostRemoveAsync(int catalogItemId)`
- `buyerId` comes only from `User.Identity.Name`. No other request fields are read.
- Status mapping:
  - Binding failure or `catalogItemId <= 0`: `BadRequest()` (400, never 500).
  - Service `NotFound`: `NotFound()` (404).
  - Service `Conflict`: reload list, set `Response.StatusCode = 409`, render the page with "Saved Items is full (100 maximum)."
  - Ok: `RedirectToPage()` (PRG).
  - Anything else: let the exception reach the existing error page (500).
- View: simple list (name, picture, price, remove form with antiforgery). No paging. Reuse existing catalog CSS, no new assets.
- Tests (`FunctionalTests`, `WebApplicationFactory`): anonymous GET redirects to login; signed-in empty page; save then list; save twice gives one row; remove; remove twice; `catalogItemId` of `abc`, `0`, `-1`, `2147483648` gives 400; unknown id gives 404; 100 items then save gives 409 with message.

Interface to Tile: the page accepts an antiforgery-protected POST to `/SavedItems?handler=Save` with a single integer form field `catalogItemId` and answers 302 to `/SavedItems` on success, 302 to login when anonymous, 400 for a malformed id, 404 for an unknown item, and 409 (page rendered) when the list is full.

## 5. Tile button: `_product.cshtml`

- Add a second, separate `<form method="post" asp-page="/SavedItems/Index" asp-page-handler="Save">` with a hidden `catalogItemId` (`Model.Id`) and a submit button. It cannot nest inside the existing basket form.
- No JavaScript. Button is shown to anonymous users too; submit leads to login.
- Tests: one end-to-end (`EndToEndTests`, Playwright) of save from the catalog then view on the page; a functional check that the catalog page contains the save form.

Interface to the browser (end of chain): the tile renders a form whose only submitted fields are `catalogItemId` and the antiforgery token, at most 1 KB per request.

## 6. Verification

- `dotnet test tests/UnitTests/UnitTests.csproj`, `IntegrationTests`, `FunctionalTests`; E2E only if Playwright browsers are installed.
- `dotnet format eShopOnWeb.slnx --verify-no-changes`.
- NFR checks that the in-memory provider cannot prove (unique index, 50 parallel saves = 1 row, latency P95): run against the SQL Server container, manually or with a one-off script. Not part of the CI suite.

## Open questions (also in [docs/context/cold/gap-log.md](../../docs/context/cold/gap-log.md))

1. Login redirect target: spec says `/Identity/Account/Login`, but `ConfigureCookieSettings` sets `LoginPath = "/Account/Login"`. Plan follows the code unless told otherwise.
2. DB round trips: spec caps at 2 per request. Save needs 3 (list, catalog lookup, insert) because the in-memory provider does not enforce the FK, so a missing item cannot be detected from the insert. Options: accept 3 for save (recommended), or drop the explicit catalog check and rely on the FK on SQL Server only.
3. File budget: five files is only reachable by nesting the Ardalis specifications in `SavedItemService.cs` (stack.md says specifications live in `ApplicationCore/Specifications`) and counting the Razor `.cshtml`/`.cshtml.cs` pair as one. Alternative: move specifications to `Specifications/` and accept 6 or 7 files.
4. In-memory provider does not enforce the unique index or cascade delete, so the concurrency and cascade criteria cannot be tested outside SQL Server.
5. `Conflict` result status relies on `Ardalis.Result` 10.1.0 (already referenced); confirm `ResultStatus.Conflict` exists before coding.
