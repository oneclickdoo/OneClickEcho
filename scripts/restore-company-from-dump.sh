#!/usr/bin/env bash
# Selective restore: one company (default biosvet) from a pg_dump custom file
# into the live oneclickecho database, without touching other companies.
#
# Server example:
#   cd /var/www/OneClickEcho
#   docker compose --profile full stop api dashboard
#   COMPANY_ID=075fe381-fda7-4d94-aaf1-5d72ec07a2eb DUMP_PATH=/root/oneclickecho.dump ./scripts/restore-company-from-dump.sh
#   docker compose --profile full up -d api dashboard
#
# Local Windows (Git Bash / WSL) with postgres container oneclickecho.postgres17:
#   PG_CONTAINER=oneclickecho.postgres17 PG_USER=postgres DUMP_PATH=/c/Users/Admin/Desktop/oneclickecho.dump ./scripts/restore-company-from-dump.sh

set -euo pipefail

COMPANY_NAME="${COMPANY_NAME:-biosvet}"
# Optional: exact UUID from dump (overrides name lookup when set)
COMPANY_ID="${COMPANY_ID:-}"
PG_CONTAINER="${PG_CONTAINER:-oneclick_postgres}"
PG_USER="${PG_USER:-oneclickecho_admin}"
TARGET_DB="${TARGET_DB:-oneclickecho}"
SRC_DB="${SRC_DB:-oneclickecho_src}"
DUMP_PATH="${DUMP_PATH:-/root/oneclickecho.dump}"
BACKUP_PATH="${BACKUP_PATH:-/root/backup_before_${COMPANY_NAME}_restore.dump}"

psql_cmd() {
  local db="$1"
  shift
  docker exec -i "$PG_CONTAINER" psql -v ON_ERROR_STOP=1 -U "$PG_USER" -d "$db" "$@"
}

psql_scalar() {
  local db="$1"
  local sql="$2"
  psql_cmd "$db" -tAc "$sql" | tr -d '\r' | sed '/^$/d' | head -1
}

copy_rows() {
  local table="$1"
  local where="$2"
  local tmp="/tmp/restore_${table}.csv"
  echo "  copy $table ..."
  docker exec "$PG_CONTAINER" psql -U "$PG_USER" -d "$SRC_DB" -c "\\copy (SELECT * FROM ${table} WHERE ${where}) TO '${tmp}' WITH (FORMAT csv, HEADER true)"
  docker exec "$PG_CONTAINER" psql -U "$PG_USER" -d "$TARGET_DB" -c "DELETE FROM ${table} WHERE ${where}"
  docker exec "$PG_CONTAINER" psql -U "$PG_USER" -d "$TARGET_DB" -c "\\copy ${table} FROM '${tmp}' WITH (FORMAT csv, HEADER true)"
  docker exec "$PG_CONTAINER" rm -f "$tmp"
}

echo "==> Backup target database to ${BACKUP_PATH}"
docker exec "$PG_CONTAINER" pg_dump -U "$PG_USER" -Fc -f "/tmp/pre_restore.backup" "$TARGET_DB"
docker cp "${PG_CONTAINER}:/tmp/pre_restore.backup" "$BACKUP_PATH"
ls -lh "$BACKUP_PATH"

echo "==> Restore dump into temporary database ${SRC_DB}"
docker cp "$DUMP_PATH" "${PG_CONTAINER}:/tmp/oneclickecho.dump"
psql_cmd postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname IN ('${SRC_DB}', '${TARGET_DB}') AND pid <> pg_backend_pid();" >/dev/null || true
psql_cmd postgres -c "DROP DATABASE IF EXISTS ${SRC_DB};"
psql_cmd postgres -c "CREATE DATABASE ${SRC_DB};"
docker exec "$PG_CONTAINER" pg_restore -U "$PG_USER" -d "$SRC_DB" --no-owner --role="$PG_USER" /tmp/oneclickecho.dump 2>&1 | tail -20 || true

