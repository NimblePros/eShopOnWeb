# Issue #342 — POST api/users lets clients set EmailConfirmed / PhoneNumberConfirmed

https://github.com/NimblePros/eShopOnWeb/issues/342

Follow-up to #328, which fixed the same mass assignment for PUT api/users.

## Approach

- `CreateUserEndpoint`: stop using `FromUserDto`. Build the new `ApplicationUser` from
  `UserName`, `Email`, `PhoneNumber` and `LockoutEnd` only, the same field set as
  `UpdateUserEndpoint`.
  - `EmailConfirmed` / `PhoneNumberConfirmed` are server controlled: the request values are
    ignored and new users start with both `false`.
  - An empty phone number (`""` from the form) is stored as `null`, as in PUT.
- Remove `ApplicationUserExtensions.FromUserDto`: nothing uses it any more.
- BlazorAdmin `Pages/UserPage/Create.razor`: remove the "Email Confirmed" checkbox, since the
  API no longer accepts it.
- The endpoint's OpenAPI description does not mention the fields, so it needs no change.

## Assumptions

- `LockoutEnd` stays settable on create: creating a locked-out account is a legitimate admin
  action, and PUT already allows it.
- `TwoFactorEnabled` was never written by the API on create and stays that way.

## Phase 1 — Fix `CreateUserEndpoint`

- [x] Copy only the allowed fields; confirmation flags start as `false`
- [x] Treat an empty phone number as null
- [x] Remove the now unused `FromUserDto`
- [x] Check the OpenAPI description (no mention of the fields; unchanged)

## Phase 2 — BlazorAdmin

- [x] Remove the Email Confirmed checkbox from the create dialog
- [x] Check BlazorAdmin unit tests and Playwright tests for references to it (none)

## Phase 3 — Tests (`tests/PublicApiIntegrationTests/UserManagementEndpoints/CreateUserEndpointTest.cs`)

- [x] `EmailConfirmed` / `PhoneNumberConfirmed` in the request are ignored (stored as `false`)
- [x] `UserName`, `Email`, `PhoneNumber` and `LockoutEnd` are stored
- [x] Mutation check: copying the flags again fails only `IgnoresConfirmationFlagsInRequest`

## Phase 4 — Verification

- [x] `dotnet build eShopOnWeb.slnx`: no new warnings
- [x] `dotnet test --project tests/PublicApiIntegrationTests` passes
- [x] `dotnet test --project tests/UnitTests` passes

## Phase 5 — Review follow-ups

- [x] `CreateUserEndpoint`: return 400 with the Identity errors when the create fails, as PUT does
      since #328
- [x] Create the user and its default password in one `CreateAsync(user, password)` call (local
      development only), so a rejected password leaves no user without one; drop the
      `AddPasswordAsync` call and the extra `FindByNameAsync` lookup
- [x] OpenAPI description: 201, 400 (problem details) and 409
- [x] BlazorAdmin create dialog: stop if the create is rejected (`result` is null; the error toast
      is already shown) instead of reading `result.UserId`; roles are only saved for a created user
- [x] Remove the "Two-Factor Authentication Enabled" checkbox from the create dialog (the API never
      saves it)
- [x] Test: an invalid user name returns 400 with the Identity error and creates no user
- [x] No password test: the password is the `DEFAULT_PASSWORD` constant, which passes validation,
      so a failing password cannot reach Identity from a request
- [x] Build, PublicApiIntegrationTests, UnitTests and `dotnet format --verify-no-changes` pass
