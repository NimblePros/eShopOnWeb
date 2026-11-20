# Issue #398 — LoggerEmailSender logs recipient addresses and full email bodies

https://github.com/NimblePros/eShopOnWeb/issues/398

## Findings

`LoggerEmailSender` is the only `IEmailSender`, and `ConfigureCoreServices` registers it in every
environment. It logged the recipient, subject and full body at Information level. Confirmation and
password-reset bodies contain tokenized links, so the logs held PII and account tokens (ND3127 pattern).

## Decision

Redact instead of restricting registration. The deployed sample has no real email sender, so
registering `LoggerEmailSender` only in local development (or failing fast elsewhere) would break
registration / email confirmation in the deployed demo.

- Local development (`IsLocalDevelopment()`: Development, Docker, Testing): log the full email as
  before, so developers can follow the confirmation and reset links.
- Everywhere else: log only the subject and a masked recipient (`j***@example.com`), never the body.

## Phase 1 — Code fix
- [x] Inject `IHostEnvironment` into `LoggerEmailSender`; branch on `IsLocalDevelopment()`.
- [x] Add `LoggerEmailSender.MaskEmail` (first character of the local part + domain; `***` for
      anything that isn't a well-formed address).

## Phase 2 — Automated tests
- [x] `tests/UnitTests/Infrastructure/Services/LoggerEmailSenderTests.cs`: Production/Staging never
      log the address, body or token; Development/Docker/Testing log the full email; masking edge cases.
- [x] Mutation check: forcing the full-log branch everywhere makes both non-local tests fail.

## Phase 3 — Verification
- [x] `dotnet ./BuildTestFormat.cs`: build OK; AppHost 6/6, EndToEnd 2/3 (1 skipped), Functional
      83/83, PublicApiIntegration 158/158, Unit 185/185 (12 new); `dotnet format --verify-no-changes` clean.
- [ ] IntegrationTests: 14/20 locally. The 6 failures are the Testcontainers SQL Server migration
      tests, which need Docker (not installed on the dev machine). This change doesn't touch the
      database; CI runs them.
