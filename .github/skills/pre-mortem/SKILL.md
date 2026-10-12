---
name: pre-mortem
description: 'Identifies potential production risks before they occur. Use when reviewing a pull request or proposed code change for a pre-mortem analysis.'
---

# Pre-mortem

1. Obtain the pull request or proposed change. If none was provided, ask the user for it and stop until it is supplied.
2. Read the change in full, including the surrounding code it touches, so every claim below is grounded in code you have actually seen.
3. Assume the change causes a production incident 30 days after release.
4. Enumerate five plausible root causes for that incident.
5. For each root cause, describe the specific code path or assumption in the change that enables it. Name the file, function, or line you identified.
6. If you cannot identify a concrete code path or assumption for a root cause, say so explicitly. Do not invent code paths, files, or behavior you cannot see.
7. Present the five root causes as a numbered list, each with its root cause and the enabling code path or assumption.