# Skills used

Which agent skill drove which change, per release.

## v1.1.0

| Skill | What it drove |
| --- | --- |
| superpowers `brainstorming` | Scoped the release from the frozen brief (no questions allowed, so choices are recorded as my calls) into `docs/superpowers/specs/2026-10-21-v1.1.0-design.md`. |
| superpowers `writing-plans` | `docs/superpowers/plans/2026-10-21-v1.1.0.md`: tasks, interfaces and the review-focus inputs (non-English culture, ambiguous day/month, wrong weekday, date without time, all mirrors down). |
| superpowers `systematic-debugging` | Root cause before any fix: read the failing log, compared with the green nightly's History screenshot (24-hour format), traced the Firefox `#name` flake through the trace and the page's own script to the status AJAX layout shift. |
| superpowers `test-driven-development` | `SnipeItDateTests` and `QuarantineTests` pushed first and watched failing in CI, then implemented (no local SDK, so red and green were observed in GitHub Actions). |
| superpowers `verification-before-completion` | Nothing claimed without a run: two green E2E runs on the branch plus green Build, CodeQL and E2E on the final head, all cited in `docs/verification.md`. |
| superpowers `requesting-code-review` | One independent review of the branch diff before the final push; its findings and what was done with them are in the final commit. |
| `ui-ux-pro-max` | Guidance for the Latest E2E run block: an atomic `role="status"` summary, never colour alone (icon plus text), a semantic table with tabular numerals. |
| `impeccable` | Polish pass and its detector on `docs/index.html`: fixed white-on-accent contrast in dark mode (2.5:1) and the heading order the new section introduced; added focus rings and selection colours. |
| `archify` | `docs/architecture.html`: a code-backed architecture diagram pinned to a commit, every box linked to its source lines; all automated gates (validate, deliver, check, browser-check) passed. |
| `graphify` | AST graph of the test project (212 nodes, 558 edges) used to name the real hubs in the README architecture section. |
| `understand` (Understand Anything) | Not run: the graphify graph and the archify diagram already covered the architecture section, and its multi-agent pipeline does not fit this memory-tight box. |
