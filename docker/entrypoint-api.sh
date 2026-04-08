#!/bin/sh
set -e
# Named volume mounts often own /app/UploadedFiles as root; API runs as user app → permission denied → HTTP 500 on upload.
mkdir -p /app/logs /app/UploadedFiles
chown -R app:app /app/logs /app/UploadedFiles
exec runuser -u app -- dotnet /app/OneClickEcho.Api.dll
