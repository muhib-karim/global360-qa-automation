# Verification

Everything below comes from GitHub Actions logs and artifacts. Nothing here was run on a developer machine.
The demo is shared and resets, so the asset links in this file stop working after a reset; the runs and their
artifacts (kept for 14 days, the Allure report for 30) are the evidence.

## v1.1.0 on `upgrade/v1.1.0`

Workflows run on pushes to `upgrade/**`, so every run below is on the branch itself (no pull request yet).
"passed-on-retry" means the job's first attempt failed and the one CI-level retry passed; each of those first
failures was investigated and fixed, see [HANDOFF.md](../HANDOFF.md#three-more-flakes-found-on-the-way).

| Commit | Workflow | Run | Result |
| --- | --- | --- | --- |
| `6998caa` | Build and format | [39693113795](https://github.com/muhib-karim/global360-qa-automation/actions/runs/39693113795) | red on purpose: the new `SnipeItDate` tests fail against the stub |
| `6998caa` | E2E | [39693113645](https://github.com/muhib-karim/global360-qa-automation/actions/runs/39693113645) | red: reproduces the nightly's History date failure (`Wed Oct 21, 2026 7:25PM`) |
| `47b3371` | Build and format | [39693770403](https://github.com/muhib-karim/global360-qa-automation/actions/runs/39693770403) | all 26 date test cases pass; the new quarantine test fails on purpose against its stub; `docker` E2E passed |
| `47b3371` | E2E | [39693770413](https://github.com/muhib-karim/global360-qa-automation/actions/runs/39693770413) | green 1: chromium and firefox passed, webkit passed-on-retry (success flash) |
| `4d17ca3` | E2E | [39694502145](https://github.com/muhib-karim/global360-qa-automation/actions/runs/39694502145) | green 2: chromium, firefox and webkit passed on the first attempt, browser cache restored; quarantine job and report site passed |
| `4d17ca3` | Build and format, CodeQL | [39694502206](https://github.com/muhib-karim/global360-qa-automation/actions/runs/39694502206), [39694502156](https://github.com/muhib-karim/global360-qa-automation/actions/runs/39694502156) | passed |
| `890b89e` | E2E | [39695719121](https://github.com/muhib-karim/global360-qa-automation/actions/runs/39695719121) | green 3: firefox and webkit passed, chromium passed-on-retry (Optional Information toggle) |
| `890b89e` | Build and format, CodeQL | [39695718990](https://github.com/muhib-karim/global360-qa-automation/actions/runs/39695718990), [39695719113](https://github.com/muhib-karim/global360-qa-automation/actions/runs/39695719113) | passed |

| `caf2328` | E2E | [39696445347](https://github.com/muhib-karim/global360-qa-automation/actions/runs/39696445347) | green 4: chromium, firefox and webkit passed on the first attempt (toggle fix) |
| `caf2328` | Build and format, CodeQL | [39696445303](https://github.com/muhib-karim/global360-qa-automation/actions/runs/39696445303), [39696445332](https://github.com/muhib-karim/global360-qa-automation/actions/runs/39696445332) | passed |
| `e518e82` | Build and format | [39697314643](https://github.com/muhib-karim/global360-qa-automation/actions/runs/39697314643) | red: `dotnet format` IDE1006 on a private const added with the alert fix |
| `e518e82` | E2E, CodeQL | [39697314522](https://github.com/muhib-karim/global360-qa-automation/actions/runs/39697314522), [39697314429](https://github.com/muhib-karim/global360-qa-automation/actions/runs/39697314429) | passed; code scanning alerts #14 and #15 fixed on the branch |
| `6976329` | E2E | [39697754166](https://github.com/muhib-karim/global360-qa-automation/actions/runs/39697754166) | green 5: chromium, firefox and webkit passed on the first attempt |
| `6976329` | Build and format, CodeQL | [39697754086](https://github.com/muhib-karim/global360-qa-automation/actions/runs/39697754086), [39697754098](https://github.com/muhib-karim/global360-qa-automation/actions/runs/39697754098) | passed; no open code scanning alerts on the branch |

Code scanning: alert #14 (`cs/xmldoc/missing-summary`, `LoginPage.SignInAsync` had a `<returns>` without a
`<summary>`) is fixed on the branch, and a scan of every doc comment under `tests/` found no other. Alert #15
(`cs/useless-assignment-to-local`) pointed into the regex source generator's output for two `[GeneratedRegex]`
methods this release had added; both were replaced by plain string and CSS selector code. Alert #14 stays open on
`main` until the merge, when CodeQL's run on `main` closes it.

Later commits on the branch only update this file and the handoff note; their runs are on the branch's Actions page.

## Release v1.0.0 on `main`

Tag [`v1.0.0`](https://github.com/muhib-karim/global360-qa-automation/releases/tag/v1.0.0) is commit
[`94054ff`](https://github.com/muhib-karim/global360-qa-automation/commit/94054ff), the squash merge of pull request [#6](https://github.com/muhib-karim/global360-qa-automation/pull/6). Its push to `main`
ran every workflow green:

| Workflow | Run | Result |
| --- | --- | --- |
| E2E | [38854731769](https://github.com/muhib-karim/global360-qa-automation/actions/runs/38854731769) | chromium, firefox, webkit, Allure report and Pages publish passed |
| Build and format | [38854731823](https://github.com/muhib-karim/global360-qa-automation/actions/runs/38854731823) | `build-and-format`, `gitleaks` and `docker` passed |
| CodeQL | [38854731798](https://github.com/muhib-karim/global360-qa-automation/actions/runs/38854731798) | passed |

The published report from `main` is at [/report/](https://muhib-karim.github.io/global360-qa-automation/report/).
The detailed results below were recorded on the pull request before the merge.

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
