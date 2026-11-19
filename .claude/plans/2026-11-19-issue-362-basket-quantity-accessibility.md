# Issue #362: Improve accessibility of basket quantity inputs and validation errors

https://github.com/NimblePros/eShopOnWeb/issues/362 (found while reviewing #360)

On `src/Web/Pages/Basket/Index.cshtml` the per-line quantity input has no accessible name,
validation errors only appear in the page-level summary without saying which line is wrong,
the input gets no `aria-invalid`/`aria-describedby`, and the typed values are lost after a
failed update.

## Assumptions

- The site uses Bootstrap 3.4.1, so a visually hidden `<label class="sr-only">` gives each input
  its accessible name ("Quantity for <product>") without changing the layout.
- Per-line state (posted value, error message) is keyed by **basket item Id**, not by form
  index. The posted `Items[i]` index only matches the rendered row while the basket is
  unchanged; keying by Id stays correct if the basket changed in another tab.
- On a failed update the whole update is rejected (existing behavior), so **every** posted value
  is echoed back, not only the invalid ones. Line costs and the total keep showing the saved
  quantities, since nothing was saved.
- The validation summary switches to `ModelOnly` and gets one model-level error per invalid
  line that names the product. The per-field messages sit next to their inputs, so the summary
  would otherwise repeat the same generic text once per line.
- Posted lines whose Id is not in the shopper's basket are ignored, as `SetQuantities` does.

## Phase 1: Page model

- [x] In `OnPostUpdate`, when `ModelState` is invalid, collect per basket item Id the posted
      raw value and the first `items[i].Quantity` error (parsed from `ModelState` keys,
      case-insensitively).
- [x] Expose them as `PostedQuantities` and `QuantityErrors` on `IndexModel`.
- [x] Add a model-level error per invalid line: "<product>: <message>".

## Phase 2: View

- [x] Give each input a stable `id` (`basket-quantity-{itemId}`) and a `sr-only` `<label>`.
- [x] Render a per-line error `<span id="basket-quantity-{itemId}-error">` when the line is invalid,
      and set `aria-invalid="true"` and `aria-describedby` on the input.
- [x] Render the posted value on a failed update.
- [x] Switch the summary to `ModelOnly`; give it `role="alert"` so the failure is announced.

## Phase 3: Automated tests (FunctionalTests, `Web/Pages/Basket/IndexTest.cs`)

- [x] Each quantity input has a label naming the product, linked by `for`/`id`.
- [x] Valid page has no `aria-invalid`.
- [x] Out-of-range update: the input has `aria-invalid="true"` and `aria-describedby` pointing at
      an element holding the error, the summary names the product, and the posted value is echoed.
- [x] Fractional quantity (`2.5`, which a number input accepts but an int rejects): same, with the value echoed.
- [x] Two lines, one invalid: only that line is marked invalid; the valid line's typed value is
      echoed too.

## Phase 4: Independent review

- [x] A separate agent with fresh context reviews the full diff (correctness, security,
      performance, accessibility).
- [x] Address every significant finding, or record why it was skipped:
  - `role="alert"` on a summary present at page load is not reliably announced: the first
    invalid input now gets `autofocus`, so its label, invalid state and message are read, and the
    page title is prefixed with "Error:". Kept `role="alert"`; it does no harm.
  - The multi-line test didn't check that the summary names the right product: it now asserts
    the invalid line's product is named and the valid line's isn't, plus `aria-describedby`.
  - A tampered form that repeats a line id could pair one row's value with another row's error:
    the first row now wins for both. Test added. The pre-existing 500 when repeated ids are all
    valid (`ToDictionary` in `OnPostUpdate`) is filed as #369.
  - Costs and total show saved quantities next to echoed inputs: the summary now starts with
    "Your basket was not updated. Correct the quantities below and try again."
  - The non-numeric test was unrealistic (browsers blank invalid number values): replaced with
    `2.5`.
  - Skipped: with one quantity error plus a stale or tampered id elsewhere, only the quantity
    error is shown. The update is still rejected safely, and the stale-id case needs a tampered
    or very stale form.
  - Skipped: a null `ProductName` would give "Quantity for " / ": ..." text. Catalog items
    always have a name.

## Phase 5: Verification

- [x] `dotnet ./BuildTestFormat.cs` passes after the last code change.
