# Contributing

Thanks for helping. Changes arrive as pull requests into `main` and are merged only after review and green checks.

## Quality gates

CI enforces all of these on every pull request. A change is ready when every check is green.

1. **Locked restore.** `dotnet restore --locked-mode`. If you change a package, regenerate the lock file with
   `dotnet restore --force-evaluate` and commit `tests/SnipeIt.UiTests/packages.lock.json`.
2. **Release build, warnings as errors.** `dotnet build -c Release` with `Nullable`, `AnalysisLevel=latest-recommended`,
   `EnforceCodeStyleInBuild` and NUnit.Analyzers. Suppressions go in `.editorconfig` with a one-line reason.
3. **Formatting.** `dotnet format --verify-no-changes`. Run `dotnet format` before pushing.
4. **Unit tests.** `dotnet test --filter TestCategory=Unit` (no browser, no network).
5. **Guardrails.** No `Thread.Sleep`, `WaitForTimeoutAsync` or `Force = true` in tests; no en or em dash characters
   anywhere (use commas, colons, parentheses or hyphens); no committed build output or test artifacts; the
   CI-level retry script passes its self-test.
6. **E2E on chromium, firefox and webkit** against the public demo, plus the merged Allure report. A failed test
   step is re-run once by CI and reported as `passed-on-retry`; explain any retry in the pull request.
7. **Docker.** The image builds and the E2E test passes inside it.
8. **gitleaks** finds no secrets in the git history.
9. **CodeQL** (C#, `security-and-quality` queries) analyses the change. Fix its alerts, or dismiss them with a reason.

## Writing tests

- Put UI interactions and UI assertions in page objects (`Pages/`) and reusable widgets in `Components/`.
  Tests read as the user workflow.
- Locate by role or label first, then stable ids. Find table columns by `data-field`, never by index.
- Use Playwright web-first assertions (`Expect(locator).ToHaveTextAsync(...)`). No sleeps, no forced clicks,
  no retries inside tests.
- Create your own data with `AssetDataBuilder`; never depend on, edit or delete existing demo records.
- Log steps through `StepAsync` and add new log messages to `Logging/Log.cs` as `[LoggerMessage]` methods.
- Keep business actions in the UI. The REST API may only be used for read-only cross-checks.

## Updating the accessibility baseline

The E2E test fails when a visited page shows a critical axe rule that is not in
`tests/SnipeIt.UiTests/accessibility-baseline.json`. When that happens:

1. Open `axe-<page>.json` in the `diagnostics-<browser>` artifact of the failing run and read the new rule.
2. If it is caused by our own change, fix the change. If it is a change in the third-party demo, add the rule id to
   that page in the baseline.
3. Link the run in the pull request so a reviewer can see the evidence. Removing a rule from the baseline when
   the demo fixes it is always welcome.

## Commits and pull requests

- Small commits with conventional messages (`feat:`, `fix:`, `test:`, `ci:`, `docs:`, `build:`, `style:`).
- Fill in the pull request template. Every claim in the README must be backed by a CI run.
- Never commit secrets. The only credentials in the repository are the demo's published admin account.
