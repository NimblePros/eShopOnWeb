# Issue #316 — POST /User/Logout has no antiforgery protection

https://github.com/NimblePros/eShopOnWeb/issues/316

## Approach

- New `GET /User/AntiforgeryToken` FastEndpoint (anonymous) calls
  `IAntiforgery.GetAndStoreTokens`, which sets the antiforgery cookie, and returns the request
  token as JSON. A cross-site page can't read that response (no CORS), so it can't get a token.
- `LogoutEndpoint` checks `IAntiforgery.IsRequestValidAsync` before signing out and returns
  400 when the check fails. The token travels in the `RequestVerificationToken` header (the
  ASP.NET Core default, which Razor Pages already use).
- BlazorAdmin `Logout.razor` fetches the token right before logging out and sends it in that
  header. Fetching at logout time means the token is bound to the current user. A token issued
  before sign-in would fail validation.

## Assumptions

- FastEndpoints' `EnableAntiforgery()` was not used: its middleware only checks form
  content types, and logout posts an empty body. Checking `IAntiforgery` directly in the
  endpoint covers header tokens.
- An anonymous logout with a valid token still returns 200 (same as before for anonymous
  callers).

## Phase 1 — Web endpoints

- [x] `Endpoints/UserEndpoints/GetAntiforgeryTokenEndpoint` (`GET /User/AntiforgeryToken`)
- [x] `LogoutEndpoint` validates the antiforgery token, 400 on failure, before any sign-out

## Phase 2 — BlazorAdmin

- [x] `Logout.razor` gets the token and posts with the `RequestVerificationToken` header

## Phase 3 — Tests (FunctionalTests/Web/Endpoints/UserEndpoints)

- [x] Logout without a token → 400, user is still signed in
- [x] Logout with an invalid token → 400
- [x] Logout with a token → 200, user signed out (existing test updated)
- [x] Logout with a token still revokes the cookie on replay (existing test updated)
- [x] Anonymous logout with a token → 200
- [x] Token endpoint returns a token and sets the antiforgery cookie

## Phase 4 — Independent review

- [ ] Separate agent reviews the full diff (correctness, security, performance, accessibility)
- [ ] Address significant findings, or record here why they were not

## Phase 5 — Verification

- [ ] `dotnet ./BuildTestFormat.cs` passes (build, tests, format)
