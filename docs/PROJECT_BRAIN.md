# Project brain: global360-qa-automation

Handoff note for the next session. Short on purpose; details live in HANDOFF.md and git history.

## Status

v1.1.0 implemented on `upgrade/v1.1.0` (plan tasks 1-5 done). E2E green twice on the branch (runs 39693770413,
39694502145); Build and CodeQL green on 4d17ca3. Code review of the branch in progress.

## Next step

Apply review findings, push, confirm every workflow green on the head, record runs in docs/verification.md.

## Decisions

- The History date is parsed with InvariantCulture against Snipe-IT's own display formats, never matched by a
  loose regex.
- Workflows also run on pushes to `upgrade/**`; Pages deploys only from `main`.

## Gotchas

- No .NET SDK on the dev box (disk and memory are tight): build and tests run in CI.
- The shared demo's date format setting changes over time (seen: `Wed Oct 21, 2026 4:31AM`, `10/16/2026 07:20`).

## Session log
2026-10-21 23:50Z claude-code/claude-opus-5-5: date fix, install robustness, quarantine, Pages report, docs done; review and final green head next.
