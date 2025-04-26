---
title: Patterns
parent: Explore
---

The eShopOnWeb reference application employs a number of software design patterns which are implemented either in the code itself, or with the assistance of various open source NuGet packages.

- [MVC](#mvc)
- [Razor Pages](#razor-pages)
- [API Endpoints](#api-endpoints)
- [Mediator](#mediator)
- [Repository](#repository)
- [Specification](#specification)
- [Guard Clauses](#guard-clauses)
- [Entities](#entities)
- [Aggregates](#aggregates)
- [Resources](#resources)

## MVC

The front end of the application's web project implements the class Model-View-Controller pattern. It includes a number of controllers in its `Controllers` folder, which work with Views to return rendered HTML to the client, typically a browser. For example, the `OrderController` class includes an action method that returns a Detail view. The view is in the /Views/Order/Detail.cshtml file, and binds to a model of type `OrderViewModel` to render specific data within the razor view template.

The sample also demonstrates the use of alternate flavors of the MVC pattern, including Razor Pages and API Endpoints, described in more detail below.

## Razor Pages

Razor Pages are an ASP.NET Core feature that reorganizes MVC's controllers, action methods, and views. Instead of having controllers with many often unrelated action methods which in turn were bound to many views in a separate folder structure, Razor Pages replaces views with a page metaphor and uses handlers attached to the page instead of actions. Razor Pages also provide their own strongly typed viewmodel, eliminating the need for a separate folder full of viewmodels in many cases.

## API Endpoints

API Endpoints reorganize APIs in much the same way Razor Pages reorganize controllers and views. Instead of having many unrelated endpoints as separate actions in a controller (or Program) class, each web API endpoint is encapsulated in its own class, along with its associated model types. Many .NET developers already use the mediator pattern (below) to achieve organization; API Endpoints eliminates the need to use a mediator and instead works by simply extending the `ControllerBase` class.

## Mediator

Some parts of the application use the [mediator design pattern](https://deviq.com/design-patterns/mediator-pattern) to decouple initiating actions from their implementations. These actions can be in the form of commands that expect a result, or events that have no expected response. The sample uses [the MediatR package](https://github.com/jbogard/MediatR) for its implementation of the pattern, which you will see used in the Web project.

## Repository

The [Repository pattern](https://deviq.com/design-patterns/repository-pattern) enables loose coupling and persistence ignorance. This pattern is seen in eShopOnWeb whenever we need to deal with something from a data store - getting items, adding items, deleting items, and updating items. As eShopOnWeb uses Entity Framework Core, that data store implementation might be a SQL Server-based database because of how it is configured currently. You could switch the underlying data store to something else - such as Postgres or even file-based. Regardless of how you store the data, the way you work with a `Basket`, `CatalogItem`, `Order`, or `CatalogBrand` remains the same.

## Specification

The [Specification design pattern](https://deviq.com/design-patterns/specification-pattern) encapsulates query details in separate classes as part of the domain model. This pattern can be used to solve the following problems:

* Duplicate query logic throughout the application
* Growing Repository implementations that keep adding custom query methods

Additionally, individual specifications may be more easily discovered and better-understood than complex LINQ queries littered throughout the application's layers.

A common problem in applications that leverage the repository pattern, especially a generic repository, is the need to add custom repository implementations any time a custom query is required. This is especially problematic in larger applications with dozens or hundreds of entities, since every entity might need to have corresponding custom repository interfaces and implementations to support it.

Using the Specification design pattern and a generic repository that accepts specifications as a means of describing queries typically eliminates this problem completely. In addition to resulting in less code overall, the pattern helps the data access portions of the application to better follow the [SOLID principles](https://deviq.com/principles/solid) of Single Responsibility and Open-Closed. Repositories no longer are responsible for communicating with a data technology and formulating specific kinds of queries. And the addition of new queries no longer requires modifying existing data access code, but instead can be achieved purely by adding a new specification class.

## Guard Clauses

TBD

## Result

The Result pattern is common in many functional languages, sometimes referred to as Maybe. Some examples include:

* https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/results
* https://wiki.haskell.org/Maybe

You can see this pattern as work in ASP.NET Core MVC itself, which typically uses some kind of `ActionResult` as the return type from action methods. While it is possible to return an object or a specific result type (for example, `JsonResult`), it is far less flexible. What if there is a validation error? What if the resource in question wasn't found? What if some other kind of exception occurs? By returning an `ActionResult`, individual actions can easily be written such that non-success scenarios can be represented with custom types of responses.

Application services can benefit from this same pattern. Rather than having a service return a Basket directly, it might return a Result<Basket> which would include the Basket instance if successful, but could also return a not found result if appropriate. Or a set of validation errors if the request was incorrectly formatted or out of range.

The eShopOnWeb sample uses [an open source Result package](https://www.nuget.org/packages/Ardalis.Result) for this abstraction. This package includes helper methods that will automatically translate from its own abstractions to ASP.NET Core MVC `ActionResult` instances as well, making it easy to use, especially with ASP.NET Core web APIs.

## Entities

TBD

## Aggregates

TBD

## Resources

These resources can help you learn more about the patterns mentioned in this application.

### [DevIQ](https://deviq.com)
- [Aggregate Pattern](https://deviq.com/domain-driven-design/aggregate-pattern)
- [Entity Pattern](https://deviq.com/domain-driven-design/entity)
- [Guard Clauses](https://deviq.com/design-patterns/guard-clause)
- [Mediator Pattern](https://deviq.com/design-patterns/mediator-pattern)
- [Repository Pattern](https://deviq.com/design-patterns/repository-pattern)
- [Specification Pattern](https://deviq.com/design-patterns/specification-pattern)
