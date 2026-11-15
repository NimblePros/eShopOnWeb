# Issue #328 — PUT api/users bypasses root-admin protection and allows mass assignment

https://github.com/NimblePros/eShopOnWeb/issues/328

## Approach

- Centralize the root admin user name as `AuthorizationConstants.DEFAULT_ADMIN_USERNAME`
  and add an `ApplicationUser.IsDefaultAdmin()` extension. Use it in the seed, Delete,
  SaveRoles and Update endpoints instead of repeating the string literal.
- `UpdateUserEndpoint`:
  - Refuse to modify the default admin (400 with a validation error, matching SaveRoles).
  - Stop using `FromUserDto`. Copy only `UserName`, `Email`, `PhoneNumber` and `LockoutEnd`.
  - `EmailConfirmed` / `PhoneNumberConfirmed` are server controlled: the request values are
    ignored. Changing the email or phone number resets its confirmed flag (same as
    `UserManager.SetEmailAsync` / `SetPhoneNumberAsync`).
  - When the user name, email, phone number or lockout changes, rotate the security stamp so
    existing cookie sessions are invalidated. This is done in one save
    (`UpdateSecurityStampAsync` validates, normalizes and saves the user), so a failure
    never leaves the user partly updated.
  - Check the `IdentityResult` and return its errors as a 400 (e.g. duplicate user name).
- BlazorAdmin `Pages/UserPage/Edit.razor`: show "Email Confirmed" as read-only, since the
  API no longer accepts it.

## Assumptions

- `LockoutEnd` stays editable by admins: locking out an account is a legitimate admin action.
  The default admin is protected by the guard above.
- `TwoFactorEnabled` was never written by the API and stays that way.

## Out of scope

- `POST api/users` still copies `EmailConfirmed` / `PhoneNumberConfirmed` through
  `FromUserDto`. The issue is about PUT, and the create form sets these on purpose. Worth a
  follow-up issue.

## Phase 1 — Centralize the protected account

- [x] Add `AuthorizationConstants.DEFAULT_ADMIN_USERNAME`
- [x] Add `IsDefaultAdmin()` extension
- [x] Use it in seed, `DeleteUserEndpoint`, `SaveRolesForUserEndpoint`

## Phase 2 — Fix `UpdateUserEndpoint`

- [x] Guard the default admin
- [x] Copy only the allowed fields; reset confirmation flags when email / phone change
- [x] Rotate the security stamp on identity / lockout changes, in a single save
- [x] Surface `IdentityResult` errors as 400
- [x] Update the OpenAPI description (400)

## Phase 3 — BlazorAdmin

- [x] Make the Email Confirmed checkbox read-only in the edit dialog

## Phase 4 — Tests (`tests/PublicApiIntegrationTests/UserManagementEndpoints/UpdateUserEndpointTest.cs`)

- [x] 400 when updating the default admin, and the admin is unchanged afterwards
- [x] `EmailConfirmed` / `PhoneNumberConfirmed` in the request are ignored
- [x] Changing the user name rotates the security stamp (read via `UserManager` from the test host)
- [x] Duplicate user name returns 400 instead of a silent 200
- [x] `LockoutEnd` can still be set for a normal user
- [x] Replace the test that flipped `EmailConfirmed` on seeded `demouser` with one on a
      dedicated user (also covers #333)
- [x] Existing Delete / SaveRoles default-admin tests still pass

## Phase 5 — Independent review

- [ ] A separate agent reviews the full diff (correctness, security, performance, accessibility)
- [ ] Address significant findings

## Phase 6 — Verification

- [ ] `dotnet ./BuildTestFormat.cs` passes
