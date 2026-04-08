#!/usr/bin/env bash
set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ENV_FILE="$PROJECT_DIR/.env.local"
RUN_DIR="$PROJECT_DIR/run"
PID_FILE="$RUN_DIR/web.pid"
MARKER_BEGIN="# cej-tracker start"
MARKER_END="# cej-tracker end"

if [[ -f "$ENV_FILE" ]]; then
  set -a
  # shellcheck disable=SC1090
  source "$ENV_FILE"
  set +a
fi

if [[ -f "$PID_FILE" ]]; then
  PID="$(cat "$PID_FILE")"
  if kill -0 "$PID" 2>/dev/null; then
    kill "$PID"
    echo "Stopped dashboard process $PID."
  else
    echo "Dashboard PID file existed, but process $PID was not running."
  fi
  rm -f "$PID_FILE"
else
  echo "No dashboard PID file found."
fi

CURRENT_CRONTAB="$(crontab -l 2>/dev/null || true)"
printf '%s\n' "$CURRENT_CRONTAB" | sed "/$MARKER_BEGIN/,/$MARKER_END/d" | crontab -
echo "Removed cej-tracker cron job."

if [[ -f "$ENV_FILE" ]]; then
  (
    cd "$PROJECT_DIR"
    /usr/bin/env npm run test-discord -- stopped
  )
  echo "Sent shutdown confirmation Discord notification."
fi
