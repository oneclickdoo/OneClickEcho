FROM alpine:3.20
RUN apk add --no-cache gzip findutils
COPY docker/log-rotation/rotate.sh /usr/local/bin/rotate.sh
RUN chmod +x /usr/local/bin/rotate.sh
# Hourly rotation; loop inherits LOG_* from docker-compose environment (unlike busybox crond).
CMD ["sh", "-c", "/usr/local/bin/rotate.sh; while sleep 3600; do /usr/local/bin/rotate.sh; done"]
