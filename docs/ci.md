# CI: test-file deletion gate

The workflow [.github/workflows/test-deletion-guard.yml](../.github/workflows/test-deletion-guard.yml) runs on every pull request into `main` and calls [scripts/ci/check-test-deletions.sh](../scripts/ci/check-test-deletions.sh).

## Admin: make the check required

In the repository, go to **Settings > Branches**, edit (or add) the branch protection rule for `main`, enable **Require status checks to pass before merging**, and add the status check named `check`. A ruleset (**Settings > Rules > Rulesets**, target `main`, rule **Require status checks to pass**) works the same way.

The status-check name comes from the job, not the workflow file name. The job has no `name:` key, so its name is its ID, `check` (the workflow's display name is `Test Deletion Guard`, so the PR checks list shows `Test Deletion Guard / check`). This is derived from the workflow file and has not been confirmed in GitHub. To confirm it, open any pull request into `main` after the workflow has run once and read the name in the Checks list. The same name is what the required-checks search box offers, and the box only lists checks that have run recently in the repository.

## For engineers

A pull request into `main` fails this check if it deletes a test file, or renames one out of `tests/`, unless at least one commit message in the PR contains `DELETE_TESTS:` followed by a non-empty reason. A test file here means any `.cs` file under `tests/`. Because the check runs on the server against the PR's commits, it applies equally to changes written by AI coding assistants, which can remove an inconvenient failing test while "fixing" a build, and which a local hook or an assistant instruction file cannot guarantee to stop. The gate does not catch tests that are weakened in place (assertions removed, methods emptied, `[Fact(Skip = ...)]` added) or deleted without removing the file, and it does not look at non-`.cs` files or test code outside `tests/`. One limitation of the script itself: a single `DELETE_TESTS: <reason>` commit anywhere in the PR permits every test deletion in that PR, and the reason is only checked for being non-empty, so reviewers still need to judge whether it matches what was deleted.