if [[ -n "$COMPANY_ID" ]]; then
  SRC_CID="$COMPANY_ID"
  if [[ -z "$(psql_scalar "$SRC_DB" "SELECT 1 FROM companies WHERE id = '${SRC_CID}'::uuid;")" ]]; then
    echo "ERROR: COMPANY_ID ${SRC_CID} not found in dump (database ${SRC_DB})."
    exit 1
  fi
else
  SRC_CID="$(psql_scalar "$SRC_DB" "SELECT id::text FROM companies WHERE lower(name) LIKE lower('%${COMPANY_NAME}%') ORDER BY created_at LIMIT 1;")"
  if [[ -z "$SRC_CID" ]]; then
    echo "ERROR: Company matching '${COMPANY_NAME}' not found in dump (database ${SRC_DB})."
    psql_cmd "$SRC_DB" -c "SELECT id, name FROM companies ORDER BY name;"
    exit 1
  fi
fi
echo "==> Source company id: ${SRC_CID}"
psql_cmd "$SRC_DB" -c "SELECT id, name, sms_username, LEFT(COALESCE(api_password,''), 4) AS api_pw_prefix FROM companies WHERE id = '${SRC_CID}';"

TGT_CID="$(psql_scalar "$TARGET_DB" "SELECT id::text FROM companies WHERE lower(name) LIKE lower('%${COMPANY_NAME}%') ORDER BY created_at LIMIT 1;")"
if [[ -n "$TGT_CID" && "$TGT_CID" != "$SRC_CID" ]]; then
  echo "==> Target has different id (${TGT_CID}); removing old ${COMPANY_NAME} rows first."
fi

echo "==> Delete existing ${COMPANY_NAME} data in ${TARGET_DB}"
psql_cmd "$TARGET_DB" <<SQL
DELETE FROM viber_delivery_events
WHERE campaign_lead_id IN (
  SELECT cl.id FROM campaign_leads cl
  JOIN campaigns c ON c.id = cl.campaign_id
  WHERE c.company_id IN (SELECT id FROM companies WHERE lower(name) LIKE lower('%${COMPANY_NAME}%'))
     OR c.company_id = '${SRC_CID}'::uuid
);

DELETE FROM received_messages
WHERE campaign_lead_id IN (
  SELECT cl.id FROM campaign_leads cl
  JOIN campaigns c ON c.id = cl.campaign_id
  WHERE c.company_id IN (SELECT id FROM companies WHERE lower(name) LIKE lower('%${COMPANY_NAME}%'))
     OR c.company_id = '${SRC_CID}'::uuid
);

DELETE FROM campaign_leads
WHERE campaign_id IN (
  SELECT id FROM campaigns
  WHERE company_id IN (SELECT id FROM companies WHERE lower(name) LIKE lower('%${COMPANY_NAME}%'))
     OR company_id = '${SRC_CID}'::uuid
);

DELETE FROM campaign_lead_collections
WHERE campaign_id IN (
  SELECT id FROM campaigns
  WHERE company_id IN (SELECT id FROM companies WHERE lower(name) LIKE lower('%${COMPANY_NAME}%'))
     OR company_id = '${SRC_CID}'::uuid
);

DELETE FROM campaigns
WHERE company_id IN (SELECT id FROM companies WHERE lower(name) LIKE lower('%${COMPANY_NAME}%'))
   OR company_id = '${SRC_CID}'::uuid;

DELETE FROM lead_assignments
WHERE lead_collection_id IN (
  SELECT id FROM lead_collections
  WHERE company_id IN (SELECT id FROM companies WHERE lower(name) LIKE lower('%${COMPANY_NAME}%'))
     OR company_id = '${SRC_CID}'::uuid
);

DELETE FROM lead_collections
WHERE company_id IN (SELECT id FROM companies WHERE lower(name) LIKE lower('%${COMPANY_NAME}%'))
   OR company_id = '${SRC_CID}'::uuid;

DELETE FROM leads
WHERE company_id IN (SELECT id FROM companies WHERE lower(name) LIKE lower('%${COMPANY_NAME}%'))
   OR company_id = '${SRC_CID}'::uuid;

