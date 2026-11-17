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

## Out of scope

- The create form still shows an editable "Two-Factor Authentication Enabled" checkbox that the
  API never saves (#328 made the same checkbox read-only in the edit dialog).
- `CreateUserEndpoint` does not check the `IdentityResult` of `CreateAsync` / `AddPasswordAsync`,
  so a failed create (e.g. an invalid user name) sends no error response. PUT returns these
  errors as 400 since #328.

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
