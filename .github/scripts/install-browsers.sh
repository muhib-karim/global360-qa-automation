#!/usr/bin/env bash
# Usage: install-browsers.sh <browser>
# Installs a Playwright browser and its OS packages so that one download host being down does not fail the run.
# Up to INSTALL_ATTEMPTS attempts (default 4) with exponential backoff from BACKOFF_SECONDS (default 10: 10 s,
# 20 s, 40 s). Attempt 1 uses Playwright's own host list; every later attempt pins PLAYWRIGHT_DOWNLOAD_HOST to
# the next alternate mirror in PLAYWRIGHT_MIRRORS. A browser that is already installed (for example restored
# from the Actions cache) is not downloaded again.
# INSTALLER replaces the Playwright CLI and INSTALL_ARGS its options (default --with-deps; empty in Docker, where
# the OS packages are a separate root layer). The build job uses INSTALLER to self-test this script offline.
set -uo pipefail
browser="${1:?usage: install-browsers.sh <browser>}"
installer="${INSTALLER:-pwsh tests/SnipeIt.UiTests/bin/Release/net10.0/playwright.ps1}"
install_args="${INSTALL_ARGS---with-deps}"
attempts="${INSTALL_ATTEMPTS:-4}"
delay="${BACKOFF_SECONDS:-10}"
read -r -a mirrors <<< "${PLAYWRIGHT_MIRRORS:-https://cdn.playwright.dev https://playwright.download.prss.microsoft.com/dbazure/download/playwright}"
for ((attempt = 1; attempt <= attempts; attempt++)); do
  if [ "$attempt" -gt 1 ]; then
    export PLAYWRIGHT_DOWNLOAD_HOST="${mirrors[$(((attempt - 2) % ${#mirrors[@]}))]}"
    echo "::warning title=Browser install retry::Attempt $((attempt - 1)) failed, retrying in $delay s from $PLAYWRIGHT_DOWNLOAD_HOST"
    sleep "$delay"
    delay=$((delay * 2))
  fi
  # shellcheck disable=SC2086 # the installer and its options are word lists
  if $installer install $install_args "$browser"; then
    echo "Installed $browser on attempt $attempt"
    exit 0
  fi
done
echo "::error title=Browser install failed::$browser was not installed after $attempts attempts"
exit 1
