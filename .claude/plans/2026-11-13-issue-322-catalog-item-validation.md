# Issue #322 — UpdateCatalogItemEndpoint returns 500 instead of 400 for invalid input

https://github.com/NimblePros/eShopOnWeb/issues/322

## Approach

- Add FastEndpoints `Validator<T>` classes (FluentValidation) for `UpdateCatalogItemRequest`
  and `CreateCatalogItemRequest`. FastEndpoints runs them before the handler and returns
  400 with the validation errors.
- Remove the unused DataAnnotations attributes from `UpdateCatalogItemRequest`. FastEndpoints
  ignores them, so they only mislead readers.
- Brand and type ids that don't exist are checked in the endpoints against
  `IRepository<CatalogBrand>` / `IRepository<CatalogType>`. A missing one adds an error and
  the endpoint returns 400. This lives in the endpoint, not the validator, because
  FastEndpoints validators are singletons and the repositories are scoped.
- `ExceptionMiddleware` is not changed. Mapping every `ArgumentException` to 400 would hide
  real server bugs. The domain guard clauses stay as a backstop.

## Assumptions

- Rules: `Id > 0` (update only), `CatalogBrandId > 0`, `CatalogTypeId > 0`, `Name` and
  `Description` not empty, `Price > 0`. The old `[Range(1, 10000)]` upper bound on ids is
  dropped: ids above 10000 are valid, and an unknown id still returns 404 on update.
- The update endpoint checks that the item exists (404) before checking brand and type
  (400). Field-level validation (400) runs first of all, which is how FastEndpoints works.

## Phase 1 — Validation

- [ ] `UpdateCatalogItemRequestValidator`
- [ ] `CreateCatalogItemRequestValidator`
- [ ] Remove DataAnnotations from `UpdateCatalogItemRequest`
- [ ] Brand/type existence checks in update and create endpoints

## Phase 2 — Tests (`tests/PublicApiIntegrationTests`)

- [ ] `UpdateCatalogItemEndpointTest`: 400 for empty/missing name, empty description,
      price <= 0, brand id 0, type id 0, id 0, brand id not found, type id not found
- [ ] `CreateCatalogItemEndpointTest`: 400 for the same field rules and missing brand/type
- [ ] Existing tests for success, 404, 401/403 and 409 still pass

## Phase 3 — Independent review

- [ ] A separate agent reviews the full diff (correctness, security, performance, accessibility)
- [ ] Address significant findings

## Phase 4 — Verification

- [ ] `dotnet ./BuildTestFormat.cs` passes
