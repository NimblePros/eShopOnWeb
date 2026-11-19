# Issue #342: POST api/users lets clients set EmailConfirmed / PhoneNumberConfirmed

## Decision
Drop the fields from create. Confirmation is server controlled, as it is for update (#328).
An admin who needs a confirmed address can still use the confirmation flow.

## Phase 1: API
- `CreateUserEndpoint`: copy only `UserName`, `Email`, `PhoneNumber` (empty -> null) and `LockoutEnd`.
  `EmailConfirmed`, `PhoneNumberConfirmed` and `TwoFactorEnabled` stay at their defaults (false).
- Return `IdentityResult` errors as 400 (today a failed create falls through with no response body).
- Remove `ApplicationUserExtensions.FromUserDto` (no other callers).

## Phase 2: BlazorAdmin
- `Create.razor`: remove the Email Confirmed and Two-Factor checkboxes (neither is honoured).
- Don't save roles or close the dialog when create fails (mirrors Edit.razor).

## Phase 3: Tests (`CreateUserEndpointTest`)
- Ignores EmailConfirmed / PhoneNumberConfirmed / TwoFactorEnabled in the request.
- Persists UserName, Email, PhoneNumber, LockoutEnd.
- Stores an empty phone number as null.
- Returns 400 and creates nothing when Identity rejects the user (invalid user name).
- Returns 401 without a token.
