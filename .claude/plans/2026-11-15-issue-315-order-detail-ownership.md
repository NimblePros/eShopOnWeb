# Issue #315 — Order detail page shows any user's order

https://github.com/NimblePros/eShopOnWeb/issues/315

## Approach

- Add `CustomerOrderWithItemsByIdSpec(buyerId, orderId)` in ApplicationCore that filters on
  both `Id` and `BuyerId`, so another user's order is never loaded from the database.
- `GetOrderDetailsHandler` uses it with `request.UserName` (the app stores the user name as
  `Order.BuyerId`, as `GetMyOrdersHandler` / `CustomerOrdersSpecification` already assume).
- Another user's order then gives `null` → existing 400 "No such order found for this user."
  The response is the same as for an unknown order, so it doesn't reveal that the order exists.
- `OrderWithItemsByIdSpec` stays as-is (still used by repository integration tests).

## Phase 1 — Fix

- [x] New spec `CustomerOrderWithItemsByIdSpec`
- [x] Handler uses the new spec

## Phase 2 — Tests

- [x] Unit (`tests/UnitTests/ApplicationCore/Specifications`): spec matches only when both
      buyer and id match
- [x] Unit (`tests/UnitTests/MediatorHandlers/OrdersTests/GetOrderDetails`): handler
      returns null for another buyer's order, maps the owner's order
- [x] Functional (`tests/FunctionalTests/Web/Pages/Order/MyOrdersOnGet`): user A places an
      order, user B gets 400 on `/order/detail/{id}`

## Phase 3 — Independent review

- [x] A separate agent reviews the full diff (correctness, security, performance, accessibility)
- [x] Address significant findings

## Phase 4 — Verification

- [x] `dotnet ./BuildTestFormat.cs` passes

## Review notes

- Addressed: the cross-user functional test now asserts the 400 body is identical to the
  unknown-order response; `ListsPlacedOrderWithWorkingDetailLink` reuses the new
  `PlaceOrderAndGetDetailUrlAsync` helper; `OrderWithItemsByIdSpec` has a doc comment
  pointing callers to the owner-filtered spec.
- Not addressed (pre-existing, out of scope): `Login.cshtml.cs` transfers the anonymous
  basket to the email exactly as typed rather than the stored `UserName`, so `BuyerId` can
  differ in case from `User.Identity.Name`. SQL Server's case-insensitive collation hides it.
- Not addressed: audit logging of cross-user attempts. Not requested by the issue.
