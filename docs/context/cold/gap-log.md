# Gap log

- Saved Items spec says login redirect is `/Identity/Account/Login`; code sets `LoginPath = "/Account/Login"` (`src/Web/Configuration/ConfigureCookieSettings.cs`). Unresolved.
- Saved Items spec caps DB round trips at 2 per request; the save path needs 3 with an explicit catalog-item check. Unresolved, see `specs/saved-items/plan.md`.
- Saved Items spec caps new production files at 5; reachable only by nesting specifications in the service file and counting the Razor pair as one. Unresolved.
- Saved Items spec says the service catches `DbUpdateException`, but `ApplicationCore` cannot reference EF Core. Plan uses a re-query after any `AddAsync` failure instead. Needs confirmation.
