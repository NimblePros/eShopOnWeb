# Saved Items

## Scenario

A signed-in shopper browsing the catalog sees a product they like but is not ready to buy. They save it to a personal "Saved Items" list, come back later to review the list, and remove products they no longer want. The list holds catalog products only, belongs to one shopper, and is separate from the basket. Anonymous shoppers are asked to sign in first. The feature spans `ApplicationCore` (entity and service), `Infrastructure` (table and migration), and `Web` (a Razor Page and a button on each product tile).

## Acceptance criteria

1. **Save**
   - Given a signed-in shopper with fewer than 100 saved items
   - When they save an existing catalog item that is not yet on their list
   - Then the item appears on their Saved Items page.

2. **Save twice**
   - Given a signed-in shopper who already saved catalog item 5
   - When they save catalog item 5 again
   - Then their list still contains exactly one entry for item 5.

3. **Remove**
   - Given a signed-in shopper with catalog item 5 on their list
   - When they remove item 5
   - Then item 5 no longer appears on their Saved Items page.

4. **Anonymous**
   - Given a shopper who is not signed in
   - When they request the Saved Items page
   - Then they are redirected to the login page.

## Concurrency

Two simultaneous saves of the same item by the same shopper must produce one row. A unique index on `(BuyerId, CatalogItemId)` enforces this; the service catches the unique-violation `DbUpdateException` and treats it as success, so both requests return the same result. Removing an item that is already gone is also a success (idempotent). The 100-item cap is checked with a count before insert, so N simultaneous saves at 99 items can overshoot to at most 99 + N; this is accepted because the cap protects page size, not correctness. Different shoppers never contend: rows are keyed per buyer.

## Errors

Plain Razor Pages responses, no JSON body. Not signed in: 302 to `/Identity/Account/Login`. Malformed id (non-numeric, zero, negative, or above `int.MaxValue`): 400. Catalog item does not exist: 404. List already has 100 items: 409, and the Saved Items page is shown with the message "Saved Items is full (100 maximum)." Unexpected failures (database down): 500 through the existing error page; no partial write is possible because each request is a single `SaveChanges`. Duplicate save and double remove are not errors (200/redirect, same as the first call).

## Boundaries

Empty list: the page renders the text "You have no saved items yet." with a link to the catalog, and returns 200. Oversized: maximum 100 items per shopper, and the page shows all of them without paging (no new pagination). Malformed input: only an integer `catalogItemId` is accepted; no other user-supplied fields are read, and the shopper identity comes from the authenticated user, never from the request. `BuyerId` uses the same value as the basket (`User.Identity.Name`), at most 256 characters, matching the existing `Baskets.BuyerId` column. If a catalog item is deleted after being saved, its saved row is removed with it (cascade delete) and it simply stops appearing.

## Integrations

No external service. Two in-process dependencies are assumed: ASP.NET Core Identity cookie authentication (provides `User.Identity.Name`; if it is null for an authenticated user, the request is treated as unauthenticated) and the existing SQL Server `CatalogContext` via EF Core (or the in-memory provider when `UseOnlyInMemoryDatabase=true`). Assumed failure mode: the database is unreachable or times out, so the request fails with 500 and no retry. The saved-items list is not cached, so there is no stale-read case. No Public API, Blazor Admin, or basket merge behavior is included.

## NFR budget

NFR budget:
- Latency: P95 ≤ 300 ms for the Saved Items page with 100 items; P95 ≤ 200 ms for a save or remove request. Measured at the server on the local SQL Server container, 20 concurrent users.
- Payload size: request ≤ 1 KB (one integer id plus the antiforgery token); Saved Items page response ≤ 60 KB at 100 items; at most 2 database round trips per request.
- Error behavior: 0 duplicate rows under 50 parallel saves of the same item; 0 unhandled exceptions for malformed ids (400, never 500); the failure statuses are exactly those listed under Errors.
- Cost/complexity constraint: 1 new table (`SavedItems`) with 1 unique index and 1 EF migration; no new NuGet packages; no new external services; no more than 5 new production files outside the migration.
