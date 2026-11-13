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

- [x] Delete `BaseApiController`
- [x] `Program.cs`: drop `AddMvc` route-token convention, `AddControllersWithViews`,
      `MapControllerRoute`, slugify constraint; delete `SlugifyParameterTransformer`
- [x] Move `Views/Shared/*` and view imports into `Pages/`; delete `Views/` (incl. orphaned
      `Views/Account`), update `Web.csproj` content entries
- [x] Update docs (`docs/explore/patterns.md`, `docs/faq.md`, `docs/explore/tests.md`)
- [x] Move functional tests out of `Web/Controllers` folder
- [x] Verify

## Phase 5 — Independent review

- [x] Separate agent reviewed full diff vs. the issue: correctness, regressions, test gaps,
      security (authz on every page/endpoint, antiforgery), performance, accessibility.
      No high/medium findings; it confirmed `GET /User` JSON shape, routes BlazorAdmin uses,
      authz on every page, antiforgery on POSTs, and layout resolution in the Identity area.
- [x] Addressed:
  - Logout test now replays the pre-logout cookie and asserts it is revoked.
  - Added tests: anonymous redirect for disable-2fa / generate-recovery-codes,
    SendVerificationEmail handler, SetPassword POST, RemoveLogin (success and guard).
  - RemoveLogin now refuses server-side to remove a user's only sign-in method
    (pre-existing: only the UI hid the button).
  - SetPassword redirects straight to ChangePassword so "Your password has been set." shows
    (pre-existing: the message was consumed by an intermediate redirect).
  - `aria-current="page"` on the active Manage nav link.
  - New flow tests use throwaway users (`WebPageHelpers.CreateUserAsync`).
- Not addressed, with reasons:
  - Internal-only URLs changed shape (`/manage/disable2fa-warning` → `/manage/disable-2fa`,
    `*-warning` GETs folded into their pages, link-login callback is now a page handler) and
    `/order/detail/abc` is 404 rather than 400. Nothing links to the old URLs; noted in the PR
    instead of adding redirects.
  - `UpdatesPhoneNumber` still edits the demo user, as the original test did; the change is
    harmless to other tests and the class runs in the Sequential collection.
  - Pre-existing, out of scope (noted in PR): no antiforgery on `POST /User/Logout` (needs a
    BlazorAdmin change), `Disable2fa`/`GenerateRecoveryCodes` GET throw when 2FA is off,
    missing `alt` text / `javascript:` logout link in shared layout markup, and
    `GetOrderDetailsHandler` doesn't check the order belongs to the requesting user.

## Phase 6 — Final verification

- [x] `dotnet build eShopOnWeb.slnx`: 0 errors, no new warnings.
- [x] `dotnet test --solution eShopOnWeb.slnx`: 172 total, 171 passed, 0 failed, 1 skipped
      (the existing skipped EndToEnd test).
- [x] `dotnet format --verify-no-changes` is clean for every `.cs` file this branch touches.

## Phase 7 — Repo-wide formatting and BuildTestFormat.cs (follow-up request)

- [x] Line endings: `.gitattributes` says `eol=lf`, but 13 files were committed as CRLF and
      `.editorconfig` had no `end_of_line`, so `dotnet format` on Windows expected CRLF. Added
      `end_of_line = lf` and renormalized (line-ending-only commit).
- [x] Applied `dotnet format whitespace` + `style` across the solution: whitespace, using order,
      UTF-8 BOM. Did **not** apply `dotnet format analyzers` fixes: those removed
      `StateHasChanged()` calls and wrapped JS interop in empty try/catch blocks.
- [x] EF migrations marked `generated_code = true` so the formatter leaves them alone.
- [x] BlazorAdmin analyzer warnings that `--verify-no-changes` reports:
  - BL0012 fixed: removed redundant `StateHasChanged()` from EventCallback handlers.
  - BL0013 fixed: the user list reads the cascading `Task<AuthenticationState>`.
  - BL0016 lowered to suggestion, only for the one-line wrappers in `BlazorAdmin/JavaScript`.
- [x] `BuildTestFormat.cs`: uses `dotnet test --project` (MTP), parses MTP's summary, and
      reports skipped tests.
- [x] `dotnet ./BuildTestFormat.cs` exits 0: build OK, 171/172 tests pass (1 skipped, existing),
      and `dotnet format eShopOnWeb.slnx --verify-no-changes` is clean repo-wide.
- Follow-up issues created: #315 (order detail doesn't check the owner), #316 (logout CSRF).
