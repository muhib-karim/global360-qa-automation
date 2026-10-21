# Project brain: global360-qa-automation

Handoff note for the next session. Short on purpose; details live in HANDOFF.md and git history.

## Status

v1.1.0 in progress on `upgrade/v1.1.0`. Spec and plan in `docs/superpowers/`.

## Next step

Task 1 of the plan: `SnipeItDate` parser, test first.

## Decisions

- The History date is parsed with InvariantCulture against Snipe-IT's own display formats, never matched by a
  loose regex.
- Workflows also run on pushes to `upgrade/**`; Pages deploys only from `main`.

## Gotchas

- No .NET SDK on the dev box (disk and memory are tight): build and tests run in CI.
- The shared demo's date format setting changes over time (seen: `Wed Oct 21, 2026 4:31AM`, `10/16/2026 07:20`).

## Session log
