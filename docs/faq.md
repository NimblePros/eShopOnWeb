---
title: Frequently Asked Questions
nav_order: 2
---

These are the frequently asked questions for eShopOnWeb.

<details markdown="block">
  <summary>
    <h2>When should I use MVC and when should I use Razor Pages? This repo does both!</h2>
  </summary>
  This app could be done completely in one way or the other, but as this is a demo app we're trying to show how it's done in each. This way you can see the advantages / disadvantages of each. A few years ago in this project we tried to maintain two separate projects - 1 for just MVC and 1 for just Razor pages. It was difficult to maintain both, so we combined the projects.
</details>
<hr />

<details markdown="block">
  <summary>
    <h2>Why do some controllers/Pages use MediatR and some do not?</h2>
  </summary>

With this being a demo app, we were trying to show a few different ways to approach solutions. For example, the [OrderController](https://github.com/NimblePros/eShopOnWeb/blob/master/src/Web/Controllers/OrderController.cs) was updated to use MediatR while the [ManageController](https://github.com/NimblePros/eShopOnWeb/blob/master/src/Web/Controllers/ManageController.cs) was not. This way you can see the advantages / disadvantages of each approach while staying in the same repo.

I would definitely recommend for a "real" project that you maintain one approach or the other.
</details>
<hr />

<details markdown="block">
  <summary>
    <h2>Why does the Web project have access to the BasketRepository?</h2>
  </summary>
Ideally a Repository shouldn't have business logic, per se. It should only be responsible for getting or storing an entity or aggregate from/to persistence - whatever that might be. If the Web project wants to work with the domain model, it needs a way to get those items. If it's not just creating them from scratch, there must be some abstraction it uses to access them from persistence. That abstraction is the Repository pattern. Many applications prefer to decouple the UI/Web project from the domain model via a collection of application services, and that's fine, too. But then these application services still must have a way to access the model from persistence.
</details>