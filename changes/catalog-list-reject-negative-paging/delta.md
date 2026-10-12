# Change: catalog-list-reject-negative-paging

Scope: `GET api/catalog-items` ([CatalogItemListPagedEndpoint.cs](../../src/PublicApi/CatalogItemEndpoints/CatalogItemListPagedEndpoint.cs)) only.
Source not edited yet. Items marked (inferred) come from reading code, not from running it.

## Preserved behaviour (one sentence)
After this change, any request to `GET api/catalog-items` with `pageSize` and `pageIndex` omitted or `>= 0` must still return 200 with the same items, `PageCount` and `CorrelationId` as today, and an omitted or `0` `pageSize` must still return the entire catalog.

## ADDED
- `pageSize < 0` returns 400 Bad Request.
- `pageIndex < 0` returns 400 Bad Request.
- The 400 body names the offending field (FastEndpoints validation `ErrorResponse`: `statusCode`, `message`, `errors`).
- OpenAPI/Swagger for the route gains a 400 response (inferred, FastEndpoints behaviour).
- Integration tests (MSTest, in `PublicApiIntegrationTests`) for `pageSize=-1` and `pageIndex=-1`.

## MODIFIED
- Negative paging input: from "reaches `Skip`/`Take` and does whatever the provider does" to "rejected before `ExecuteAsync`". Previous outcome is unverified (see REMOVED).
- Invalid requests no longer pay the unconditional `Task.Delay(1000)`, because validation runs before `ExecuteAsync` (inferred).
- No change to `CatalogFilterPaginatedSpecification`. It is also used by [CatalogViewModelService.cs](../../src/Web/Services/CatalogViewModelService.cs#L41), and its `take == 0` to `int.MaxValue` rewrite must stay.

## REMOVED
Behaviours the new path no longer guarantees for negative input:
- `pageSize = -n` (any `pageIndex`) reaching the repository. Outcome today is unknown and provider-dependent (inferred): `Take(negative)` may yield an empty 200 on the in-memory provider and an error on SQL Server.
- `pageIndex = -n` with `pageSize > 0` returning some page of results. `Skip(negative)` may be treated as 0 and return the first page (inferred, unverified). A client using `pageIndex=-1` as an alias for page 0 breaks.
- `pageSize < 0` and `pageIndex < 0` together producing a positive `skip` (`pageIndex * pageSize`) and a 200 (inferred).
- The `PageCount = totalItems > 0 ? 1 : 0` branch for negative `pageSize` (`request.PageSize > 0` else-branch at line 52). It stays reachable only for `pageSize = 0`.
- Every response from this route taking at least 1 s. Rejected requests return fast.
- A 200 `ListPagedCatalogItemResponse` (with `CorrelationId`) for negative input. The 400 body has a different shape and no `CorrelationId`.
- Error-shape parity with the rest of the API. Server errors use `ErrorDetails` JSON from [ExceptionMiddleware.cs](../../src/PublicApi/Middleware/ExceptionMiddleware.cs). The new 400 uses the FastEndpoints `ErrorResponse` instead (inferred).
- Server-side error logging and telemetry for negative input, if it previously threw and was caught by `ExceptionMiddleware` (conditional on the unverified 500 case). A 400 from validation is not an exception.
- No test pins any of the above, so a green suite proves nothing about removed behaviour. Existing tests cover only `pageSize=10`, `pageIndex=1`, and no parameters.

## REMOVED audit
- Generator missed: the delay no longer applying to invalid input; the `PageCount` else-branch becoming `pageSize = 0` only; the response-shape and `CorrelationId` difference; the logging/telemetry loss; the fact that no test pins negative-input behaviour.
- Genuine "complete" entries: the `PageCount` branch and the `Task.Delay` entries (read directly from the endpoint). The `pageSize`/`pageIndex` binding is read from the [request class](../../src/PublicApi/CatalogItemEndpoints/CatalogItemListPagedEndpoint.ListPagedCatalogItemRequest.cs). The provider-specific outcomes for negative `Skip`/`Take` are NOT complete: they are inferred and must be checked by running the current build (in-memory and SQL Server) before signing off.
- Not removed, but easy to confuse with removal: `pageSize = 0` meaning "all", `pageIndex` ignored when `pageSize = 0`, out-of-range `pageIndex` returning empty 200, case-insensitive query names (`PageSize` vs `pageSize`), anonymous access, and `int` overflow of `pageIndex * pageSize` for large positives (still unvalidated, out of scope).

## Risk note
- Highest-risk preserved behaviour: omitted or `0` `pageSize` returns the whole catalog.
- Why at risk: two callers depend on it with no parameters. [BlazorAdmin `List()`](../../src/BlazorAdmin/Services/CatalogItemService.cs#L84) calls `catalog-items` bare, and [ApiHealthCheck.cs](../../src/Web/HealthChecks/ApiHealthCheck.cs#L27) requires ".NET Bot Black Sweatshirt" in that response. A rule written as `GreaterThan(0)` instead of `>= 0`, or a validator that treats the defaulted `0` as invalid, returns 400 to both. The FastEndpoints validator is also a singleton, so it must hold no state.
- One test or check that proves it survives: MSTest in `PublicApiIntegrationTests` asserting `GET /api/catalog-items?pageSize=0` and `GET /api/catalog-items` both return 200 with the same item count, equal to the seeded catalog size.
