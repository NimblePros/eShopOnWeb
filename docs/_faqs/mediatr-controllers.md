---
title: "Why do some controllers/Pages use MediatR and some do not?"
---

With this being a demo app, we were trying to show a few different ways to approach solutions. For example, the [OrderController](https://github.com/NimblePros/eShopOnWeb/blob/master/src/Web/Controllers/OrderController.cs) was updated to use MediatR while the [ManageController](https://github.com/NimblePros/eShopOnWeb/blob/master/src/Web/Controllers/ManageController.cs) was not. This way you can see the advantages / disadvantages of each approach while staying in the same repo.

I would definitely recommend for a "real" project that you maintain one approach or the other.