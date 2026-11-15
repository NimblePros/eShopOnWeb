# Issue #344 — Replace microsoft.com demo account emails with eshop.test

https://github.com/NimblePros/eShopOnWeb/issues/344

## Assumptions

- All `@microsoft.com` email addresses (seeded accounts and throwaway test addresses) move to `@eshop.test`; local parts are unchanged.
- Existing local identity databases keep the old accounts (the seed only creates users). Developers recreate the identity DB to get the new ones; documented in the PR.
- Doc screenshots showing old addresses are not regenerated; only alt text is updated.
- Historical plan files in `.claude/plans` are left as-is (they describe past work).

## Phase 1 — Shared constants and seed data

- [x] Add `BlazorShared.Authorization.Constants.DemoUsers` (`DEMO_USER`, `PRODUCT_MANAGER`, `ADMINISTRATOR`) so BlazorAdmin can use them (it does not reference ApplicationCore)
- [x] Point `AuthorizationConstants.DEFAULT_ADMIN_USERNAME` at `DemoUsers.ADMINISTRATOR`
- [x] Seed demo and product manager users from the new constants

## Phase 2 — UI

- [x] Login page demo credentials hint shows `@eshop.test` users
- [x] BlazorAdmin `UserPage/List.razor` and `RolePage/Details.razor` use the constant instead of a hard-coded string

## Phase 3 — Tests

- [x] Update unit tests (`TransferBasket`, `SendVerificationEmailAsync`)
- [x] Update functional tests (`WebPageHelpers`, `AccountSignIn`, `CheckoutTest`, `ApiTokenHelper`, commented `AuthenticateEndpoint`)
- [x] Update PublicApi integration tests (auth, token helper, user management endpoints)
- [x] Add a unit test asserting the demo user constants use the `eshop.test` domain and that `DEFAULT_ADMIN_USERNAME` matches the seeded admin
- Existing functional/integration tests that log in as the seeded users cover the seed change end-to-end

## Phase 4 — Docs

- [x] README, `docs/getting-started-for-beginners.md`, `docs/features/role-membership.md`, `docs/features/user-management.md`

## Phase 5 — Independent review

- [x] Separate agent reviews full diff (correctness, missed occurrences, security, performance, accessibility)
- [x] Address significant findings

### Review findings

- Existing local DBs keep old accounts (old admin loses protection) — addressed with a README note and PR description call-out.
- Login.cshtml hard-coded addresses — switched to the `DemoUsers` constants.
- Test helpers still use literal addresses — kept deliberately: literals in tests catch an incorrect constant value.
- `DemoUserConstantsTests` mostly restates values — kept; the admin-alias test guards against the two admin constants drifting.
- Screenshot `eShopOnWeb-user-management-no-admin-delete.png` still shows `admin@microsoft.com` — out of scope (no regeneration); alt text updated.

## Phase 6 — Final verification

- [x] `dotnet ./BuildTestFormat.cs` passes (build, tests, format)
