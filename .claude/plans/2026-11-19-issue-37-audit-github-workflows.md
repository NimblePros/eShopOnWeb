# Issue #37: Audit GitHub workflows and update action versions

Issue: https://github.com/NimblePros/eShopOnWeb/issues/37

The issue started as "move to Node 20 actions" (checkout v2, toc-generator v4). The request
widened it: decide which workflows to keep, and bring every action in the kept ones up to the
current release.

## Evaluation

| Workflow | Decision | Reason |
|---|---|---|
| `dotnetcore.yml` (Build and Test) | Keep | The main CI: build, unit/integration/e2e tests, coverage. |
| `comment-on-pr.yml` (coverage PR comment) | Keep | Posts the coverage summary to PRs. Runs on `workflow_run` so fork PRs get comments without a write token in the PR build. |
| `toc.yml` (Generate Table of Contents) | Remove | `technote-space/toc-generator` last released v4.3.1 in Dec 2022 and still declares `runs.using: node16`; the repo has had no pushes since May 2023. This is the action that kept #37 open. The README TOC is a doctoc block of about 15 lines that rarely changes, and the bot commit to `main` adds nothing a reviewer can't do by hand (or with `npx doctoc README.md`). |

## Phase 1: Update kept workflows

- [x] `dotnetcore.yml`: `actions/checkout` v4 -> v7, `actions/setup-dotnet` v1 -> v6, `actions/upload-artifact` v4 -> v7.
- [x] `dotnetcore.yml`: add `permissions: contents: read` (least privilege for the PR build).
- [x] `comment-on-pr.yml`: replace `dawidd6/action-download-artifact@v6` with first-party `actions/download-artifact@v8`, using `run-id: ${{ github.event.workflow_run.id }}` so the artifacts always come from the run that triggered the comment.
- [x] `comment-on-pr.yml`: `marocchino/sticky-pull-request-comment` v2 -> v3.
- [x] `comment-on-pr.yml`: add explicit `permissions` (`actions: read` to download another run's artifacts, `pull-requests: write` to comment).

## Phase 2: Remove unneeded workflow

- [x] Delete `toc.yml`.

## Phase 3: Keep versions current

- [x] Add a `github-actions` ecosystem entry to `.github/dependabot.yml` so future action releases arrive as PRs (weekly).

## Automated test requirements

There is no unit test harness for workflow YAML. Coverage comes from:

- [ ] The PR's own `eShopOnWeb Build and Test` run on the updated `dotnetcore.yml`.
- [ ] `comment-on-pr.yml` only runs from the default branch, so it can't be exercised before merge. Verify after merge that the next PR gets a coverage comment.
- [x] Parse every changed YAML file locally.

## Phase 4: Independent review

- [x] Fresh-context agent reviews the diff (correctness of action inputs across the major version bumps, permissions, security of `workflow_run`).
- [x] Address significant findings:
  - The PR number comes from an artifact the fork PR controls, so a PR could point the coverage comment at any PR (this was true before too). The workflow now checks the number is numeric and that the PR's head SHA matches `workflow_run.head_sha`. On a mismatch it skips the comment instead of failing.
  - Cancelled builds (from `cancel-in-progress`) have no artifacts. The job now skips `conclusion == 'cancelled'`.
  - Removed the redundant "Confirm the PR Number (Debugging)" step.

## Phase 5: Verification

- [x] Run `dotnet ./BuildTestFormat.cs` (no code changes, but confirm the tree is clean).

## Assumptions

- "Current" means the latest major release tag as of 2026-11-19 (checkout v7.0.1, setup-dotnet v6.1.0, upload-artifact v7.0.2, download-artifact v8.0.2, sticky-pull-request-comment v3.0.5). Major tags (`@v7`) are used, matching the repo's existing style, rather than SHA pins.
- GitHub-hosted `ubuntu-latest` runners meet the Node 24 runner minimum these releases need.
