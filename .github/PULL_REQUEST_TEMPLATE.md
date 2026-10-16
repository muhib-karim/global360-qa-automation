## What and why

<!-- One or two sentences. Link the issue if there is one. -->

## How it was verified

- [ ] `build-and-format` is green (locked restore, Release build with warnings as errors, `dotnet format`, unit tests, guardrails)
- [ ] `e2e` is green on chromium, firefox and webkit, or the failure is explained below
- [ ] Any CI-level retry in the run is explained
- [ ] README and docs still match the behaviour (every claim backed by a CI run)
- [ ] If `accessibility-baseline.json` changed: the reason and the run that shows the new rules
