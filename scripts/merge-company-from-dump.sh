#!/usr/bin/env bash
# Merge one company from a pg_dump into the live DB: INSERT rows that are missing by primary key (id).
# Does NOT delete or overwrite existing rows (keeps new campaigns created on the new server).
#
# Example (BioSvet from backup taken before a bad restore):
#   docker compose --profile full stop api dashboard
#   COMPANY_NAME=biosvet DUMP_PATH=/root/backup_before_biosvet_restore.dump bash ./scripts/merge-company-from-dump.sh
#   docker compose --profile full up -d api dashboard
#
# Reuse an already-loaded check DB (skip 700MB pg_restore):
#   SKIP_SRC_RESTORE=1 SRC_DB=oneclickecho_check COMPANY_NAME=biosvet bash ./scripts/merge-company-from-dump.sh

set -euo pipefail

COMPANY_NAME="${COMPANY_NAME:-biosvet}"
COMPANY_ID="${COMPANY_ID:-}"
PG_CONTAINER="${PG_CONTAINER:-oneclick_postgres}"
PG_USER="${PG_USER:-oneclickecho_admin}"
TARGET_DB="${TARGET_DB:-oneclickecho}"
SRC_DB="${SRC_DB:-oneclickecho_src}"
DUMP_PATH="${DUMP_PATH:-/root/oneclickecho.dump}"
SKIP_SRC_RESTORE="${SKIP_SRC_RESTORE:-0}"

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

get_common_columns() {
  local table="$1"
  local tgt_cols src_cols col result first=1
  tgt_cols="$(psql_cmd "$TARGET_DB" -tAc "
    SELECT column_name FROM information_schema.columns
    WHERE table_schema = 'public' AND table_name = '${table}'
    ORDER BY ordinal_position" | tr -d '\r')"
  src_cols="$(psql_cmd "$SRC_DB" -tAc "
    SELECT column_name FROM information_schema.columns
    WHERE table_schema = 'public' AND table_name = '${table}'
    ORDER BY ordinal_position" | tr -d '\r')"
  result=""
  while IFS= read -r col; do
    col="${col//$'\r'/}"
    [[ -z "${col}" ]] && continue
    if echo "${src_cols}" | grep -Fxq "${col}"; then
      if [[ "${first}" -eq 0 ]]; then
        result+=", "
      fi
      result+="${col}"
      first=0
    fi
  done <<< "${tgt_cols}"
  echo "${result}"
}

table_exists() {
  local db="$1"
  local table="$2"
  [[ -n "$(psql_scalar "${db}" "SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = '${table}' LIMIT 1;")" ]]
}

# INSERT ... SELECT across databases (connected to postgres), only rows whose id is not already on target.
merge_rows() {
  local table="$1"
  local src_where="$2"
  local cols inserted
  if ! table_exists "${SRC_DB}" "${table}"; then
    echo "  skip ${table} (not in ${SRC_DB})"
    return 0
  fi
  cols="$(get_common_columns "${table}")"
  if [[ -z "${cols}" ]]; then
    echo "  skip ${table} (no shared columns)"
    return 0
  fi
  echo "  merge ${table} ..."
  inserted="$(psql_scalar postgres "
    WITH ins AS (
      INSERT INTO ${TARGET_DB}.public.${table} (${cols})
      SELECT ${cols} FROM ${SRC_DB}.public.${table} s
      WHERE ${src_where}
        AND NOT EXISTS (
          SELECT 1 FROM ${TARGET_DB}.public.${table} t WHERE t.id = s.id
        )
      RETURNING 1
    )
    SELECT COUNT(*)::text FROM ins;
  ")"
  echo "    inserted: ${inserted:-0}"
}

if [[ "${SKIP_SRC_RESTORE}" != "1" ]]; then
  echo "==> Load dump into ${SRC_DB}"
  docker cp "$DUMP_PATH" "${PG_CONTAINER}:/tmp/merge_company.dump"
  psql_cmd postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '${SRC_DB}' AND pid <> pg_backend_pid();" >/dev/null || true
  psql_cmd postgres -c "DROP DATABASE IF EXISTS ${SRC_DB};"
  psql_cmd postgres -c "CREATE DATABASE ${SRC_DB};"
  docker exec "$PG_CONTAINER" pg_restore -U "$PG_USER" -d "$SRC_DB" --no-owner --role="$PG_USER" /tmp/merge_company.dump 2>&1 | tail -20 || true
else
  echo "==> Using existing ${SRC_DB} (SKIP_SRC_RESTORE=1)"
fi

if [[ -n "$COMPANY_ID" ]]; then
  SRC_CID="$COMPANY_ID"
else
  SRC_CID="$(psql_scalar "$SRC_DB" "SELECT id::text FROM companies WHERE lower(name) LIKE lower('%${COMPANY_NAME}%') ORDER BY created_at LIMIT 1;")"
