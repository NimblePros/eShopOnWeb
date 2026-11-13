# Issue #302: Update to net11 and update all packages

Issue: https://github.com/NimblePros/eShopOnWeb/issues/302
Branch: `issue-302-net11-package-updates`

## Assumptions

The issue has no body or comments, so scope is taken from the title:

- "Update to net11" means every project targets `net11.0`, the SDK pin moves to 11.0, and
  everything that names a .NET version (CI, Dockerfiles, README/docs) moves with it.
- "Update all packages" means every `PackageVersion` in `Directory.Packages.props` moves to
  its latest **stable** release. Prerelease packages that have no stable release stay on the
  latest prerelease. Packages that are deprecated or no longer needed on .NET 11 are noted,
  not silently removed, unless they break the build.
- Major-version bumps with breaking changes are taken if the code can be adapted with small
  changes. Any that need a larger rewrite are pinned and listed as follow-up work.

## Phase 1: Retarget to .NET 11

- [x] `Directory.Packages.props` `TargetFramework` → `net11.0`
- [x] `src/eShopWeb.AppHost` and `src/eShopWeb.AspireServiceDefaults` `TargetFramework` → `net11.0`
- [x] `global.json` SDK version → `11.0.100`
- [x] `.github/workflows/*.yml` `dotnet-version` → `11.0.x`
- [x] `src/Web/Dockerfile`, `src/PublicApi/Dockerfile` base images → `11.0`
- [x] README / docs version references → 11.0

## Phase 2: Update packages

- [x] Microsoft.* / System.* / EF Core / ASP.NET Core packages → 11.0.x
- [x] Aspire, OpenTelemetry, Microsoft.Extensions.* resilience/service discovery → latest
- [x] Third-party packages (Ardalis.*, FastEndpoints, FluentValidation, Mediator, etc.) → latest
- [x] Test packages (xunit.v3, Test SDK, NSubstitute, Playwright, MSTest, coverlet) → latest
- [x] Fix any compile errors, new warnings-as-errors, or API changes caused by the updates
- [x] Re-run `dotnet list package --outdated` to confirm nothing is left behind

### Decisions made during Phase 2

- **Aspire AppHost SDK** (`Aspire.AppHost.Sdk/13.4.6` in the AppHost csproj, outside CPM)
  → 13.6.5. 13.4.6 has a critical (GHSA-4g86-54fm-h9h8) and a high (GHSA-f7vh-h5g3-7jrh)
  advisory.
- **AutoMapper removed.** `AutoMapper.Extensions.Microsoft.DependencyInjection` 12.0.1 is
  deprecated and pulls in AutoMapper 12.0.1 (high, GHSA-rvv3-g6hj-g44x). The only patched
  versions (15.1.1+, 16.1.1+) are under AutoMapper's commercial license. It was used for three
  trivial maps in PublicApi (Web referenced it but never used it), and `Program.cs` already had
  a TODO to remove it, so the maps are now explicit `ToDto()` extension methods.
- **Microsoft.Extensions.Caching.Memory pinned to 11.0.0 in the AppHost.** `NimblePros.Metronome`
  (already latest, 0.4.1) depends on EF Core 8, which brings in Caching.Memory 8.0.0
  (high, GHSA-qj66-m88j-hmgj). Only the AppHost resolved the old version.
- **Removed framework-provided package references** that NuGet flagged with NU1510 on net11
  (10 `System.*` references in Web/BlazorAdmin), and removed central versions that no project
  referenced (`System.Security.Claims`, `System.Text.Encoding`, `System.Text.Json`,
  `Microsoft.Win32.Primitives`, and the long-deprecated `Microsoft.AspNetCore.Mvc` 2.2.0).
- **OpenTelemetry.Exporter.Prometheus.AspNetCore** → `1.21.0-beta.1` (no stable release exists;
  matches the OpenTelemetry 1.21 core packages). **Mediator** stays on 3.0.2: 3.1 is only an RC.
  **Microsoft.VisualStudio.Web.CodeGeneration.Design** is already the newest (preview) build.
- **Moved tests to Microsoft.Testing.Platform (MTP).** xunit.v3 4.x ships MTP 2, which no longer
  supports the VSTest mode of `dotnet test` on the .NET 10+ SDK. `global.json` now opts into the
  MTP mode; the MSTest project enables MSTest's own runner; `coverlet.collector` (a VSTest
  collector) is replaced with `coverlet.MTP`; the CI test command and coverage summary step are
  updated to match.
- **EF Core 11 owned-entity check.** `OrderBuilder` handed the same `CatalogItemOrdered` instance
  to several `OrderItem`s, which EF Core rejects for owned types, so
  `GetOrderAndItemsByOrderIdWhenMultipleOrdersPresent` failed. The builder now returns a new
  instance each time, the same way `OrderService` does. Production code was not affected.
- **CI was already red on main** because Playwright browsers were never installed, so the two
  EndToEnd tests failed with "Executable doesn't exist". Added an install step.

## Phase 3: Tests

No new behaviour is added. The existing suites are the regression check for the upgrade, plus
tests that pin down the behaviour AutoMapper used to provide:

- [x] New: `CatalogBrandListEndpointTest` / `CatalogTypeListEndpointTest` (PublicApiIntegrationTests)
      check every seeded brand/type is returned with `Name` mapped from `Brand`/`Type`.
      Written and passing **before** AutoMapper was removed.
- [x] New: `MapsEveryCatalogItemFieldTheSameAsGetById` checks the paged list maps every field
      the same way as the (hand-mapped) get-by-id endpoint.
- [x] `DataTestMethod` → `TestMethod` (MSTEST0044 obsolete in MSTest 4.5)
- [x] UnitTests pass (49)
- [x] IntegrationTests pass (3)
- [x] FunctionalTests pass (12)
- [x] PublicApiIntegrationTests pass (57)
- [x] EndToEndTests (Playwright) pass locally (2 passed, 1 intentionally skipped demo)

## Phase 4: Independent review

- [ ] A separate agent with fresh context reviews the full diff: missed version references,
      breaking changes in updated packages, security (vulnerable/deprecated packages),
      performance regressions, and accessibility of any UI-affecting changes
- [ ] Address all significant findings, or record here why one was not addressed

## Phase 5: Final verification

- [ ] `dotnet build && dotnet test && dotnet format --verify-no-changes` runs cleanly

### Verification notes

- The bare verify command can't run in this repo: the root has both `eShopOnWeb.slnx` and a
  tracked `docker-compose.dcproj`, so `dotnet build` fails with MSB1011. The steps are run
  against `eShopOnWeb.slnx` explicitly.
- `dotnet format --verify-no-changes` **already fails on main** (131 findings: CHARSET,
  IMPORTS, WHITESPACE across ~60 files). This branch introduces none and removes 3 (BOMs on
  files added in #303). Reformatting the whole repo is deferred to its own PR so this upgrade
  stays reviewable.

## Deferred / follow-up

- Repo-wide `dotnet format` cleanup (see above).
- New .NET 11 Blazor analyzer warnings in BlazorAdmin (BL0012 unnecessary `StateHasChanged`,
  BL0013 auth state subscription, BL0016 unguarded JS interop) and ASPIRE010. These are
  warnings only and point to code changes beyond a version upgrade.
- `actions/setup-dotnet@v1` is very old; consider moving to a current major version.
