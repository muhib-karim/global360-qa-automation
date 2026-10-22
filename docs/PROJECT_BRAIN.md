# Project brain: global360-qa-automation

Handoff note for the next session. Short on purpose; details live in HANDOFF.md and git history.

## Status

v1.1.0 complete on `upgrade/v1.1.0` pending the final head's CI. E2E green on the branch three times (runs
39693770413, 39694502145, 39695719121), code review applied, three asset-form flakes fixed (see HANDOFF.md). Code scanning alerts #14 and #15 fixed on
the branch; all workflows green on 6976329 (runs in docs/verification.md).

## Next step

Coordinator: open the PR, review, merge, tag v1.1.0. Pages and the main badges turn green with the first run on
`main` after the merge.

## Decisions

- The History date is parsed with InvariantCulture against Snipe-IT's own display formats, never matched by a
  loose regex.
- Workflows also run on pushes to `upgrade/**`; Pages deploys only from `main`.

## Gotchas

- No .NET SDK on the dev box (disk and memory are tight): build and tests run in CI. Check `.editorconfig`
  naming rules by hand: private fields, consts included, need `_camelCase` (IDE1006 fails `dotnet format`).
- The asset form's "Optional Information" open state is a cookie replayed on document ready: open it through
  `OptionalInfoSection`, never with a blind click.
- The shared demo's date format setting changes over time (seen: `Wed Oct 21, 2026 4:31AM`, `10/16/2026 07:20`).

## Session log
2026-10-21 23:50Z claude-code/claude-opus-5-5: date fix, install robustness, quarantine, Pages report, docs done; review and final green head next.
2026-10-22 00:00Z claude-code/claude-opus-5-5: review fixes, Optional Information toggle fix, verification table; final head CI next.
2026-10-22 00:15Z claude-code/claude-opus-5-5: fixed code scanning alerts #14 and #15, IDE1006 follow-up; all green on 6976329; awaiting coordinator merge.
