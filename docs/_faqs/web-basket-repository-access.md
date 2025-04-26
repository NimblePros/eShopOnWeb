---
title: "Why does the Web project have access to the BasketRepository?"
---

Ideally a Repository shouldn't have business logic, per se. It should only be responsible for getting or storing an entity or aggregate from/to persistence - whatever that might be. If the Web project wants to work with the domain model, it needs a way to get those items. If it's not just creating them from scratch, there must be some abstraction it uses to access them from persistence. That abstraction is the Repository pattern. Many applications prefer to decouple the UI/Web project from the domain model via a collection of application services, and that's fine, too. But then these application services still must have a way to access the model from persistence.
