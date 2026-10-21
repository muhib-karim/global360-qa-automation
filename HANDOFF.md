# Handoff: v1.1.0

Branch `upgrade/v1.1.0`, cut from `main` at `3e3fee5`. Not merged and no pull request opened: the coordinator
reviews and merges. Everything below was verified in GitHub Actions; this machine has no .NET SDK, so no test
ran locally.

## Why the E2E badge was red

The nightly E2E run [39569739741](https://github.com/muhib-karim/global360-qa-automation/actions/runs/39569739741)
(2026-10-21, `main`) failed for one real reason and logged a second, latent one.

### 1. The History date assertion (the failure)

`Macbook_can_be_created_checked_out_found_and_audited` failed in Chromium, Firefox and WebKit, on both attempts:

```text
Locator expected to have text matching regex '\b\d{4}\b.*\b\d{1,2}:\d{2}\b'
unexpected value "Wed Oct 21, 2026 4:31AM"
```

Root cause: the regex ends in `\b` after the minutes. In `4:31AM` the `A` is a word character right after the
`1`, so there is no word boundary and the pattern cannot match. It only passed while the shared demo used a
24-hour format. The green nightly the day before (run 39393494219) shows `2026-10-20 04:28` in its History
screenshot. The demo's date and time format is an admin setting on a shared instance (`date_display_format` plus
`time_display_format`), so it changes. The old code comment even listed `Fri Oct 16, 2026 2:04AM` as a format it
supported; it never did.

It was not the runner's locale or timezone: the cell is server-rendered text. Both timestamps are the demo's
server time at UTC-4 (`4:31AM` for a run at 08:31 UTC), and Playwright's `Locale = "en-US"` does not affect it.
The fix still makes the check independent of the runner, because parsing is where locale would bite.

Fix (`tests/SnipeIt.UiTests/Data/SnipeItDate.cs`, used by `AssetDetailsPage.VerifyHistoryAsync`):

- Parse with `DateTime.TryParseExact`, `CultureInfo.InvariantCulture` and exactly Snipe-IT's nine date formats
  times its three time formats (`g:iA`, `h:i A`, `H:i`). Day-of-week must match the date; `m/d/Y` versus `d/m/Y`
  ambiguity returns both readings.
- The parsed time must be a server-local time of this run in some real timezone: between the test start minus
  12 h and now plus 14 h. A stale row from yesterday or a garbage string fails.
- The `create new` and `checkout` rows must be within two minutes of each other.
- 26 unit test cases pin it, including a `th-TH` (Buddhist calendar) culture run, the failing string itself, a wrong
  weekday and a date without a time. They were pushed first and watched failing (build run 39693113795), then
  passing (39693770403).

This is stricter than the old regex, not looser: the regex accepted any four digits followed by any `h:mm`.

### 2. `getaddrinfo ENOTFOUND` during the browser install (latent)

The install step logged `ENOTFOUND playwright.download.prss.microsoft.com` for FFmpeg and the Firefox/WebKit
downloads. Playwright tried its next built-in host and succeeded, so it did not fail that run, but a wider outage
would have, and every run downloaded about 190 MiB again. Now:

- `.github/scripts/install-browsers.sh`: up to 4 attempts, backoff 10, 20, 40 s. Attempts 1 and 2 use
  Playwright's own host list (it already falls through its CDN hosts per file, which is what saved the nightly).
  Attempts 3 and 4 pin `PLAYWRIGHT_DOWNLOAD_HOST` to each official host on its own: the storage host that served
  the downloads that night, then the CDN host. Both belong to Playwright, so a total Playwright outage still
  fails the job; `PLAYWRIGHT_MIRRORS` takes an independent mirror (for example an internal artifact cache) if
  one is wanted. A third-party public mirror was deliberately not added: browser binaries would come unverified
  from outside Microsoft. Used by the E2E matrix, the quarantine job and the Dockerfile. The build job
  self-tests it offline with a fake installer that only succeeds once a host is pinned (attempt 3).
- `actions/cache` on `~/.cache/ms-playwright`, key `ms-playwright-<os>-<browser>-<Playwright version>`, the
  version read from `packages.lock.json` (the step fails if it cannot read one).

## Two more flakes found on the way

Both were in the create form and both showed up in runs on this branch or in the failing nightly:

- Firefox (nightly, attempt 2): `#name` never became visible. The trace shows the `#optional_info` click 67 ms
  after choosing the status. Choosing a status calls `/api/v1/statuslabels/<id>/deployable` and then fades in
  "This asset can be checked out." and the Checkout to section, which moves the layout under the click. The form
  now waits for that message (which also asserts the status is deployable) before the next click.
- WebKit (run 39693770413, attempt 1): no `.alert-success` after saving, although the page had redirected to the
  asset list. The demo does not always render the flash after a redirect, as the existing check-in, edit and
  delete steps already document. Creation is now verified the same way: the form is left without an error, then
  the list, the asset page and the API prove it.

## Flakiness controls

- One reported CI-level retry (unchanged). `failure-traces-<browser>` now holds the trace of every failed test,
  also when the retry passed.
- `[Quarantine("<why>, issue #<n>", "<yyyy-MM-dd>")]` (CONTRIBUTING.md): excluded from the gating jobs and the
  Docker image, run by the non-blocking `quarantine` job, expired by a unit test. Nothing is quarantined; the date
  test was fixed, not hidden.

## Pages report

The report job now builds the whole site (`build-site.py`): landing page with the latest pass counts per browser
(`latest.json`), each browser's VSTest HTML report under `runs/`, the merged Allure report under `report/`, and
the architecture diagram. Every run uploads it as `pages-site` and screenshots it (`site-screenshots`). Only
`main` deploys it: the `github-pages` environment allows `main` only, so the published site and the README badges
turn green with the first green run on `main` after the merge.

## Verification on the branch

See [docs/verification.md](docs/verification.md#v110-on-upgradev110) for the run table.

## For the coordinator

1. Open the pull request from `upgrade/v1.1.0`, review, merge.
2. The push to `main` runs every workflow and publishes Pages; check the badges and the Pages homepage.
3. Tag `v1.1.0` on the merge commit if releases are cut from tags.
4. Optional: drop the `upgrade/**` push trigger if branch verification without a pull request is not wanted.
