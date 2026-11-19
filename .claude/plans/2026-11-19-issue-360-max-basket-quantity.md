# Issue #360: Add a sensible per-line maximum basket quantity

https://github.com/NimblePros/eShopOnWeb/issues/360 (follow-up from #354 / #359)

## Decisions and assumptions

- The maximum is `BasketItem.MaxQuantity = 999`, a public const in ApplicationCore.
- `SetQuantity`, `AddQuantity` and the `BasketItem` constructor reject anything above it
  (they still throw `ArgumentOutOfRangeException`, as they do today).
- **Basket merge at login clamps.** `TransferBasketAsync` caps each merged line at
  `MaxQuantity` and doesn't throw. A failed login over basket contents is worse than
  silently capping.
- Adding a product from the catalog when that line is already at the max shows a status
  message on the basket page instead of a 500.
- The view model's `[Range]` uses `MaxQuantity`, and the basket quantity input gets a
  matching `max` attribute.
- Rows already stored above 999 aren't migrated. `BasketItem` has a private parameterless
  constructor so EF Core can load them without tripping the guard. A merge clamps them to
  999. Otherwise the user has to lower them. The data migration is tracked in #361.

## Phase 1: Domain

- [x] Add `BasketItem.MaxQuantity` and enforce it in `SetQuantity` and `AddQuantity`
- [x] Add `Basket.MergeItem` (clamping add) and use it in `BasketService.TransferBasketAsync`
- [x] Unit tests: BasketItemSetQuantity, BasketAddItem, BasketMergeItem, TransferBasket

## Phase 2: Web

- [x] `BasketItemViewModel.Quantity` `[Range(0, BasketItem.MaxQuantity)]` with a clear message
- [x] `max` attribute on the basket quantity input
- [x] `IndexModel.OnPostUpdate` reloads the basket when ModelState is invalid (otherwise the page renders empty)
- [x] `IndexModel.OnPost` shows a status message when the line is already at the max
- [x] Functional tests: updating to more than the max keeps the quantity and shows the error; updating to exactly the max succeeds; adding at the max shows the message

## Phase 3: Review

- [x] Review of the full diff by an independent agent with fresh context
- [x] Address significant findings:
  - **Fixed.** Medium: loading a stored row above 999 threw, because EF Core used the
    guarded constructor. That broke the basket page, checkout and login. Fixed with a
    private EF constructor. Integration tests in `LoadOverMaxQuantity`.
  - **Fixed (found while fixing the above).** `AddQuantity` on a row above the max threw
    `ArgumentException` (a negative range bound), not `ArgumentOutOfRangeException`.
    `AddQuantityUpToMax` could overflow when `Quantity` was near `int.MaxValue`.
  - **No change.** Low: the broad `ArgumentOutOfRangeException` catch in `OnPost`. With a
    quantity of 1 and a catalog item that exists, the max is the only way that call can
    throw. A pre-check would map the whole basket on every add.
  - **No change.** Low: no notice when a login merge clamps. The issue allows a silent
    clamp, and there's nowhere to show a message during login.
  - **Deferred to #362.** Low: accessibility of the quantity inputs and validation
    messages, and typed values lost after a failed update. Both predate this change.
  - **Deferred to #361.** Checkout returns 400 for stored rows above 999.

## Phase 4: Verify

- [x] `dotnet ./BuildTestFormat.cs` passes (build, all tests, format clean)
