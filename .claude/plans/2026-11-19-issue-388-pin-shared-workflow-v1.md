# Issue #388: Pin the shared comment-on-pr workflow to a release tag

Issue: https://github.com/NimblePros/eShopOnWeb/issues/388
Blocked by NimblePros/NimblePros.GitHub.Workflows#6 (closed by NimblePros/NimblePros.GitHub.Workflows#7). Relates to #232.

## Status of the blocker

#6 is closed and PR #7 added protection on `main`, protection on `v*` tags, and a release workflow that moves the `vMAJOR` tag. But as of 2026-11-19, **no release or tag exists yet**: `git ls-remote --tags` returns nothing. The README's next step for an admin is `gh release create v1.0.0 --target main --generate-notes`. Until that runs, `@v1` doesn't resolve, and merging this change would break the coverage comment workflow.

## Phase 1: Pin the reference

- [x] Change `.github/workflows/comment-on-pr.yml` from `@main` to `@v1`. This follows the shared README's recommendation and matches the major-tag style of every other `uses:` in this repo.
- [x] Dependabot's `github-actions` ecosystem is already enabled (#384). According to the shared README, it offers a PR when `v2` ships.

## Phase 2: Wait for the tag

- [ ] The `v1.0.0` release has been published on NimblePros.GitHub.Workflows and the `v1` tag exists.
- [ ] Mark the PR ready for review.

## Automated test requirements

- [x] Parse the YAML locally.
- [ ] `workflow_run` workflows only run from `main`. After merge, confirm the next PR gets a coverage comment.

## Phase 3: Independent review

- [x] Skipped a separate review agent: the change is one ref on one line, and the review for #232 already covered the caller's inputs and permissions. The open question (does `v1` exist) is a repository state, not a code question.

## Phase 4: Verification

- [x] `dotnet ./BuildTestFormat.cs`

## Assumptions

- Use the moving major tag `@v1`, not `@v1.0.0` or a SHA. Releases in the shared repo are immutable, `v*` tags are protected by a ruleset, and only the release workflow can move `v1`. This is the trade-off the shared README recommends.
