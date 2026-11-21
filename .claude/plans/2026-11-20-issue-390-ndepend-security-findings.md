# Issue #390: NDepend security rule findings (ND31xx)

Issue: https://github.com/NimblePros/eShopOnWeb/issues/390
Parent: #98 (Address Issues from NDepend Report)

## Current state of each finding

The NDepend sample report was taken from an older snapshot. Each finding below was checked against `main` as of 2026-11-20.

| Rule | Report location | Current state | Action |
|---|---|---|---|
| ND3114 hard-coded secret | `AuthorizationConstants.DEFAULT_PASSWORD` | The documented demo password. It is used to seed demo users (local development, or when `SeedDemoUsers` is turned on explicitly) and to give admin-created users a password (local development only) | Justify; no change |
| ND3114 (same class) | `AuthorizationConstants.AUTH_KEY` | Hard-coded key with no references; JWT uses `JwtSettings.SigningKey` | **Remove** |
| ND3117 cookies | `ConfigureCookieSettings`, basket cookie in Checkout/Index | Already fixed: app cookie sets `HttpOnly` + `SecurePolicy.Always`; `BasketCookie.CreateOptions()` sets `HttpOnly` + `Secure` | None |
| ND3127 secrets in logs | `Login`/`Register` `OnPostAsync` | Log messages are constants; nothing user-supplied is logged. The rule only flags that the method both reads `Input.Password` and logs | False positive; no change |
| ND3130 anonymous state change | `UserController.Logout` (now `LogoutEndpoint`) | `AllowAnonymous` kept on purpose: requires a valid antiforgery token, and signing out an expired session must still work | Justify; no change |
| ND3131 security ops that log nothing | `ManageController` (now `Pages/Manage/*`) | `ExternalLogins` (link, link callback, remove), `SetPassword`, and the `MyAccount` email/phone change log nothing | **Add audit logging** |
| ND3133 HTML-encoding bypass | `EditCatalogItem.cshtml` `readonly`, `MyAccount.cshtml` `disabled` | Minimized attributes on tag helpers compile to `Html.Raw(...)` | **Use `readonly="readonly"` / `disabled="disabled"`** (same HTML semantics) |

## Phase 1: Remove the unused hard-coded key
- [x] Delete `AuthorizationConstants.AUTH_KEY`.

## Phase 2: Audit logging for account-security operations
- [x] `ExternalLoginsModel`: log a link request (user id, provider), a link added, a link failure (warning), a login removed, a removal failure, and a refused removal of the only sign-in method (warning).
- [x] `SetPasswordModel`: log that a password was set, or that setting it failed (warning).
- [x] `MyAccountModel`: log email and phone number changes by user id. Never log the values themselves.
- [x] Log only user ids and provider names, never emails, phone numbers, passwords or provider keys.

## Phase 3: HTML-encoding bypasses
- [x] `EditCatalogItem.cshtml`: `readonly` → `readonly="readonly"`.
- [x] `MyAccount.cshtml`: `disabled` → `disabled="disabled"`.

## Automated test requirements
- [x] FunctionalTests: register a capturing `ILoggerProvider` in `TestApplication`.
- [x] Extend the `AccountManagementFlowTests` for set password, remove login and refused removal to assert the audit entry is written.
- [x] Add a test showing that changing the profile email logs the change without logging the email address.
- [x] Existing functional tests for the Manage pages and the admin edit page still pass.

## Phase 4: Independent review
- [x] A separate agent with fresh context reviews the full diff. It found no bugs and no PII leaks.
- [x] Only log provider names the app offers or the user really has: unknown providers are rejected in LinkLogin, and RemoveLogin looks the login up first (log forging, CWE-117). This changes behavior: removing a login the user doesn't have now shows an error instead of a false success. A test covers it.
- [x] Write the audit entry before `SignInAsync`.
- [x] Make the Manage folder consistent: `ChangePassword` logs the user id and failures; `EnableAuthenticator` logs invalid codes; `Disable2fa` logs failures; all messages use `{UserId}` without quotes.
- [x] Check for email leaks across all log categories in the test, and filter the captured logs to app entries plus Microsoft/System warnings.
- [x] Make the functional tests use a unique provider key per test. They had depended on test order, because a provider key can belong to only one user.
- [x] Filed #398 as a follow-up: `LoggerEmailSender` logs full email bodies (including confirmation tokens) in every environment.
- Not changed: the Warning before each `throw` duplicates the Error the exception handler logs. Kept as an explicit audit line.

## Phase 5: Verification
- [x] `dotnet ./BuildTestFormat.cs` passes (build, all tests, `dotnet format --verify-no-changes`).

## Assumptions
- Findings that are already fixed or are false positives get an explanation in the PR and no code change. Suppression attributes are not added, because this repo doesn't define `CODE_ANALYSIS` and doesn't run NDepend in CI.
