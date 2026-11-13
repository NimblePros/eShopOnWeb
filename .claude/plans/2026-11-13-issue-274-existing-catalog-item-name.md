# Issue #274 — Fix typo in existingCatalogItem variable name

https://github.com/NimblePros/eShopOnWeb/issues/274

## Findings

The original misspelling (`existingCataloogItem`) in
`src/PublicApi/CatalogItemEndpoints/CreateCatalogItemEndpoint.cs` was already corrected on
`main` by commit beaa7fb, but that commit was never linked to the issue, so the issue stayed
open. The rename also dropped the whitespace before `=` and `>`
(`var existingCatalogItem= ...`, `if (existingCatalogItem> 0)`), and the variable actually
holds a **count** from `CountAsync`, not a catalog item.

**Assumption:** finishing the cleanup (accurate name + spacing) and adding coverage for the
duplicate-name path is in scope; no behavior changes.

## Phase 1 — Code fix
- [x] Rename `existingCatalogItem` to `existingCatalogItemCount` and restore normal spacing.

## Phase 2 — Automated tests
- [x] `tests/PublicApiIntegrationTests/CatalogItemEndpoints/CreateCatalogItemEndpointTest.cs`:
      add an integration test that creating a catalog item with a name that already exists
      returns `409 Conflict` (the branch touched by this change had no coverage).

## Phase 3 — Independent review
- [x] Separate agent reviews the full diff (correctness, security, performance, accessibility).
- [x] Address significant findings.

Review result: no significant findings. Applied the minor nit (`using System;` instead of a
fully-qualified `System.Guid`). Not addressed, as out of scope for #274: the existing
count-then-insert duplicate check is racy (needs a unique index on `Name`), and the test file's
namespace (`PublicApiIntegrationTests.AuthEndpoints`) doesn't match its folder.

## Phase 4 — Verification
- [x] `dotnet build eShopOnWeb.slnx` succeeds (the solution has to be named explicitly because
      the repo root holds more than one project/solution file).
- [x] `dotnet test --solution eShopOnWeb.slnx`: 126 passed, 0 failed.
- [x] `dotnet format --verify-no-changes` is clean for both files this change touches. Also fixed
      the existing `new{` whitespace violation in `CreateCatalogItemEndpoint.cs`.
      **Not clean repo-wide:** `main` already fails `dotnet format --verify-no-changes`
      (about 125 CHARSET/IMPORTS/WHITESPACE violations in unrelated files). That should be a
      separate follow-up.
