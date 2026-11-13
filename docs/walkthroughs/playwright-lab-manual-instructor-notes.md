---
title: Playwright Lab Manual and Instructor Notes
parent: Walkthroughs
---

# Playwright Lab Manual and Instructor Notes

This guide is for running a live lab based on the Playwright work completed in this repository.

It is designed to answer the questions that drove the plan:

1. What is Playwright codegen and how should we use it?
2. How do we move from generated draft code to readable tests?
3. How do we debug failures with trace, screenshot, and video artifacts?
4. What should be in browser tests vs lower-level tests?
5. How can AI/agents help without lowering quality?
6. How can we automate issue reproduction using Playwright and agents?

## Audience and Outcomes

By the end of this lab, learners should be able to:

1. Generate a Playwright draft test and explain why generated code is only a starting point.
2. Refactor toward stable, semantic locators and intent-based assertions.
3. Produce and inspect artifacts from an intentional failure.
4. Classify test scenarios into the right test layer.
5. Use an AI-assisted workflow with explicit human review gates.

## Where the Supporting Material Lives

- End-to-end test docs: `tests/EndToEndTests/README.md`
- Test strategy page: `docs/explore/tests.md`
- Level 2 skill: `.claude/skills/playwright-dotnet-refactor/SKILL.md`
- Refactor prompt template: `.claude/skills/playwright-dotnet-refactor/assets/refactor-prompt-template.md`
- Issue repro skill: `.claude/skills/playwright-issue-repro-report/SKILL.md`
- Issue repro workflow: `.claude/skills/playwright-issue-repro-report/references/WORKFLOW.md`
- Issue prompt/comment templates: `.claude/skills/playwright-issue-repro-report/templates/`

## Lab Prerequisites

Run these once before the session:

```powershell
dotnet build tests/EndToEndTests/EndToEndTests.csproj
pwsh tests/EndToEndTests/bin/Debug/net11.0/playwright.ps1 install
```

Sanity check (should pass with one intentionally skipped demo test):

```powershell
dotnet test tests/EndToEndTests/EndToEndTests.csproj
```

Start the app for interactive codegen demos (separate terminal):

```powershell
dotnet run --project src/Web/Web.csproj -- --UseOnlyInMemoryDatabase=true
```

Keep this terminal running while you execute codegen steps.

## Suggested 75-Minute Flow

1. 0-10 min: Framing and key questions.
2. 10-25 min: Demo 1 (Codegen to cleaned test thinking).
3. 25-45 min: Demo 2 (Intentional failure and artifacts).
4. 45-60 min: Demo 3 (Test strategy and layer mapping).
5. 60-72 min: Demo 4 (AI-assisted workflow).
6. 72-75 min: Wrap-up and Q&A.

## Instructor Script by Question

### Question 1: What is Playwright codegen?

Instructor answer:

- Codegen records interactions and outputs runnable Playwright code.
- It is a drafting accelerator, not final test quality.
- We always refactor generated output to improve selector stability and readability.

Live steps:

1. Ensure the app is running in a separate terminal:

```powershell
dotnet run --project src/Web/Web.csproj -- --UseOnlyInMemoryDatabase=true
```

1. Run codegen:

```powershell
pwsh tests/EndToEndTests/bin/Debug/net11.0/playwright.ps1 codegen https://localhost:5001
```

In the browser, perform:

1. Open home page.
2. Click first Add to Basket.
3. Click Basket.
4. Confirm basket line item is visible.

Teaching notes:

- Call out any brittle selector the recorder chooses.
- Ask participants: "Would this selector survive a UI refactor?"

### Question 2: How do we clean generated tests?

Instructor answer:

- Preserve user intent, then simplify.
- Prefer semantic locators: role, label/text, test-id.
- Keep assertions user-visible and behavior-oriented.
- Extract helpers only when they improve readability.

Show examples in repo:

- `tests/EndToEndTests/Playwright/CatalogTests.cs`
- `tests/EndToEndTests/Playwright/Pages/CatalogPage.cs`

Optional AI assist:

- Use `.claude/skills/playwright-dotnet-refactor/assets/refactor-prompt-template.md`

### Question 3: How do we debug failures with artifacts?

Instructor answer:

- We use intentional failure to practice diagnosis.
- Artifacts show what happened during the failed run.
- Trace is usually the fastest path to root cause.

Live steps:

1. Temporarily remove `Skip` from:
   - `tests/EndToEndTests/Playwright/BasketTests.cs`
2. Run only the failing scenario:

```powershell
dotnet test tests/EndToEndTests/EndToEndTests.csproj --filter FullyQualifiedName~BasketTests.Cart_AddItem_ShowsExpectedTotal
```

1. Inspect artifact folder:

- `TestResults/PlaywrightArtifacts/<test-name>-<timestamp>/failure.png`
- `TestResults/PlaywrightArtifacts/<test-name>-<timestamp>/trace.zip`
- `TestResults/PlaywrightArtifacts/<test-name>-<timestamp>/video/`

1. Open trace locally:

```powershell
pwsh tests/EndToEndTests/bin/Debug/net11.0/playwright.ps1 show-trace TestResults/PlaywrightArtifacts/<folder>/trace.zip
```

