---
title: Tests
parent: Explore
---

We have a variety of tests in this application.

## Unit Tests

These are the smallest tests. We use the Builder pattern to assist us in creating `Address`, `Basket`, and `Order` objects in our tests.

The [ApplicationCore tests](https://github.com/NimblePros/eShopOnWeb/tree/main/tests/UnitTests/ApplicationCore) demonstrate concepts such as:

- [Testing entities](https://github.com/NimblePros/eShopOnWeb/tree/main/tests/UnitTests/ApplicationCore/Entities)
- [Testing services](https://github.com/NimblePros/eShopOnWeb/tree/main/tests/UnitTests/ApplicationCore/Services/)
- [Testing with the Specifications pattern](https://github.com/NimblePros/eShopOnWeb/tree/main/tests/UnitTests/ApplicationCore/Specifications)

The [MediatorHandlers/OrdersTests](https://github.com/NimblePros/eShopOnWeb/tree/main/tests/UnitTests/MediatorHandlers/OrdersTests) can give you an idea of how to write tests around Mediator handlers.

## Integration Tests

There are currently 2 projects for integration tests:

- [IntegrationTests](https://github.com/NimblePros/eShopOnWeb/tree/main/tests/IntegrationTests) - shows tests around repositories
- [PublicApiIntegrationTests](https://github.com/NimblePros/eShopOnWeb/tree/main/tests/PublicApiIntegrationTests) - shows tests around API endpoints

## Functional Tests

Some of the things seen in the functional tests include:

- [Signing in and posting forms with antiforgery tokens](https://github.com/NimblePros/eShopOnWeb/blob/main/tests/FunctionalTests/Web/Pages/Account/AccountSignIn.cs)
- [Testing Redirect on anonymous login](https://github.com/NimblePros/eShopOnWeb/blob/main/tests/FunctionalTests/Web/Pages/Order/MyOrdersOnGet.cs)
- [Testing pages that are secured by authorization](https://github.com/NimblePros/eShopOnWeb/blob/main/tests/FunctionalTests/Web/Pages/Manage/ManagePagesTests.cs)
- [Testing API endpoints in the web project](https://github.com/NimblePros/eShopOnWeb/tree/main/tests/FunctionalTests/Web/Endpoints/UserEndpoints)

## Architecture Tests

We have examples of architecture tests in the [sadukie/ArchUnitNET-tests branch](https://github.com/NimblePros/eShopOnWeb/tree/sadukie/ArchUnitNET-tests). Sadukie covers these architecture tests in:

- [Architecture Testing for .NET webinar](https://mailchi.mp/nimblepros/arch-testing-for-dotnet-recording)
- [Getting Started with Architecture Testing blog post](https://blog.nimblepros.com/blogs/getting-started-with-archunitnet/)

## Browser Test Strategy (Playwright)

Use Playwright to verify the system is wired together correctly from the user perspective, not to exhaustively prove every business rule.

### Bad Strategy vs Better Strategy

- Bad: test every combination of discounts, pricing rules, and validation rules through browser automation.
- Better: keep browser tests focused on critical user journeys and confidence checks at app boundaries; move rule combinations to unit/integration/API tests.

### eShopOnWeb Test Layer Matrix

| Scenario Type | Example in eShopOnWeb | Best Layer |
| --- | --- | --- |
| Smoke | Home page loads and seeded products are visible | Playwright end-to-end |
| Critical journey | Add item to basket and verify basket outcome | Playwright end-to-end |
| Business rule permutations | Pricing, discount, quantity edge combinations | Unit tests + integration tests |
| API contract behavior | Endpoint status, payload, auth policy | Public API integration tests |
| Fragile legacy UI coverage (temporary) | High-value flow with unstable selectors during UI transition | Playwright end-to-end with explicit cleanup plan |

## AI-Assisted Playwright Workflow

Use this workflow for agent-assisted test authoring while preserving human control over quality.

1. Ask the agent to explore catalog and basket flows and summarize candidate user journeys.
2. Ask for 3-5 scenario proposals and require each to be tagged as smoke, critical journey, or lower-level-test candidate.
3. Ask the agent to draft one Playwright C# test for a selected scenario.
4. Human review gate:
 - Preserve user intent.
 - Prefer role/text/test-id locators.
 - Keep assertions user-visible.
5. Run targeted tests first, then broader suite.
6. On failure, inspect trace, screenshot, and video artifacts.
7. Ask the agent to diagnose from failure output and artifacts, then apply minimal fix.

Reusable Level 2 refactor prompt:

- `.claude/skills/playwright-dotnet-refactor/assets/refactor-prompt-template.md`

## Resources

Here are more resources for learning about testing:

- [DevIQ - Testing](https://deviq.com/testing/testing-overview)
- [NimblePros on-demand webinar - Exploring Design Patterns for Testing](https://mailchi.mp/nimblepros/design-patterns-testing-recording)
- [NimblePros blog - Testing Techniques series](https://blog.nimblepros.com/series/testing-techniques/)
