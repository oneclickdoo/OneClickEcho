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
#
# Merge only up to a campaign date (old server dump), e.g. Immunoo Flex copies through 2026-01-23:
#   COMPANY_NAME=biosvet DUMP_PATH=/root/oneclickecho.dump \
#   CAMPAIGN_CREATED_BEFORE='2026-01-23 08:45:51' \
#   bash ./scripts/merge-company-from-dump.sh
#
# Or resolve cutoff from campaign name in the dump:
#   CAMPAIGN_UNTIL_NAME='Immunoo Flex - Copy - Copy - Copy - Copy - Copy - Copy' ...

set -euo pipefail

COMPANY_NAME="${COMPANY_NAME:-biosvet}"
COMPANY_ID="${COMPANY_ID:-}"
PG_CONTAINER="${PG_CONTAINER:-oneclick_postgres}"
PG_USER="${PG_USER:-oneclickecho_admin}"
TARGET_DB="${TARGET_DB:-oneclickecho}"
SRC_DB="${SRC_DB:-oneclickecho_src}"
DUMP_PATH="${DUMP_PATH:-/root/oneclickecho.dump}"
SKIP_SRC_RESTORE="${SKIP_SRC_RESTORE:-0}"
# Inclusive upper bound on campaigns.created_at (and leads / lead_collections with same cutoff).
CAMPAIGN_CREATED_BEFORE="${CAMPAIGN_CREATED_BEFORE:-}"
# If set without CAMPAIGN_CREATED_BEFORE, cutoff = created_at of this campaign name in the dump.
CAMPAIGN_UNTIL_NAME="${CAMPAIGN_UNTIL_NAME:-}"

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

# Export from SRC, stage on TARGET in one session, INSERT only ids that do not exist yet.
merge_rows() {
  local table="$1"
  local src_where="$2"
  local cols inserted tmp="/tmp/merge_${table}.csv"
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
  docker exec "$PG_CONTAINER" psql -U "$PG_USER" -d "$SRC_DB" -c "\\copy (SELECT ${cols} FROM ${table} WHERE ${src_where}) TO '${tmp}' WITH (FORMAT csv, HEADER true)"
  inserted="$(docker exec -i "$PG_CONTAINER" psql -v ON_ERROR_STOP=1 -U "$PG_USER" -d "$TARGET_DB" -tA <<EOSQL | tail -1
BEGIN;
CREATE TEMP TABLE merge_stage AS SELECT ${cols} FROM ${table} WHERE false;
COPY merge_stage FROM '${tmp}' WITH (FORMAT csv, HEADER true);
WITH ins AS (
  INSERT INTO ${table} (${cols})
  SELECT ${cols} FROM merge_stage s
  WHERE NOT EXISTS (SELECT 1 FROM ${table} t WHERE t.id = s.id)
  RETURNING 1
)
SELECT COUNT(*)::text FROM ins;
COMMIT;
EOSQL
)"
  docker exec "$PG_CONTAINER" rm -f "$tmp"
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

if [[ -n "${CAMPAIGN_UNTIL_NAME}" && -z "${CAMPAIGN_CREATED_BEFORE}" ]]; then
  CAMPAIGN_CREATED_BEFORE="$(psql_scalar "$SRC_DB" "
    SELECT created_at::text FROM campaigns
    WHERE company_id = '${SRC_CID}'::uuid AND name = '${CAMPAIGN_UNTIL_NAME//\'/''}'
    ORDER BY created_at DESC LIMIT 1;")"
  if [[ -z "${CAMPAIGN_CREATED_BEFORE}" ]]; then
    echo "ERROR: Campaign '${CAMPAIGN_UNTIL_NAME}' not found in ${SRC_DB}."
    psql_cmd "$SRC_DB" -c "SELECT name, created_at FROM campaigns WHERE company_id = '${SRC_CID}'::uuid AND name ILIKE '%Immunoo%' ORDER BY created_at;"
    exit 1
  fi
  echo "==> Cutoff from campaign name: ${CAMPAIGN_CREATED_BEFORE}"
fi

