# Snipe-IT asset lifecycle UI test

A .NET 10 / Playwright / NUnit test for the public [Snipe-IT demo](https://demo.snipeitapp.com/login).

The test uses the UI for every business action. It does not create assets or read their history through an API.

## Run

Requirements:

- .NET SDK 10.0.402 (the stable SDK used for verification; `global.json` allows later patches).
- PowerShell 7 (`pwsh`) for Playwright's browser installer.
- Internet access to the public demo and NuGet.

From the repository root:

```sh
dotnet restore --locked-mode
dotnet build --no-restore
pwsh tests/SnipeIt.UiTests/bin/Debug/net10.0/playwright.ps1 install chromium
dotnet test --no-build --logger "trx;LogFileName=asset-lifecycle.trx" -- NUnit.NumberOfTestWorkers=0
```

On a fresh Linux machine, use `install --with-deps chromium` instead of `install chromium` for the browser's OS dependencies. That step may need administrator rights. The tests themselves do not.

To watch the test:

```sh
HEADED=1 dotnet test --no-build -- NUnit.NumberOfTestWorkers=0
```

In PowerShell, set `$env:HEADED = "1"` first, then run `dotnet test --no-build -- NUnit.NumberOfTestWorkers=0`.

## What is checked

One end-to-end test keeps the lifecycle in one fresh browser context:

1. Sign in with the demo's published admin credentials.
2. Create a unique asset tag, name, serial and note, choose **Macbook Pro 13"**, set **Ready to Deploy**, and check it out to a randomly selected user during creation.
3. Search the assets list by the unique tag. Check its tag, name, serial, model, status and assigned user before opening the asset from that row.
4. Check the asset page's visible tag, name, serial, model, note, deployable status, deployed badge and assigned user.
5. Open **History**. Require exactly one `create new` row and one `checkout` row. Bind both rows to this asset, its model, the signed-in actor, a timestamp and quantity 1. Require an empty creation target, the selected user as checkout target and the checkout-on-creation note.

"Ready to Deploy" is the asset's status label. Snipe-IT also displays "Deployed" after checkout; these are not contradictory.

## Configuration

All settings are optional:

| Variable | Default | Purpose |
| --- | --- | --- |
| `SNIPE_BASE_URL` | `https://demo.snipeitapp.com` | Only HTTPS on this demo host is allowed, to prevent accidental writes to a real inventory. |
| `SNIPE_USERNAME` | `admin` | Demo login username. |
| `SNIPE_PASSWORD` | `password` | Demo login password. These defaults are published on the demo login page, not private credentials. |
| `SNIPE_USER_SEED` | A generated integer | Seed for choosing a user. The seed and selected user are printed and saved. |
| `HEADED` | Unset | Set to `1` to display Chromium. |

User selection is uniform among the eligible options in the first loaded user-dropdown page, not among every user in the database. A fixed seed is useful while debugging, but the shared demo's user list can change.

## Results and diagnostics

The console prints the asset tag, selected user, final URL and artifact directory. Each run has a unique artifact directory under the test work directory (normally `tests/SnipeIt.UiTests/bin/Debug/net10.0/artifacts/`). It contains:

- `asset-details.png` and `asset-history.png` after their checks pass.
- `run.json` with the input fields, selected user, seed, actor, asset URL and UTC verification time on success.
- `trace.zip` on success or failure, attached to the test result.
- `failure.html` and `failure.png` on failure.

The TRX file is in `tests/SnipeIt.UiTests/TestResults/`. For more console output, add `--logger "console;verbosity=detailed"` to `dotnet test`.

Open a trace locally:

```sh
pwsh tests/SnipeIt.UiTests/bin/Debug/net10.0/playwright.ps1 show-trace path/to/trace.zip
```

Artifacts are git-ignored. Traces and HTML can include demo session data, so keep them private rather than publishing them with the source.

## Design and trade-offs

- Page objects own interactions and UI assertions. The test reads as the requested workflow; `Select2` handles the demo's custom dropdowns.
- Prefer accessible labels and roles. Use stable IDs where the demo's labels are not associated correctly or where tab labels include an icon/count. History columns are resolved by their `data-field` header, rather than fixed cell numbers.
- Playwright waits for elements and asserted outcomes. There are no fixed sleeps, forced clicks, test retries, response stubs or direct write endpoints.
- Each run gets a fresh context and a GUID-based asset identity. The test is nonparallel because this is a shared public demo.
- The test leaves its unique `QA-` asset in place for inspection. It never edits or deletes existing inventory. The public demo may reset data or change while a run is in progress; a saved asset link is not permanent evidence. This is deliberately not a production cleanup policy.
- The scope is the requested Chromium workflow, not cross-browser coverage, performance testing or a full inventory regression suite.

The exact dependencies are in the project file and NuGet lock file. See [verification notes](docs/verification.md) for the checked run.
