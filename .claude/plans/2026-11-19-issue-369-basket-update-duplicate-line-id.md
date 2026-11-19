# Issue #369: Basket update returns 500 when the form repeats a line id

https://github.com/NimblePros/eShopOnWeb/issues/369 (found while reviewing #362)

`IndexModel.OnPostUpdate` builds its update with `items.ToDictionary(b => b.Id.ToString(), ...)`.
A tampered form that posts the same basket item id twice with valid quantities makes
`ToDictionary` throw `ArgumentException`, so the page returns 500.

## Assumptions

- The issue offers two fixes: reject the post with a model error, or take the first/last value.
  Rejecting is chosen: a repeated id is never produced by the rendered form, so guessing which
  quantity the shopper meant would silently save something they may not have intended.
- The rejection reuses the existing `UpdateFailedMessage` summary error (the same message shown
  for a tampered, unparseable line id) and re-renders the saved basket. Nothing is saved.
- When the repeated lines also carry an invalid quantity, the existing invalid-ModelState path
  runs first and keeps its current behavior (covered by
  `OnPostUpdateKeepsFirstValueAndErrorTogetherGivenRepeatedLineId`).

## Phase 1: Fix

- [x] In `OnPostUpdate`, after the ModelState check, detect repeated line ids; if any, add
      `UpdateFailedMessage` as a model-level error, show the saved basket, and return without
      calling `SetQuantities`.

## Phase 2: Automated tests (FunctionalTests)

- [x] `IndexTest.OnPostUpdateShowsSummaryErrorAndSavesNothingGivenRepeatedLineIdWithValidQuantities`:
      posting the same id twice with valid quantities returns 200, shows `UpdateFailedMessage`,
      and the stored quantity stays 1.

## Phase 3: Independent review

- [x] A separate agent reviews the full diff (correctness, security, performance, accessibility).
- [x] Address all significant findings or record why they were skipped.

## Phase 4: Verification

- [x] Run `dotnet ./BuildTestFormat.cs`: build, all tests, and format all clean.

## Results

- The new test failed (500) with the fix removed and passes with it.
- Review: no significant findings. It noted that posts omitting `Items[n].Id` bind `Id = 0` and are
  now also rejected as repeats rather than returning 500.
- `dotnet ./BuildTestFormat.cs`: build OK, all tests pass (FunctionalTests 79/79), format clean.
