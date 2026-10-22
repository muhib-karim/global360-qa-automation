"""Assembles the GitHub Pages site: the docs landing page, the merged Allure report, each browser's VSTest HTML
report from its final attempt, and latest.json with the pass counts the landing page shows.

Usage: build-site.py <docs-dir> <allure-report-dir> <test-results-dir> <out-dir> [expected,browsers]
<test-results-dir> holds one test-results-<browser> folder per matrix job (actions/download-artifact layout).
An expected browser without a TRX (its job died before the tests) is listed as missing, never dropped.
Exits 1 if no browser has a TRX, so an empty report is never published as if it were a run.
"""
import datetime
import glob
import json
import os
import shutil
import sys
import xml.etree.ElementTree as ET

docs, allure, results, out = sys.argv[1:5]
expected = [b for b in (sys.argv[5] if len(sys.argv) > 5 else "").split(",") if b]
ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}

shutil.copytree(docs, out, ignore=shutil.ignore_patterns("*.md", "superpowers"), dirs_exist_ok=True)
if os.path.isdir(allure):
    shutil.copytree(allure, os.path.join(out, "report"), dirs_exist_ok=True)
os.makedirs(os.path.join(out, "runs"), exist_ok=True)

browsers = []
for folder in sorted(glob.glob(os.path.join(results, "test-results-*"))):
    browser = os.path.basename(folder).removeprefix("test-results-")
    trx_files = sorted(glob.glob(os.path.join(folder, "attempt-*", "*.trx")))
    if not trx_files:
        continue
    final = trx_files[-1]  # attempt-2 only exists when attempt 1 failed
    counters = ET.parse(final).getroot().find("t:ResultSummary/t:Counters", ns)
    entry = {
        "browser": browser,
        "attempt": os.path.basename(os.path.dirname(final)),
        "total": int(counters.get("total")),
        "passed": int(counters.get("passed")),
        "failed": int(counters.get("failed")),
        "html": None,
    }
    html = glob.glob(os.path.join(os.path.dirname(final), "*.html"))
    if html:
        entry["html"] = f"runs/{browser}.html"
        shutil.copyfile(html[0], os.path.join(out, entry["html"]))
    browsers.append(entry)

if not browsers:
    print("No TRX found under", results)
    sys.exit(1)
found = {b["browser"] for b in browsers}
browsers += [{"browser": b, "attempt": None, "total": 0, "passed": 0, "failed": 0, "html": None, "missing": True}
             for b in expected if b not in found]

statistic = None
summary = os.path.join(allure, "widgets", "summary.json")
if os.path.isfile(summary):
    with open(summary, encoding="utf-8") as f:
        statistic = json.load(f)["statistic"]

server, repo, run_id = (os.environ.get(k, "") for k in ("GITHUB_SERVER_URL", "GITHUB_REPOSITORY", "GITHUB_RUN_ID"))
latest = {
    "generated": datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
    "commit": os.environ.get("GITHUB_SHA", "")[:7],
    "branch": os.environ.get("GITHUB_REF_NAME", ""),
    "run": f"{server}/{repo}/actions/runs/{run_id}" if run_id else None,
    "browsers": browsers,
    "allure": statistic,
}
with open(os.path.join(out, "latest.json"), "w", encoding="utf-8") as f:
    json.dump(latest, f, indent=2)
print(json.dumps(latest, indent=2))
