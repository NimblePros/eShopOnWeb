# Issue Repro Workflow Reference

## 1. Gather issue context

Capture:

1. Issue title and URL.
2. Expected behavior.
3. Observed behavior.
4. Preconditions (auth, data, browser, viewport).

## 2. Prepare environment

```powershell
dotnet build tests/EndToEndTests/EndToEndTests.csproj
pwsh tests/EndToEndTests/bin/Debug/net11.0/playwright.ps1 install
```

If using interactive codegen, start the app in another terminal:

```powershell
dotnet run --project src/Web/Web.csproj
```

## 3. Build repro test

- Start from generated code or existing test.
- Normalize locators (`GetByRole`, `GetByText`, `GetByTestId`).
- Assert only user-observable outcomes.

## 4. Execute repro run

Run targeted test filter when possible:

```powershell
dotnet test tests/EndToEndTests/EndToEndTests.csproj --filter FullyQualifiedName~<TestName>
```

## 5. Collect evidence

Use latest folder under:

- `TestResults/PlaywrightArtifacts/<test-name>-<timestamp>/failure.png`
- `TestResults/PlaywrightArtifacts/<test-name>-<timestamp>/trace.zip`
- `TestResults/PlaywrightArtifacts/<test-name>-<timestamp>/video/`

## 6. Publish issue comment

Use template from `templates/issue-comment-template.md`.

Two posting options:

1. GitHub web UI:

- Drag and drop `failure.png` and/or a zipped video into the comment box.
- Paste trace viewing instructions and attach `trace.zip` if allowed.

1. GitHub CLI:

- Upload artifacts to a reachable location first, then include URLs in comment body.
- `gh issue comment <number> --body-file <comment.md>`

## 7. Close the loop

- State repro status: reproduced/not reproduced/inconclusive.
- Include next action (owner, follow-up test, missing data).
