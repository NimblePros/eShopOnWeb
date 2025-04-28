---
title: Single Sign-on
parent: Features
---

eShopOnWeb has external login support using GitHub.

## Why OAuth? Why not Open ID Connect (OIDC)?

Username and password pairs are not fun to manage. They can lead to more password fatigue. Single sign-on alleviates that issue. We have to use OAuth in this case as the provider we chose didn't support OIDC at the time we added the code to the system.

## Why GitHub?

This was chosen since our audience is typically developers, and GitHub is a common login platform for developers.

Initially, we had hoped to show a platform with OpenID Connect and with PKCE. However, GitHub had neither of these features at the time the code was added to eShopOnWeb. We really wanted to use an account platform that we knew devs would use, so that's why we stayed with GitHub.
