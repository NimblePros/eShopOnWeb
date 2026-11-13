# Issue #308 — Migrate Away From Controllers

https://github.com/NimblePros/eShopOnWeb/issues/308

Controller actions returning views become Razor Pages (root `Pages/` folder); API actions
become FastEndpoints endpoints (root `Endpoints/` folder). One sub-issue per controller, one
commit per sub-issue, verified (build + tests + format) after each.

Sub-issues: #310 Order, #311 User, #312 Manage, #313 cleanup.

## Assumptions

- Public entry URLs stay the same: `/order/my-orders`, `/order/detail/{id}`,
  `/manage/my-account`, `GET /User`, `POST /User/Logout`. Pages get explicit `@page` routes
  so they keep the old slugified URLs. Internal-only URLs (2FA sub-pages, link-login
  callback) may change shape (page handlers instead of separate actions).
- Already-broken behaviour is fixed rather than preserved: missing
  `GenerateRecoveryCodesWarning` / `ShowRecoveryCodes` views, GET links to POST-only actions,
  the dead "Cancel" order link (no such action exists), and the email confirmation link that
  pointed at a non-existent `ConfirmEmail` controller.
- One PR closes #308 and all sub-issues (avoids conflicts between parallel PRs that touch
  `_LoginPartial` and `Program.cs`).

## Phase 1 — #310 OrderController → Razor Pages

- [x] `Pages/Order/MyOrders` (`@page "/order/my-orders"`) and `Pages/Order/Detail`
      (`@page "/order/detail/{orderId:int}"`), Mediator queries unchanged
- [x] `AuthorizeFolder("/Order")`; 400 when the order isn't found for the user
- [x] Update `_LoginPartial` and the orders list links; drop the dead Cancel link
- [x] Delete `OrderController` and `Views/Order`
- [x] Functional tests (FunctionalTests/Web/Pages/Order): anonymous redirect,
      authenticated listing renders, unknown order → 400
- [x] Verify

## Phase 2 — #311 UserController → FastEndpoints

- [x] Add FastEndpoints to Web, scanning only the Web assembly (same as PublicApi, #281)
- [x] `Endpoints/User/GetCurrentUserEndpoint` (`GET /User`, anonymous) and
      `Endpoints/User/LogoutEndpoint` (`POST /User/Logout`)
- [x] Delete `UserController`
- [x] Functional tests (FunctionalTests/Web/Endpoints): anonymous user info, authenticated
      user info with claims + token, logout returns 200
- [x] Verify

## Phase 3 — #312 ManageController → Razor Pages

- [x] `Pages/Manage/*` with `_Layout`, `_ManageNav`, `_StatusMessage`, `ManageNavPages`
- [x] Pages: MyAccount (+SendVerificationEmail handler), ChangePassword, SetPassword,
      ExternalLogins (+LinkLogin, LinkLoginCallback, RemoveLogin handlers),
      TwoFactorAuthentication, Disable2fa, EnableAuthenticator, ShowRecoveryCodes,
      ResetAuthenticator, GenerateRecoveryCodes
- [x] `AuthorizeFolder("/Manage")`
- [x] Fix `EmailConfirmationLink` to target the Identity `ConfirmEmail` page
- [x] Delete `ManageController`, `Views/Manage`, now-unused view models
- [x] Functional tests (FunctionalTests/Web/Pages/Manage): anonymous redirect, each page
      loads for the demo user, profile update, change-password error path (no state change)
- [x] Unit test for `EmailConfirmationLink` / email verification service still passing
- [x] Verify

## Phase 4 — #313 Remove MVC controller infrastructure

- [ ] Delete `BaseApiController`
- [ ] `Program.cs`: drop `AddMvc` route-token convention, `AddControllersWithViews`,
      `MapControllerRoute`, slugify constraint; delete `SlugifyParameterTransformer`
- [ ] Move `Views/Shared/*` and view imports into `Pages/`; delete `Views/` (incl. orphaned
      `Views/Account`), update `Web.csproj` content entries
- [ ] Update docs (`docs/explore/patterns.md`, `docs/faq.md`, `docs/explore/tests.md`)
- [ ] Move functional tests out of `Web/Controllers` folder
- [ ] Verify

## Phase 5 — Independent review

- [ ] Separate agent reviews full diff vs. the issue: correctness, regressions, test gaps,
      security (authz on every page/endpoint, antiforgery), performance, accessibility
- [ ] Address all significant findings (record skipped ones here)

## Phase 6 — Final verification

- [ ] `dotnet build && dotnet test && dotnet format --verify-no-changes` clean
