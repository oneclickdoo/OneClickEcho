FROM alpine:3.20
RUN apk add --no-cache gzip findutils
COPY docker/log-rotation/rotate.sh /usr/local/bin/rotate.sh
RUN chmod +x /usr/local/bin/rotate.sh
# Hourly at minute 0 (busybox crond; crontab format: min hour day month weekday command)
RUN echo "0 * * * * /usr/local/bin/rotate.sh" > /etc/crontabs/root
CMD ["crond", "-f", "-l", "2"]
