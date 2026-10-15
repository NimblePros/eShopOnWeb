# eShopOnWeb: rules for every session

Hot file. Keep it under 80 lines. Stack facts, commands, and versions live in
[docs/context/stack.md](../docs/context/stack.md). Read that file before touching
tests, packages, or project references. Do not copy its contents here.

## Where context belongs

| Kind of context | File |
| --- | --- |
| Behavioral rules that apply to every task | this file |
| Language, framework, build tool, test runner, commands, versions, architecture | `docs/context/stack.md` |
| Style that `dotnet format` can enforce | `.editorconfig` (do not restate it) |
| Open questions, gaps, unverified assumptions | `docs/context/cold/gap-log.md` |

## Rules

1. Read the existing code and `docs/context/stack.md` first. Match what is there.
2. Use the test framework of the project you are editing. Check its `.csproj`. Do not
   introduce a new framework or mix them within one project.
3. Respect the layering in `docs/context/stack.md`. `ApplicationCore` must not
   reference `Infrastructure`, `Web`, or `PublicApi`.
4. Add NuGet versions only in `Directory.Packages.props`. Never put `Version=` in a
   `.csproj`.
5. Make the smallest change that solves the request. No drive-by refactors,
   renames, or reformatting of untouched code.
6. New behavior gets a test in the matching test project. A bug fix gets a
   regression test.
7. Never commit secrets or connection strings. Use user-secrets or Key Vault.
8. Do not edit generated or migration files (`Migrations/`, `*.Designer.cs`,
   `*ModelSnapshot.cs`) by hand. Add a new EF migration instead.
9. Do not overwrite or delete existing AI config (`.claude/`, `.github/`,
   `docs/context/`) without asking. Extend it.
10. When a fact is missing or unclear, record it in `docs/context/cold/gap-log.md`
    and ask. Do not guess.

## Before declaring done

- Build and run the relevant tests (commands are in `docs/context/stack.md`).
- Run `dotnet format eShopOnWeb.slnx --verify-no-changes`.
- Report what you ran and what you did not run.

## Communication

- Be brief. Lead with the result. Link files by workspace-relative path.
- State assumptions explicitly when you make them.
