#!/usr/bin/env bash
# Preflight: wait until the shared demo serves /login with HTTP 200.
# The demo answers 503 ("Resetting the database") while it restores its data.
set -uo pipefail
url="${SNIPE_BASE_URL:-https://demo.snipeitapp.com}/login"
deadline=$((SECONDS + ${1:-360}))
while :; do
  code=$(curl -s -o /dev/null -w '%{http_code}' --max-time 20 "$url")
  if [ "$code" = 200 ]; then
    echo "Demo is up: $url returned 200"
    exit 0
  fi
  if [ "$SECONDS" -ge "$deadline" ]; then
    echo "::error title=Demo unavailable::$url returned $code until the preflight deadline"
    exit 1
  fi
  echo "Demo returned $code, waiting 20 s"
  sleep 20
done
