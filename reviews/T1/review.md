# T1 review

## Verdict: request changes

One blocker and one major, both small and fixable inside T1's three files. No spec ambiguity or missing evidence prevents a verdict.

### Blocker

- **B1. The T1 done-check is red.** `Anonymous_Get_RedirectsToLogin` fails at `SavedItemsPageTests.cs` L26. It compares `Location.OriginalString` to `/Account/Login`, but the actual value is the absolute `http://localhost/Identity/Account/Login?ReturnUrl=...` (session log, runs 3 and 4). T1's "Done" requires this test to pass, so no passing evidence exists for AC4.
  - Drivers: findings 3 (AC to line), 4 (test coverage), 5 (input shapes, malformed and empty rows).
  - Cause: the test is wrong, not the page. The page redirects to `/Identity/Account/Login`, which is what spec.md says. T1's "Done" wording (`/Account/Login`) contradicts the spec.
  - Fix: assert `new Uri(location, UriKind.RelativeOrAbsolute)` path equals `/Identity/Account/Login`, and update the T1 wording in `tasks.md` and the first gap-log entry. Replace `Location!` with an assertion that fails clearly when the header is missing.

### Major

- **M1. Unauthorised behaviour: hero banner.** `Index.cshtml` L7-L11 adds `main_banner_text.png` and the `esh-catalog-*` styles. Neither T1 nor spec.md asks for it, it adds a same-origin asset dependency, and no test covers it.
  - Drivers: findings 1 (authorised behaviour), 2 (environment assumptions), 3 (extras).
  - Fix: remove the section (preferred), or record it as an accepted addition in the follow-up.

### Minor and follow-ups (not blocking)

- **F1. No signed-in test.** The 200 response, the empty-state text and the catalog link are untested, and `ReturnUrl=/SavedItems` is not asserted (finding 4). Add before or with T6.
- **F2. Null `Identity.Name` rule.** The spec requires treating it as unauthenticated, and `[Authorize]` (`Index.cshtml.cs` L6) does not. It is harmless until `Identity.Name` becomes `BuyerId`, so track it for T4-T6 (findings 3, 5, 6).
- **F3. Hard-coded empty state.** `Index.cshtml` L16 is unconditional and becomes wrong once T6 lists items (finding 2).
- **F4. Antiforgery.** The first state-changing handlers arrive in T6 and T7, and they must require antiforgery (findings 5, 6).
- **F5. Speculative code.** The empty `OnGet` and the public `Client` property have no consumer. They are harmless and follow repo conventions (finding 7).

### Attack surface (finding 6)

One new endpoint (`GET /SavedItems`) and one new auth gate (`[Authorize]`). There is no tool call and no external service interaction. The page reads no request data. No security blocker.

### Verdict rule applied

- Approve needs no blockers. B1 is a blocker.
- Hold needs missing evidence or spec ambiguity. The failing run and the actual `Location` are recorded, and spec.md settles the login path, so neither applies.
- Request changes applies: at least one blocker (B1) plus one major (M1).

Re-review when B1 and M1 are resolved and the T1 test passes.