DELETE FROM api_messages
WHERE company_id IN (SELECT id FROM companies WHERE lower(name) LIKE lower('%${COMPANY_NAME}%'))
   OR company_id = '${SRC_CID}'::uuid;

DELETE FROM test_messages
WHERE company_id IN (SELECT id FROM companies WHERE lower(name) LIKE lower('%${COMPANY_NAME}%'))
   OR company_id = '${SRC_CID}'::uuid;

DELETE FROM senders
WHERE company_id IN (SELECT id FROM companies WHERE lower(name) LIKE lower('%${COMPANY_NAME}%'))
   OR company_id = '${SRC_CID}'::uuid;

DELETE FROM application_user_companies
WHERE company_id IN (SELECT id FROM companies WHERE lower(name) LIKE lower('%${COMPANY_NAME}%'))
   OR company_id = '${SRC_CID}'::uuid;

DELETE FROM companies
WHERE lower(name) LIKE lower('%${COMPANY_NAME}%')
   OR id = '${SRC_CID}'::uuid;
SQL

echo "==> Copy company + related rows from ${SRC_DB}"
copy_rows companies "id = '${SRC_CID}'::uuid"
copy_rows senders "company_id = '${SRC_CID}'::uuid"
copy_rows leads "company_id = '${SRC_CID}'::uuid"
copy_rows lead_collections "company_id = '${SRC_CID}'::uuid"
copy_rows lead_assignments "lead_collection_id IN (SELECT id FROM lead_collections WHERE company_id = '${SRC_CID}'::uuid)"
copy_rows campaigns "company_id = '${SRC_CID}'::uuid"
copy_rows campaign_lead_collections "campaign_id IN (SELECT id FROM campaigns WHERE company_id = '${SRC_CID}'::uuid)"
copy_rows campaign_leads "campaign_id IN (SELECT id FROM campaigns WHERE company_id = '${SRC_CID}'::uuid)"
copy_rows received_messages "campaign_lead_id IN (SELECT cl.id FROM campaign_leads cl JOIN campaigns c ON c.id = cl.campaign_id WHERE c.company_id = '${SRC_CID}'::uuid)"
copy_rows viber_delivery_events "campaign_lead_id IN (SELECT cl.id FROM campaign_leads cl JOIN campaigns c ON c.id = cl.campaign_id WHERE c.company_id = '${SRC_CID}'::uuid)"
copy_rows api_messages "company_id = '${SRC_CID}'::uuid"
copy_rows test_messages "company_id = '${SRC_CID}'::uuid"
copy_rows application_user_companies "company_id = '${SRC_CID}'::uuid"

echo "==> Fix viber_message_id sequence"
psql_cmd "$TARGET_DB" -c "SELECT setval(pg_get_serial_sequence('campaign_leads', 'viber_message_id'), COALESCE((SELECT MAX(viber_message_id) FROM campaign_leads), 1));"

echo "==> Verification"
psql_cmd "$TARGET_DB" -c "
SELECT name, sms_username, LEFT(COALESCE(sms_password,''), 3) AS sms_pw, LEFT(COALESCE(api_password,''), 3) AS api_pw
FROM companies WHERE id = '${SRC_CID}'::uuid;
SELECT 'campaigns' AS what, COUNT(*) FROM campaigns WHERE company_id = '${SRC_CID}'::uuid
UNION ALL SELECT 'leads', COUNT(*) FROM leads WHERE company_id = '${SRC_CID}'::uuid
UNION ALL SELECT 'campaign_leads', COUNT(*) FROM campaign_leads WHERE campaign_id IN (SELECT id FROM campaigns WHERE company_id = '${SRC_CID}'::uuid);
"

echo "==> Drop temporary database ${SRC_DB}"
psql_cmd postgres -c "DROP DATABASE IF EXISTS ${SRC_DB};"

echo "Done. Backup: ${BACKUP_PATH}"
echo "Login itocs@oneclick.rs (and other global users) were NOT changed — only ${COMPANY_NAME} company data/credentials from dump."
