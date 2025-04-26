---
parent: "Architecture Decision Records"
title: Just the Docs Theme
requested_by: Sarah "Sadukie" Dutkiewicz
status: "in progress"
---

Initially, I started with a GitHub Pages template.

However, looking into the Jekyll themes out there for documentation that made it easy to organize the docs and search them, I found[ Just-the-Docs] to be an easy-to-implement solution.

## Implementation Details

For simplicity's sake, we have the Just-the-Docs theme set up as a [remote theme](https://github.com/benbalter/jekyll-remote-theme).

If this needs to be switched to [a Gemfile implementation](https://github.com/just-the-docs/just-the-docs?tab=readme-ov-file#use-as-a-ruby-gem), this can be implemented using the `just-the-docs` gem.