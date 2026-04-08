#!/usr/bin/env bash
set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ENV_FILE="$PROJECT_DIR/.env.local"
LOG_DIR="$PROJECT_DIR/logs"
RUN_DIR="$PROJECT_DIR/run"
PID_FILE="$RUN_DIR/web.pid"
MARKER_BEGIN="# cej-tracker start"
MARKER_END="# cej-tracker end"

if [[ ! -f "$ENV_FILE" ]]; then
  cp "$PROJECT_DIR/.env.local.example" "$ENV_FILE"
  echo "Created $ENV_FILE from template."
  echo "Update CEJ_DISCORD_WEBHOOK_URL, then run: npm run up"
  exit 1
fi

set -a
# shellcheck disable=SC1090
source "$ENV_FILE"
set +a

mkdir -p "$LOG_DIR" "$RUN_DIR"

required_vars=(
  CEJ_SEARCH_URL
  CEJ_DISCORD_WEBHOOK_URL
  CEJ_WEB_PORT
  CRON_SCHEDULE
)

for var_name in "${required_vars[@]}"; do
  if [[ -z "${!var_name:-}" ]]; then
    echo "Missing $var_name in $ENV_FILE"
    exit 1
  fi
done

if [[ -f "$PID_FILE" ]] && kill -0 "$(cat "$PID_FILE")" 2>/dev/null; then
  echo "Dashboard already running on PID $(cat "$PID_FILE")."
else
  (
    cd "$PROJECT_DIR"
    nohup /usr/bin/env npm run web >> "$LOG_DIR/web.log" 2>&1 &
    echo $! > "$PID_FILE"
  )
  echo "Started dashboard on http://localhost:${CEJ_WEB_PORT}"
fi

(
  cd "$PROJECT_DIR"
  /usr/bin/env npm run track >> "$LOG_DIR/track.log" 2>&1
)
echo "Ran tracker once immediately."

(
  cd "$PROJECT_DIR"
  /usr/bin/env npm run test-discord -- started >> "$LOG_DIR/track.log" 2>&1
)
echo "Sent startup confirmation Discord notification."

CRON_COMMAND="$CRON_SCHEDULE /bin/zsh -lc 'cd \"$PROJECT_DIR\" && set -a && source \"$ENV_FILE\" && set +a && /usr/bin/env npm run track >> \"$LOG_DIR/track.log\" 2>&1'"
CURRENT_CRONTAB="$(crontab -l 2>/dev/null || true)"
CLEAN_CRONTAB="$(printf '%s\n' "$CURRENT_CRONTAB" | sed "/$MARKER_BEGIN/,/$MARKER_END/d")"

{
  printf '%s\n' "$CLEAN_CRONTAB"
  printf '%s\n' "$MARKER_BEGIN"
  printf '%s\n' "$CRON_COMMAND"
  printf '%s\n' "$MARKER_END"
} | crontab -

echo "Installed cron schedule: $CRON_SCHEDULE"
echo "Logs:"
echo "  Web UI:   $LOG_DIR/web.log"
echo "  Tracker:  $LOG_DIR/track.log"
