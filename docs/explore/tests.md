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

The [MediatorHandlers/OrdersTests](https://github.com/NimblePros/eShopOnWeb/tree/main/tests/UnitTests/MediatorHandlers/OrdersTests) can give you an idea of how to write tests around MediatR handlers.

## Integration Tests

There are currently 2 projects for integration tests:

- [IntegrationTests](https://github.com/NimblePros/eShopOnWeb/tree/main/tests/IntegrationTests) - shows tests around repositories
- [PublicApiIntegrationTests](https://github.com/NimblePros/eShopOnWeb/tree/main/tests/PublicApiIntegrationTests) - shows tests around API endpoints

## Functional Tests

Some of the things seen in the functional tests include:

- [Testing endpoints that are secured by authorization](https://github.com/NimblePros/eShopOnWeb/blob/main/tests/FunctionalTests/Web/Controllers/AccountControllerSignIn.cs)
- [Testing Redirect on anonymous login](https://github.com/NimblePros/eShopOnWeb/blob/main/tests/FunctionalTests/Web/Controllers/OrderControllerIndex.cs)

## Architecture Tests

We have examples of architecture tests in the [sadukie/ArchUnitNET-tests branch](https://github.com/NimblePros/eShopOnWeb/tree/sadukie/ArchUnitNET-tests). Sadukie covers these architecture tests in:
- [Architecture Testing for .NET webinar](https://mailchi.mp/nimblepros/arch-testing-for-dotnet-recording)
- [Getting Started with Architecture Testing blog post](https://blog.nimblepros.com/blogs/getting-started-with-archunitnet/)

## Resources

Here are more resources for learning about testing:

- [DevIQ - Testing](https://deviq.com/testing/testing-overview)
- [NimblePros on-demand webinar - Exploring Design Patterns for Testing](https://mailchi.mp/nimblepros/design-patterns-testing-recording)
- [NimblePros blog - Testing Techniques series](https://blog.nimblepros.com/series/testing-techniques/)