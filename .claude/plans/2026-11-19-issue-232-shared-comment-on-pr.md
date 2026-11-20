# Issue #232: Migrate to NimblePros.GitHub.Workflows for Comment on PR

Issue: https://github.com/NimblePros/eShopOnWeb/issues/232
Relates to #37 (PR #384, which hardened the local workflow) and NimblePros/NimblePros.GitHub.Workflows#2 (which ported those fixes to the shared workflow).

## Phase 1: Switch to the shared workflow

- [x] Confirm the shared `comment-on-pr.yml` on `main` includes every #384 fix: `actions/download-artifact@v8` pinned to `workflow_run.id`, `sticky-pull-request-comment@v3`, the PR number checked against the head SHA, cancelled builds skipped.
- [x] Replace the local job in `.github/workflows/comment-on-pr.yml` with a call to `NimblePros/NimblePros.GitHub.Workflows/.github/workflows/comment-on-pr.yml@main`, keeping the `workflow_run` trigger and the `actions: read` / `pull-requests: write` permissions.
- [x] Confirm the artifact names in `dotnetcore.yml` match the shared workflow's inputs and defaults (`pr-number` / `pr-number.txt`, `code-coverage-results` / `code-coverage-results.md`). They match, so `dotnetcore.yml` is unchanged.

## Automated test requirements

There is no unit test harness for workflow YAML.

- [x] Parse the YAML locally.
- [ ] `workflow_run` workflows only run from the default branch, so this can't be exercised before merge. After merge, confirm the next PR gets a coverage comment.

## Phase 2: Independent review

- [x] Fresh-context agent reviews the diff against the shared workflow. It found no correctness problems: inputs, artifact names, permissions and the `github` context all line up, and no behaviour is lost.
- [x] Address significant findings:
  - `@main` in the shared repo is unprotected, so an unreviewed push there would run here with `pull-requests: write`. Kept `@main` (see Assumptions) and filed NimblePros/NimblePros.GitHub.Workflows#6 to protect `main` and publish a `v1` tag. Once that tag exists, switch this reference to it and Dependabot will track it.

## Phase 3: Verification

- [x] `dotnet ./BuildTestFormat.cs`: build OK, all tests pass (1 e2e skipped, as before), format clean.

## Assumptions

- The workflow is referenced at `@main`, as the shared repo's README shows. The shared repo has no release tags, and it is NimblePros-owned, so tracking `main` is accepted in exchange for picking up fixes automatically.
