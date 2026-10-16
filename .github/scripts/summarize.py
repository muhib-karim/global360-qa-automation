"""Writes a Markdown summary of one E2E job from its TRX files and run.json files.

Usage: summarize.py <browser> <outcome> <results-dir> <artifacts-dir>
Exits 1 if a run.json reports a different browser than requested, or if the test step reports success but its
final TRX shows no passed test (missing results, zero tests, or all skipped), so neither a misconfigured matrix
nor a filter that matches nothing can pass silently.
"""
import glob
import json
import os
import sys
import xml.etree.ElementTree as ET

browser, outcome, results_dir, artifacts_dir = sys.argv[1:5]
ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}

lines = [f"## E2E on {browser}: {outcome or 'not run'}", ""]
lines += ["| Attempt | Total | Passed | Failed |", "| --- | --- | --- | --- |"]
final_passed = None
for trx in sorted(glob.glob(os.path.join(results_dir, "**", "*.trx"), recursive=True)):
    counters = ET.parse(trx).getroot().find("t:ResultSummary/t:Counters", ns)
    if counters is not None:
        attempt = os.path.basename(os.path.dirname(trx))
        lines.append(f"| {attempt} | {counters.get('total')} | {counters.get('passed')} | {counters.get('failed')} |")
        final_passed = int(counters.get("passed") or 0)

mismatch = False
runs = sorted(glob.glob(os.path.join(artifacts_dir, "**", "run.json"), recursive=True))
if runs:
    lines += ["", "| Run | Outcome | Browser | Asset tag | User | Seed | Duration (s) | API | New critical a11y rules |",
              "| --- | --- | --- | --- | --- | --- | --- | --- | --- |"]
for path in runs:
    with open(path, encoding="utf-8") as f:
        run = json.load(f)
    user = run.get("user") or {}
    new_rules = run.get("accessibilityNewCritical") or {}
    lines.append("| {} | {} | {} {} | `{}` | {} (id {}) | {} | {} | {} | {} |".format(
        os.path.basename(os.path.dirname(path)), run.get("outcome"), run.get("browser"), run.get("browserVersion"),
        run.get("assetTag"), user.get("DisplayName"), user.get("Id"), run.get("userSeed"),
        run.get("durationSeconds"), run.get("apiStatus"), sum(len(v) for v in new_rules.values())))
    mismatch |= run.get("browser") != browser

if mismatch:
    lines += ["", f"**Browser mismatch: expected {browser}.**"]
nothing_ran = outcome.startswith("passed") and not final_passed
if nothing_ran:
    lines += ["", "**The test step passed, but its final TRX has no passed test: nothing was really verified.**"]
print("\n".join(lines))
sys.exit(1 if mismatch or nothing_ran else 0)
