# Verification

Everything below comes from GitHub Actions logs and artifacts. Nothing here was run on a developer machine.
The demo is shared and resets, so the asset links in this file stop working after a reset; the runs and their
artifacts (kept for 14 days, the Allure report for 30) are the evidence.

## Verified commit

Commit [`fdb894d`](https://github.com/muhib-karim/global360-qa-automation/commit/fdb894de2ce6e3e4969e8553db4cc667fc397507)
on branch `hardening` (pull request [#6](https://github.com/muhib-karim/global360-qa-automation/pull/6)),
16 October 2026 (UTC). Later commits on the branch only add these docs and screenshots, a Docker console
verbosity change, and a self-test of the CI-level retry script (with a `PREFLIGHT` override used only by that
self-test); their own runs are on the pull request.

| Workflow | Run | Result |
| --- | --- | --- |
| E2E | [38841064770](https://github.com/muhib-karim/global360-qa-automation/actions/runs/38841064770) | all jobs passed |
| Build and format | [38841064768](https://github.com/muhib-karim/global360-qa-automation/actions/runs/38841064768) | `build-and-format`, `gitleaks` and `docker` passed |
| CodeQL | [38841064805](https://github.com/muhib-karim/global360-qa-automation/actions/runs/38841064805) | passed, 0 open alerts on the pull request afterwards |

## End-to-end results

All runs passed on the first attempt; the CI-level retry was not needed. Durations are the test's own
duration from `run.json`.

| Job | Browser | Asset tag | Selected user | Seed | Duration | API cross-check |
| --- | --- | --- | --- | --- | --- | --- |
| `e2e (chromium)` | Chromium 153.0.8010.12 | `QA-5da3a13bf5e9` | Kevon Ankunding (id 843) | 1614591284 | 12.2 s | 200, matched |
| `e2e (firefox)` | Firefox 155.0 | `QA-5f45e4bb02d1` | Frances Armstrong (id 482) | 467258554 | 20.3 s | 200, matched |
| `e2e (webkit)` | WebKit 26.6 | `QA-ab6c5938b5a7` | Germaine Adams (id 795) | 605168222 | 24.6 s | 200, matched |
| `docker` | Chromium 153.0.8010.12 (in the image) | `QA-445b96a6cf7f` | Luis Ankunding (id 306) | 390511088 | 14.9 s | 200, matched |

For each run, the log shows every step from the README's flow, the list, detail and History assertions passing,
the axe results, and `GET /api/v1/hardware?search=<tag>` returning exactly the created asset with model
`Macbook Pro 13"`, status `Ready to Deploy` and the selected user's id. The merged Allure report in the same run
lists 3 tests, 3 passed, one per browser.

## Accessibility

All four runs reported the same axe results (WCAG 2.0/2.1 A and AA tags), equal to the committed baseline, so no
new critical rule was found:

| Page | Violated rules | Critical rules |
| --- | --- | --- |
| `login` | 1 | none |
| `create-asset` | 5 | `label` |
| `assets-list` | 6 | `button-name`, `label` |
| `asset-page` | 6 | `aria-required-children`, `button-name` |

The baseline itself was recorded from run
[38839660149](https://github.com/muhib-karim/global360-qa-automation/actions/runs/38839660149) with the same
results in all three browsers.

## Other findings from CI

- The demo's personal access token page (`/account/api`) reads "This feature has been disabled for the demo
  installation" (run [38840073553](https://github.com/muhib-karim/global360-qa-automation/actions/runs/38840073553),
  a temporary probe since removed). This is why the API cross-check uses the signed-in session instead.
- The first "Build and format" run on `main`
  ([38836389291](https://github.com/muhib-karim/global360-qa-automation/actions/runs/38836389291)) never started:
  its annotation reported an account billing or spending limit problem while the repository was private. A re-run
  after the repository became public passed without code changes.

## Toolchain seen in the logs

| Item | Version |
| --- | --- |
| Runner image | ubuntu-24.04 (24.04.5 LTS, image 20261012.334.1) |
| .NET SDK | 10.0.402 (from `global.json`) |
| Microsoft.Playwright.NUnit | 1.63.0 |
| NUnit / NUnit3TestAdapter | 4.6.1 / 6.3.1 |
| Allure.NUnit / Allure CLI | 2.15.0 / 2.46.1 (Java: Temurin 17.0.20.1, preinstalled) |
| Deque.AxeCore.Playwright | 4.14.0 |
| Snipe-IT demo | v8.8.0, build 25378 (footer of the demo pages in the screenshots) |