1. Or use hosted viewer at <https://trace.playwright.dev>.
2. Re-add `Skip` after demo to keep default suite green.

Talking point:

- These are execution artifacts, not visual regression baselines.

### Question 4: What belongs in Playwright vs other tests?

Instructor answer:

- Playwright: critical user journeys and wiring confidence.
- Unit/integration/API: business rule permutations and contract detail.

Use the matrix from `docs/explore/tests.md` and ask participants to classify:

1. Home page catalog render.
2. Add to basket happy path.
3. Discount edge combinations.
4. API auth and payload contract.

### Question 5: How should AI/agents be used safely?

Instructor answer:

- Agents accelerate exploration and first drafts.
- Human review is mandatory before run/merge.
- Guardrails: preserve intent, semantic locators, user-visible assertions.

Run this workflow:

1. Ask agent to explore catalog flows.
2. Ask for 3-5 scenario proposals and layer classification.
3. Ask for one draft Playwright C# test.
4. Human review/edit.
5. Run targeted test.
6. If failed, inspect artifacts.
7. Ask agent to diagnose from outputs and trace observations.

## Issue-to-Repro Demo (Skill-Driven, Low Copy/Paste)

Use this as an optional advanced segment when teaching issue triage workflows.

### Reusable skill assets

- Skill: `.claude/skills/playwright-issue-repro-report/SKILL.md`
- Workflow reference: `.claude/skills/playwright-issue-repro-report/references/WORKFLOW.md`
- Repro prompt template: `.claude/skills/playwright-issue-repro-report/templates/issue-repro-prompt.md`
- Issue comment template: `.claude/skills/playwright-issue-repro-report/templates/issue-comment-template.md`

### Instructor flow

1. Start from a real GitHub issue URL/number.
2. Open `templates/issue-repro-prompt.md` and fill only issue-specific inputs.
3. Ask the agent to execute the filled prompt and produce:
   - Repro status (reproduced/not reproduced/inconclusive)
   - Deterministic repro steps
   - Test snippet or file update proposal
   - Artifact locations
4. Run the targeted repro test command from agent output.
5. Collect evidence from `TestResults/PlaywrightArtifacts/...`.
6. Open `templates/issue-comment-template.md` and fill with actual repro results.
7. Post the comment with artifacts.

### Posting artifacts to the GitHub issue

Option A: GitHub web UI (recommended for live demo)

1. Open the issue comment box.
2. Drag-and-drop `failure.png` and a zipped video artifact.
3. Paste the populated comment template text.
4. Include trace path or hosted link instructions.

Option B: GitHub CLI

1. Save populated comment body to a local markdown file.
2. Ensure artifact links are reachable URLs.
3. Post:

```powershell
gh issue comment <issue-number> --body-file <comment-file>.md
```

### Evidence quality checklist

1. Repro status clearly stated.
2. Expected vs observed behavior both documented.
3. Screenshot included for visual proof.
4. Trace included for timeline/root-cause analysis.
5. Next action and owner suggested.

## Demo Command Block (Copy/Paste)

```powershell
# Terminal 1 (keep running for codegen demos)
dotnet run --project src/Web/Web.csproj -- --UseOnlyInMemoryDatabase=true

# Terminal 2
dotnet build tests/EndToEndTests/EndToEndTests.csproj
pwsh tests/EndToEndTests/bin/Debug/net11.0/playwright.ps1 install
dotnet test tests/EndToEndTests/EndToEndTests.csproj
pwsh tests/EndToEndTests/bin/Debug/net11.0/playwright.ps1 codegen https://localhost:5001
dotnet test tests/EndToEndTests/EndToEndTests.csproj --filter FullyQualifiedName~CatalogTests
dotnet test tests/EndToEndTests/EndToEndTests.csproj --filter FullyQualifiedName~BasketTests.Cart_AddItem_ShowsExpectedTotal

# Optional: post issue comment from CLI after preparing comment markdown
# gh issue comment <issue-number> --body-file <comment-file>.md
```

After finishing interactive demos, stop the running app with Ctrl+C in Terminal 1.

## Facilitation Notes

1. Keep one terminal for commands and one editor window for code review.
2. Narrate intent before each command (why this step exists).
3. Pause after each demo to collect one "what changed confidence" reflection.
4. Timebox deep debugging rabbit holes; prioritize workflow learning.

## Common Pitfalls and Recovery

1. Browser not installed:
   - Re-run `pwsh tests/EndToEndTests/bin/Debug/net11.0/playwright.ps1 install`.
2. HTTPS/localhost issues:
   - Ensure app starts from test fixture and wait for startup.
3. Flaky locator from generated code:
   - Replace with role/text/test-id locator.
4. Suite left red after artifact demo:
   - Restore `Skip` on intentional failure test.

## Debrief Questions

1. Which generated selector looked most brittle, and how did you improve it?
2. What signal in trace/screenshot gave the fastest clue?
3. Which scenario should move out of browser tests first and why?
4. Where did AI save time, and where was human judgment essential?

## Completion Criteria for This Lab

1. Participants can explain codegen as draft-first workflow.
2. Participants can run and inspect failure artifacts.
3. Participants can classify scenarios by test layer.
4. Participants can apply AI-assisted workflow with guardrails.