fi
if [[ -z "${SRC_CID}" ]]; then
  echo "ERROR: Company not found in ${SRC_DB}."
  exit 1
fi

SRC_NAME="$(psql_scalar "$SRC_DB" "SELECT name FROM companies WHERE id = '${SRC_CID}'::uuid;")"
echo "==> Merge company: ${SRC_NAME} (${SRC_CID})"
echo "==> Target: ${TARGET_DB} (existing rows are kept; only missing ids are inserted)"

TGT_HAS_COMPANY="$(psql_scalar "$TARGET_DB" "SELECT 1 FROM companies WHERE id = '${SRC_CID}'::uuid;")"
if [[ -z "${TGT_HAS_COMPANY}" ]]; then
  echo "==> Company row missing on target; inserting company first"
  merge_rows companies "s.id = '${SRC_CID}'::uuid"
fi

CID="'${SRC_CID}'::uuid"
CAMP_IN="s.campaign_id IN (SELECT id FROM ${SRC_DB}.public.campaigns WHERE company_id = ${CID})"
CL_IN="s.campaign_lead_id IN (
  SELECT cl.id FROM ${SRC_DB}.public.campaign_leads cl
  JOIN ${SRC_DB}.public.campaigns c ON c.id = cl.campaign_id
  WHERE c.company_id = ${CID}
)"

echo "==> Merge rows (no deletes)"
merge_rows senders "s.company_id = ${CID}"
merge_rows leads "s.company_id = ${CID}"
merge_rows lead_collections "s.company_id = ${CID}"
merge_rows lead_assignments "s.lead_collection_id IN (SELECT id FROM ${SRC_DB}.public.lead_collections WHERE company_id = ${CID})"
merge_rows campaigns "s.company_id = ${CID}"
merge_rows campaign_lead_collections "${CAMP_IN}"
merge_rows campaign_leads "${CAMP_IN}"
merge_rows received_messages "${CL_IN}"
if table_exists "${SRC_DB}" "viber_delivery_events"; then
  merge_rows viber_delivery_events "${CL_IN}"
fi
if table_exists "${SRC_DB}" "gpt_requests"; then
  merge_rows gpt_requests "s.campaign_id IN (SELECT id FROM ${SRC_DB}.public.campaigns WHERE company_id = ${CID})"
fi
merge_rows api_messages "s.company_id = ${CID}"
merge_rows test_messages "s.company_id = ${CID}"

echo "  merge application_user_companies ..."
AUC_COLS="$(get_common_columns application_user_companies)"
if [[ -n "${AUC_COLS}" ]]; then
  AUC_INSERTED="$(psql_scalar postgres "
    WITH ins AS (
      INSERT INTO ${TARGET_DB}.public.application_user_companies (${AUC_COLS})
      SELECT ${AUC_COLS} FROM ${SRC_DB}.public.application_user_companies s
      WHERE s.company_id = ${CID}
        AND s.application_user_id IN (SELECT id FROM ${TARGET_DB}.public.\"AspNetUsers\")
        AND NOT EXISTS (
          SELECT 1 FROM ${TARGET_DB}.public.application_user_companies t WHERE t.id = s.id
        )
      RETURNING 1
    )
    SELECT COUNT(*)::text FROM ins;
  ")"
  echo "    inserted: ${AUC_INSERTED:-0} (links only for users that exist on target)"
else
  echo "    skip application_user_companies (no shared columns)"
fi

echo "==> Fix viber_message_id sequence"
psql_cmd "$TARGET_DB" -c "SELECT setval(pg_get_serial_sequence('campaign_leads', 'viber_message_id'), COALESCE((SELECT MAX(viber_message_id) FROM campaign_leads), 1));"

echo "==> Verification on ${TARGET_DB}"
psql_cmd "$TARGET_DB" -c "
SELECT 'campaigns' AS what, COUNT(*) FROM campaigns WHERE company_id = '${SRC_CID}'::uuid
UNION ALL SELECT 'lead_collections', COUNT(*) FROM lead_collections WHERE company_id = '${SRC_CID}'::uuid
UNION ALL SELECT 'leads', COUNT(*) FROM leads WHERE company_id = '${SRC_CID}'::uuid
UNION ALL SELECT 'lead_assignments', COUNT(*) FROM lead_assignments WHERE lead_collection_id IN (
  SELECT id FROM lead_collections WHERE company_id = '${SRC_CID}'::uuid)
UNION ALL SELECT 'campaign_leads', COUNT(*) FROM campaign_leads WHERE campaign_id IN (
  SELECT id FROM campaigns WHERE company_id = '${SRC_CID}'::uuid);
"

if [[ "${SKIP_SRC_RESTORE}" != "1" ]]; then
  echo "==> Drop temporary database ${SRC_DB}"
  psql_cmd postgres -c "DROP DATABASE IF EXISTS ${SRC_DB};"
fi

echo "Done. No duplicate ids inserted; campaigns/leads already on target were not removed or updated."
