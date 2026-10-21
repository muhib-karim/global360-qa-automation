# Changelog

All notable changes to this project are documented here.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.1.0] - 2026-10-21

### Fixed

- The History date assertion that turned the nightly E2E red on 2026-10-21 in all three browsers. The regex
  `\b\d{4}\b.*\b\d{1,2}:\d{2}\b` rejected Snipe-IT's `g:iA` time format (`Wed Oct 21, 2026 4:31AM`) because
  `AM` touches the minutes. `SnipeItDate` now parses the cell with Snipe-IT's own nine date and three time
  display formats and the invariant culture; the value must be a time of this run in some real server timezone,
  and the create and checkout rows must be logged together.
- Three asset-form flakes: a click lost while the deployable-status message shifted the layout (Firefox), a
  wait on a success flash the demo does not always render after the redirect (WebKit), and a blind click that
  closed the "Optional Information" section the page had just reopened from its cookie (Chromium).

### Added

- Four more E2E tests in the same matrix: a checked-out asset is checked in (status, assignment and History row
  verified, API cross-checked), an asset is edited and then deleted (details, list and API verified), the create
  form refuses an asset without its required fields, and a wrong password is rejected on the login page.
- `.github/scripts/install-browsers.sh`: browser install with four attempts and exponential backoff; the last
  two pin each official Playwright download host on its own (an independent mirror can be configured), used by the E2E jobs, the quarantine job and the Docker image, and
  self-tested offline in the build job.
- `~/.cache/ms-playwright` cached per browser, keyed on the Playwright version in `packages.lock.json`.
- `failure-traces-<browser>` artifact with the trace of every failed test, including a first attempt that the
  CI-level retry then passed.
- `[Quarantine(reason, until)]`: a time-boxed quarantine that the gating run excludes, a non-blocking
  `quarantine` job runs, and a unit test expires. Documented in CONTRIBUTING.md; nothing is quarantined.
- The Pages site shows the latest run's pass counts per browser and links each browser's HTML report;
  every run builds the site as the `pages-site` artifact and screenshots it (`site-screenshots`).
- Interactive architecture diagram (`docs/architecture.html`), README architecture and report showcase sections,
  HANDOFF.md and SKILLS_USED.md.

### Changed

- The workflows also run on pushes to `upgrade/**`, so a release branch is verified before its pull request.

## [1.0.0] - 2026-10-16

### Added

- End-to-end test of the Snipe-IT asset lifecycle on the public demo: sign in, create a `Macbook Pro 13"` asset
  that is Ready to Deploy and checked out to a random user, find it in the assets list, open it from the list,
  validate its details and validate its History tab by column.
- Layered structure: page objects, components (`Select2`, `BootstrapTable`), a base fixture, configuration and an
  asset data builder.
- Configuration through `appsettings.json` and environment variables; the original `SNIPE_*` variables keep working.
- Structured logging with Microsoft.Extensions.Logging routed to the NUnit test output.
- Diagnostics: Playwright trace, video, evidence screenshots, failure screenshot and HTML, `run.json`, attached to
  TRX and Allure.
- Allure report merged across browsers, VSTest TRX and HTML logs, GitHub job summaries.
- Cross-browser CI matrix (Chromium, Firefox, WebKit), demo preflight and one reported CI-level retry.
- axe-core accessibility scans of the four visited pages with a baseline gate on new critical rules.
- Read-only REST cross-check of the created asset using the signed-in session.
- Unit tests for settings, the data builder and the accessibility baseline comparison.
- Dockerfile with a CI job that builds it and runs the test inside it; dev container for Codespaces.
- gitleaks and CodeQL workflows, Dependabot for NuGet, GitHub Actions and Docker.
- GitHub Pages homepage that publishes the latest Allure report from `main`.
- README, CONTRIBUTING, SECURITY, issue and pull request templates, CODEOWNERS.

### Changed

- Analyzers at `latest-recommended` with code style enforced in the build and warnings as errors.
- Test packages updated to the newest stable releases of Microsoft.Playwright.NUnit (1.63.0), NUnit 4.x (4.6.1),
  NUnit3TestAdapter, Microsoft.NET.Test.Sdk and NUnit.Analyzers.
- GitHub Actions moved to current releases pinned by commit SHA; runners pinned to `ubuntu-24.04`.

[Unreleased]: https://github.com/muhib-karim/global360-qa-automation/compare/v1.1.0...HEAD
[1.1.0]: https://github.com/muhib-karim/global360-qa-automation/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/muhib-karim/global360-qa-automation/releases/tag/v1.0.0
