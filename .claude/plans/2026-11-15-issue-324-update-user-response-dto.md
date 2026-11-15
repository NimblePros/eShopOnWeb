# Issue #324 — PUT api/users response leaks PasswordHash and SecurityStamp

https://github.com/NimblePros/eShopOnWeb/issues/324

## Approach

- Change `UpdateUserResponse.User` from `ApplicationUser` to `UserDto` and map the
  reloaded user with the existing `ToUserDto()` extension, as `UserGetByIdEndpoint` does.
- Rename the misnamed endpoint class `UpdateRoleEndpoint` (in `UpdateUserEndpoint.cs`) to
  `UpdateUserEndpoint` so it matches its file and route.
- BlazorAdmin's `UserManagementService.Update` already deserializes this response as
  `GetUserResponse` (a `UserDto`), so no client change is needed.

## Out of scope (tracked separately)

- Root-admin protection and mass assignment on this endpoint: #328.
- Ignored `IdentityResult` from `UpdateAsync`: related to #320 / #321.

## Phase 1 — Fix

- [x] `UpdateUserResponse.User` becomes `UserDto`
- [x] Endpoint maps with `ToUserDto()`
- [x] Rename class to `UpdateUserEndpoint`

## Phase 2 — Tests (`tests/PublicApiIntegrationTests/UserManagementEndpoints`)

- [x] New `UpdateUserEndpointTest`:
  - 401 with no token, 403 for normal user and product manager tokens
  - 404 for unknown user id
  - Admin update of a dedicated user returns 200, the updated values, and a JSON body with
    no `passwordHash`, `securityStamp` or `concurrencyStamp`

## Phase 3 — Independent review

- [x] A separate agent reviews the full diff (correctness, security, performance, accessibility)
- [x] Address significant findings

## Phase 4 — Verification

- [x] `dotnet ./BuildTestFormat.cs` passes

## Review findings

- `GET api/roles/{roleName}/members` returned `List<ApplicationUser>`, leaking the same
  fields for every role member. **Fixed in this PR** (same bug class): the response now
  holds `List<UserDto>`. BlazorAdmin's `UserForMembership(UserName, Id)` still binds. Test
  added in `RoleMembershipGetByNameEndpointTest`.
- `ToUserDto()` never mapped `TwoFactorEnabled`. **Fixed.** `FromUserDto` still ignores it,
  so admins can't toggle 2FA through the API.
- The endpoint re-fetched the user after `UpdateAsync` with a `!` that could throw.
  **Fixed:** maps `existingUser` directly.
- The leak test was a denylist. **Fixed:** `Helpers/UserJsonAssert` asserts an allowlist of
  `UserDto` fields, and both endpoints use it.
- The existing success test asserted the GET's status instead of the PUT's. **Fixed.**
- Role endpoints return raw `IdentityRole` (`NormalizedName`, `ConcurrencyStamp`; not secret).
  **Not addressed:** it changes the BlazorAdmin contract and belongs in its own issue.
- The existing success test toggles `EmailConfirmed` on seeded `demouser@microsoft.com`.
  **Not addressed:** older code outside this fix, and no current test depends on that flag.
- `UserListEndpoint` has a leftover `Task.Delay(1000)`. **Not addressed:** unrelated.
