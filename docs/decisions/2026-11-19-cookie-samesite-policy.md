---
title: Cookie SameSite Policy
decisioner: Steve "Ardalis" Smith
status: Active
date: 2026-11-19
---

The cookie policy's minimum SameSite level is `Lax`, not `Strict`. Cookies that need `Strict` set it themselves.

## SameSite policy for each cookie

| Cookie | SameSite | Where it is set |
|---|---|---|
| Sign-in (`EshopIdentifier`) | `Strict` | `ConfigureCookieSettings` sets it on the application cookie |
| Antiforgery (`.AspNetCore.Antiforgery.*`) | `Strict` | ASP.NET Core default |
| Basket (`eShop`) | `Lax` | `BasketCookie.CreateOptions` |
| TempData (one-time page messages) | `Lax` | ASP.NET Core default |
| Any other cookie | its own setting, at least `Lax` | `CookiePolicyOptions.MinimumSameSitePolicy` in `ConfigureCookieSettings` |

## What the setting controls

When a request comes from another website, the browser decides whether to send the cookie.

| | Same-site requests | Clicking a link from another site | Cross-site form posts, images, scripts |
|---|---|---|---|
| `Strict` | sent | **not sent** | not sent |
| `Lax` | sent | sent | not sent |

## Why the basket cookie is `Lax`

With `Strict`, a shopper who arrives from a search engine, an email or a social post looks like a new visitor on that first page:

- The basket count in the header shows 0 until the next click.
- Landing straight on `/basket` is worse. The Basket page sees no cookie and writes a new one with a new id, which abandons the shopper's existing anonymous basket.

`Strict` adds almost no protection here. SameSite mainly defends against cross-site request forgery (CSRF). `Lax` already withholds the cookie from cross-site form posts and embedded requests, and every POST also checks an antiforgery token. The only extra request `Strict` blocks is a GET from a link on another site. The basket GETs only read the basket (apart from setting this cookie), and the page goes to the shopper, not to the other site, so the request leaks nothing to an attacker.

## Why the sign-in cookie stays `Strict`

The sign-in cookie protects the account itself, so it keeps the stricter setting. The cost is small: a signed-in shopper who follows a link from another site may appear signed out on that first page only.

## Why the minimum is `Lax`, not `Strict`

`CookiePolicyOptions.MinimumSameSitePolicy` raises every cookie to at least its level. A `Strict` minimum silently overrode the basket cookie's own `Lax` setting. A `Lax` minimum still stops any cookie from going out with `SameSite=None`, while letting each cookie choose `Strict` where it matters.

Do not raise the minimum back to `Strict`. If a new cookie needs `Strict`, set it on that cookie. Functional tests check the basket cookie (`Lax`), the sign-in cookie (`Strict`) and the antiforgery cookie (`Strict`).
