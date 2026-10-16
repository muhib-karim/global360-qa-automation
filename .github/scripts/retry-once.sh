#!/usr/bin/env bash
# Usage: retry-once.sh '<command>'
# Runs the command once (ATTEMPT=1) and, only if it fails, once more (ATTEMPT=2). A demo preflight runs before
# each attempt. The retry is reported as a warning and the result is written to $GITHUB_OUTPUT as "outcome":
# passed, passed-on-retry or failed. Tests themselves never retry.
set -uo pipefail
here="$(dirname "$0")"
out="${GITHUB_OUTPUT:-/dev/null}"
for attempt in 1 2; do
  export ATTEMPT=$attempt
  echo "::group::Attempt $attempt"
  if "$here/wait-for-demo.sh" && bash -c "$1"; then
    echo "::endgroup::"
    if [ "$attempt" = 1 ]; then echo "outcome=passed" >> "$out"; else echo "outcome=passed-on-retry" >> "$out"; fi
    exit 0
  fi
  echo "::endgroup::"
  if [ "$attempt" = 1 ]; then
    echo "::warning title=CI-level retry::Attempt 1 failed, re-running once. Both attempts are kept in the artifacts."
  fi
done
echo "outcome=failed" >> "$out"
exit 1
