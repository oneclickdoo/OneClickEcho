#!/bin/sh
# Compress Serilog *.log not touched for LOG_GZIP_MMIN minutes (default 49h).
# Delete *.gz whose mtime is older than LOG_DELETE_GZ_DAYS days (default 40).
# Mount the same volume as the API uses for /app/logs (see docker-compose.yml).

set -eu
LOG_DIR="${LOG_DIR:-/app/logs}"
# 49 hours = 2940 minutes
LOG_GZIP_MMIN="${LOG_GZIP_MMIN:-2940}"
LOG_DELETE_GZ_DAYS="${LOG_DELETE_GZ_DAYS:-40}"

[ -d "$LOG_DIR" ] || exit 0

find "$LOG_DIR" -maxdepth 1 -type f -name '*.log' ! -name '*.gz' -mmin "+${LOG_GZIP_MMIN}" -exec gzip -9 {} \; 2>/dev/null || true

find "$LOG_DIR" -maxdepth 1 -type f -name '*.gz' -mtime "+${LOG_DELETE_GZ_DAYS}" -delete 2>/dev/null || true