CID="'${SRC_CID}'::uuid"
CUTOFF_SQL=""
if [[ -n "${CAMPAIGN_CREATED_BEFORE}" ]]; then
  CUTOFF_SQL=" AND created_at <= TIMESTAMP '${CAMPAIGN_CREATED_BEFORE}'"
  echo "==> Campaign filter: created_at <= ${CAMPAIGN_CREATED_BEFORE} (inclusive)"
  psql_cmd "$SRC_DB" -c "
    SELECT COUNT(*) AS campaigns_in_dump_in_range FROM campaigns
    WHERE company_id = '${SRC_CID}'::uuid${CUTOFF_SQL};
    SELECT name, created_at FROM campaigns
    WHERE company_id = '${SRC_CID}'::uuid${CUTOFF_SQL}
    ORDER BY created_at DESC LIMIT 5;
  "
fi

TGT_HAS_COMPANY="$(psql_scalar "$TARGET_DB" "SELECT 1 FROM companies WHERE id = '${SRC_CID}'::uuid;")"
if [[ -z "${TGT_HAS_COMPANY}" ]]; then
  echo "==> Company row missing on target; inserting company first"
  merge_rows companies "id = '${SRC_CID}'::uuid"
fi

CAMP_WHERE="company_id = ${CID}${CUTOFF_SQL}"
CAMP_IN="campaign_id IN (SELECT id FROM campaigns WHERE ${CAMP_WHERE})"
CL_IN="campaign_lead_id IN (
  SELECT cl.id FROM campaign_leads cl
  JOIN campaigns c ON c.id = cl.campaign_id
  WHERE ${CAMP_WHERE}
)"
ENTITY_CUTOFF=""
if [[ -n "${CAMPAIGN_CREATED_BEFORE}" ]]; then
  ENTITY_CUTOFF=" AND created_at <= TIMESTAMP '${CAMPAIGN_CREATED_BEFORE}'"
fi

echo "==> Merge rows (no deletes)"
merge_rows senders "company_id = ${CID}"
merge_rows leads "company_id = ${CID}${ENTITY_CUTOFF}"
merge_rows lead_collections "company_id = ${CID}${ENTITY_CUTOFF}"
merge_rows lead_assignments "lead_collection_id IN (SELECT id FROM lead_collections WHERE company_id = ${CID}${ENTITY_CUTOFF})"
merge_rows campaigns "${CAMP_WHERE}"
merge_rows campaign_lead_collections "${CAMP_IN}"
merge_rows campaign_leads "${CAMP_IN}"
merge_rows received_messages "${CL_IN}"
if table_exists "${SRC_DB}" "viber_delivery_events"; then
  merge_rows viber_delivery_events "${CL_IN}"
fi
if table_exists "${SRC_DB}" "gpt_requests"; then
  merge_rows gpt_requests "${CAMP_IN}"
fi
merge_rows api_messages "company_id = ${CID}"
merge_rows test_messages "company_id = ${CID}"

echo "  merge application_user_companies ..."
AUC_COLS="$(get_common_columns application_user_companies)"
TGT_USER_IN="$(psql_cmd "$TARGET_DB" -tAc "SELECT coalesce(string_agg(quote_literal(id::text), ', '), '') FROM \"AspNetUsers\"" | tr -d '\r')"
if [[ -n "${AUC_COLS}" && -n "${TGT_USER_IN}" && "${TGT_USER_IN}" != "NULL" ]]; then
  merge_rows application_user_companies "company_id = ${CID} AND application_user_id IN (${TGT_USER_IN})"
  echo "    (only users that already exist on target)"
elif [[ -z "${TGT_USER_IN}" || "${TGT_USER_IN}" == "NULL" ]]; then
  echo "    skip application_user_companies (no AspNetUsers on target)"
else
  echo "    skip application_user_companies (no shared columns)"
fi

echo "==> Fix viber_message_id sequence"
psql_cmd "$TARGET_DB" -c "SELECT setval(pg_get_serial_sequence('campaign_leads', 'viber_message_id'), COALESCE((SELECT MAX(viber_message_id) FROM campaign_leads), 1));"

echo "==> Verification on ${TARGET_DB}"
VERIFY_CUTOFF=""
if [[ -n "${CAMPAIGN_CREATED_BEFORE}" ]]; then
  VERIFY_CUTOFF=" AND created_at <= TIMESTAMP '${CAMPAIGN_CREATED_BEFORE}'"
fi
psql_cmd "$TARGET_DB" -c "
SELECT 'campaigns' AS what, COUNT(*) FROM campaigns WHERE company_id = '${SRC_CID}'::uuid${VERIFY_CUTOFF}
UNION ALL SELECT 'campaigns_total', COUNT(*) FROM campaigns WHERE company_id = '${SRC_CID}'::uuid
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
