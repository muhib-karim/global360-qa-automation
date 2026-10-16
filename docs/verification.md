# Verification

Verified against the live public demo on 16 October 2026 (UTC).

| Item | Verified value |
| --- | --- |
| OS | Ubuntu 22.04, x64 |
| SDK | .NET 10.0.402 |
| Runtime | .NET 10.0.13 |
| Playwright | 1.63.0 |
| Browser | Bundled Chromium, headless |
| Build | 0 warnings, 0 errors; warnings treated as errors |
| Test | 1 passed, 0 failed, 0 skipped |
| Latest run duration | 16 seconds (17.717 seconds total runner time) |

The README's restore, build, PowerShell browser-install and test commands were run successfully. The strengthened History assertions passed on two consecutive runs with different generated assets and users. No retries are configured.

Latest run:

- Asset tag: `QA-9351c22be681`
- User selection seed: `1561467210`
- Selected demo user: Dimitri Anderson, ID `1499`
- Final page: https://demo.snipeitapp.com/hardware/2615#history
- Outcome: all list, detail and History assertions passed.

The screenshots, TRX, run JSON and Playwright trace were retained separately for review, not committed. The demo is shared and can reset, so the current link may not survive. This records a run, not a guarantee that the public service stays available or unchanged.
