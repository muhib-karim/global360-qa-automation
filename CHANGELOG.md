# Changelog

All notable changes to this project are documented here.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

[Unreleased]: https://github.com/muhib-karim/global360-qa-automation/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/muhib-karim/global360-qa-automation/releases/tag/v1.0.0
