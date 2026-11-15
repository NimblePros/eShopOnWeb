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
  `FromUserDto`. The issue is about PUT, and the create form sets these on purpose.
  Tracked in #342.

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

- [x] A separate agent reviews the full diff (correctness, security, performance, accessibility)
- [x] Address significant findings

## Phase 6 — Verification

- [x] `dotnet ./BuildTestFormat.cs` passes

## Review findings

The reviewer found no way left to bypass the default-admin protection. Renaming another user to
the admin name is rejected as a duplicate; lookalike Unicode names fail the allowed-characters
check; the Administrators role can't be renamed or deleted while it has members.

- `IgnoresConfirmationFlagsInRequest` also changed email/phone, so the reset branch hid whether
  the flags were really ignored. **Fixed:** sends the current values with only the flags flipped.
- No test that `EmailConfirmed` survives an unchanged email or resets on a changed one.
  **Fixed:** `KeepsEmailConfirmedGivenUnchangedEmail` / `ResetsEmailConfirmedGivenChangedEmail`.
- The comment said rotating the stamp "ends existing sessions". The API uses JWTs, which are
  not checked against the stamp. **Fixed:** comment now says only Web cookie sessions are
  affected. Revoking JWTs is not addressed: it is a broader token-lifetime change.
- Stamp rotation was only tested for a user name change. **Fixed:** added a lockout-only test,
  and a no-change test.
- The admin test would rename and lock out the seeded admin if the guard regressed. **Fixed:**
  it only changes `LockoutEnd`, to a date in the past. A mutation check (guard disabled) fails
  only this test.
- BlazorAdmin saved roles and closed the dialog even when the update failed. **Fixed:** returns
  early, so the dialog stays open after the error toast. Showing the server's error messages
  would need `HttpService` changes and is not addressed.
- `TwoFactorEnabled` checkbox was editable but never saved. **Fixed:** read-only.
- An empty phone field posts `""` against a stored `null`, so it counted as a change (stamp
  rotated, phone confirmation reset). **Fixed:** `""` is treated as null.
  Not addressed: the date-only `InputDate` for `LockoutEnd` drops the time of day. This was
  already the case before this change.
- POST api/users mass assignment. **Not addressed:** out of scope, opened #342.
