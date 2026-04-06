#!/bin/sh
# Compress Serilog rolling files older than 49 hours; delete .gz archives older than 45 days.
# Mount the same volume as the API uses for /app/logs (see docker-compose.yml).

set -eu
LOG_DIR="${LOG_DIR:-/app/logs}"

[ -d "$LOG_DIR" ] || exit 0

# Plain *.log not modified in the last 49 hours (49 * 60 = 2940 minutes)
find "$LOG_DIR" -maxdepth 1 -type f -name '*.log' ! -name '*.gz' -mmin +2940 -exec gzip -9 {} \; 2>/dev/null || true

# Remove gzip archives older than 45 days
find "$LOG_DIR" -maxdepth 1 -type f -name '*.gz' -mtime +45 -delete 2>/dev/null || true
