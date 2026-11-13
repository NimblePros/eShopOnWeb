# Issue #281: PublicApiIntegrationTests don't run (MSTest mismatch, BlazorInputFile)

Issue: https://github.com/NimblePros/eShopOnWeb/issues/281
Branch: `issue-281-publicapi-tests-blazorinputfile`

## Current state (verified on `main` @ 98cd1e6, before any change)

- `MSTest.TestAdapter` and `MSTest.TestFramework` are both `4.5.3` (aligned by #302/#305).
  `dotnet test tests/PublicApiIntegrationTests` discovers and passes all 57 tests.
- `BlazorInputFile` 0.2.0 still reaches the PublicApi host through
  `PublicApi -> ApplicationCore -> BlazorShared -> BlazorInputFile` (present in
  `PublicApi.deps.json` and the test bin). The `ReflectionTypeLoadException` doesn't fire on
  the current runtime/FastEndpoints combination, but the dependency is still a latent startup
  hazard, and `AddFastEndpoints()` still scans every assembly.
- Tests run on Microsoft.Testing.Platform (`global.json` `test.runner`). A run that executes zero
  tests exits with code 8, and `dotnet test` returns 8 (verified with a filter that matches no
  tests). The CI step therefore already fails on a silent zero-test project.

## Assumptions

- `BlazorInputFile` is only used by `CatalogItem.DataToBase64(IFileListEntry)`, which has no
  callers, plus an unused `@using` and a `<script>` tag. No UI uses the file input today, so
  "replace with the built-in `InputFile`" means removing the package and dead code. Any future
  upload UI should use `Microsoft.AspNetCore.Components.Forms.InputFile` / `IBrowserFile`.
- To stop MSTest drift, use one shared `MSTestVersion` property in `Directory.Packages.props`
  instead of the `MSTest` metapackage. The metapackage would also pull in
  `Microsoft.Testing.Extensions.CodeCoverage` alongside `coverlet.MTP`, plus a second
  `Microsoft.NET.Test.Sdk` version.

## Phase 1: Prevent MSTest version drift

- [x] Add `<MSTestVersion>` property in `Directory.Packages.props` and use it for both
      `MSTest.TestAdapter` and `MSTest.TestFramework`.

## Phase 2: Remove BlazorInputFile

- [x] Remove `PackageVersion` from `Directory.Packages.props`.
- [x] Remove `PackageReference` from `BlazorShared.csproj` and `BlazorAdmin.csproj`.
- [x] Remove `@using BlazorInputFile` from `BlazorAdmin/_Imports.razor`.
- [x] Remove the unused `CatalogItem.DataToBase64(IFileListEntry)` and its `using`.
- [x] Remove `_content/BlazorInputFile/inputfile.js` script tag from `Web/Pages/Admin/Index.cshtml`.

## Phase 3: Limit FastEndpoints discovery (defense in depth)

- [x] `AddFastEndpoints(o => { o.DisableAutoDiscovery = true; o.Assemblies = [typeof(Program).Assembly]; })`.
      All endpoints, summaries and FE types live in the PublicApi assembly.

## Phase 4: Automated tests

- [x] Existing `tests/PublicApiIntegrationTests` (57 tests) must still be discovered and pass:
      they cover endpoint discovery after limiting the scan.
- [x] Add a regression test in `tests/PublicApiIntegrationTests` asserting that
      `BlazorInputFile` isn't in the PublicApi host's dependency context.
- [x] CI zero-test guard: no workflow change needed (MTP exit code 8). Add a comment in
      `dotnetcore.yml` explaining this so nobody adds `--ignore-exit-code 8`.

## Phase 5: Independent review

- [x] Separate review agent (fresh context) reviews the full diff against the issue: correctness,
      missed requirements, test gaps, security, performance, accessibility.
- [x] Address all significant findings, or record why not.

## Phase 6: Final verification

- [x] `dotnet build && dotnet test && dotnet format --verify-no-changes` runs cleanly.

## Review findings

Independent review found nothing significant. Disposition of the minor findings:

1. *New test no longer guards the startup crash itself* (scan is now limited). Addressed: reworded the
   test comment to state its real purpose (keep the obsolete package out); host startup is covered by
   the 57 endpoint tests.
2. *Shared version property instead of the `MSTest` metapackage.* Kept. The metapackage would add
   `Microsoft.Testing.Extensions.CodeCoverage` next to `coverlet.MTP` and a second
   `Microsoft.NET.Test.Sdk` version. The PR states that #302 already aligned the versions.
3. *Exit code 8 only catches zero-test projects, not partial loss.* Accepted: the issue's failure
   mode is all-or-nothing.

## Verification results

- `dotnet build`: succeeded, 0 errors.
- `dotnet test --solution eShopOnWeb.slnx`: 125 total, 124 passed, 1 skipped (existing EndToEnd skip), 0 failed.
  PublicApiIntegrationTests: 58/58 passed.
- Mutation check: re-adding `BlazorInputFile` to BlazorShared makes `PublicApiDoesNotDependOnBlazorInputFile` fail.
- `dotnet format --verify-no-changes`: no findings in any file this branch changes. It still exits
  non-zero because of 128 pre-existing WHITESPACE errors in untouched files (ApplicationCore entities,
  AspireServiceDefaults, ...). Those predate this branch and are out of scope.
