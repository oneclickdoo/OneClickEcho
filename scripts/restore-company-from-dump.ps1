# Windows: selective restore of one company (default biosvet) from pg custom dump.
# Example:
#   $env:PG_CONTAINER = "oneclickecho.postgres17"
#   $env:PG_USER = "postgres"
#   $env:DUMP_PATH = "C:\Users\Admin\Desktop\oneclickecho.dump"
#   .\scripts\restore-company-from-dump.ps1

param(
    [string]$CompanyId = $(if ($env:COMPANY_ID) { $env:COMPANY_ID } else { "" }),
    [string]$CompanyName = $(if ($env:COMPANY_NAME) { $env:COMPANY_NAME } else { "biosvet" }),
    [string]$PgContainer = $(if ($env:PG_CONTAINER) { $env:PG_CONTAINER } else { "oneclickecho.postgres17" }),
    [string]$PgUser = $(if ($env:PG_USER) { $env:PG_USER } else { "postgres" }),
    [string]$TargetDb = $(if ($env:TARGET_DB) { $env:TARGET_DB } else { "oneclickecho" }),
    [string]$SrcDb = $(if ($env:SRC_DB) { $env:SRC_DB } else { "oneclickecho_src" }),
    [string]$DumpPath = $(if ($env:DUMP_PATH) { $env:DUMP_PATH } else { "C:\root\oneclickecho.dump" })
)

$ErrorActionPreference = "Stop"

function Invoke-PsqlScalar([string]$Db, [string]$Sql) {
    $out = docker exec -i $PgContainer psql -U $PgUser -d $Db -tAc $Sql
    return ($out -replace "`r", "").Trim()
}

function Get-CommonColumns([string]$Table) {
    $tgtCols = @(docker exec -i $PgContainer psql -U $PgUser -d $TargetDb -tAc "
        SELECT column_name FROM information_schema.columns
        WHERE table_schema = 'public' AND table_name = '$Table'
        ORDER BY ordinal_position" | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    $srcSet = [System.Collections.Generic.HashSet[string]]::new(
        [string[]](docker exec -i $PgContainer psql -U $PgUser -d $SrcDb -tAc "
            SELECT column_name FROM information_schema.columns
            WHERE table_schema = 'public' AND table_name = '$Table'
            ORDER BY ordinal_position" | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    )
    $common = [System.Collections.Generic.List[string]]::new()
    foreach ($col in $tgtCols) {
        if ($srcSet.Contains($col)) { $common.Add($col) }
    }
    if ($common.Count -eq 0) { return $null }
    return ($common -join ", ")
}

function Test-TableExists([string]$Db, [string]$Table) {
    return [bool](Invoke-PsqlScalar $Db "SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = '$Table' LIMIT 1;")
}

function Copy-TableRows([string]$Table, [string]$Where) {
    if (-not (Test-TableExists $SrcDb $Table)) {
        Write-Host "  skip $Table (table not in dump / $SrcDb)"
        return
    }
    $cols = Get-CommonColumns $Table
    if (-not $cols) {
        Write-Host "  skip $Table (no shared columns between $SrcDb and $TargetDb)"
        return
    }
    $tmp = "/tmp/restore_$Table.csv"
    Write-Host "  copy $Table (shared columns only) ..."
    docker exec $PgContainer psql -U $PgUser -d $SrcDb -c "\copy (SELECT $cols FROM ${Table} WHERE ${Where}) TO '${tmp}' WITH (FORMAT csv, HEADER true)"
    docker exec $PgContainer psql -U $PgUser -d $TargetDb -c "DELETE FROM ${Table} WHERE ${Where}"
    docker exec $PgContainer psql -U $PgUser -d $TargetDb -c "\copy ${Table} ($cols) FROM '${tmp}' WITH (FORMAT csv, HEADER true)"
    docker exec $PgContainer rm -f $tmp
}

if (-not (Test-Path $DumpPath)) {
    throw "Dump not found: $DumpPath"
}

Write-Host "==> Copy dump into container"
docker cp $DumpPath "${PgContainer}:/tmp/oneclickecho.dump"

Write-Host "==> Create temp DB and restore"
docker exec $PgContainer psql -U $PgUser -d postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname IN ('$SrcDb', '$TargetDb') AND pid <> pg_backend_pid();" | Out-Null
docker exec $PgContainer psql -U $PgUser -d postgres -c "DROP DATABASE IF EXISTS $SrcDb;"
docker exec $PgContainer psql -U $PgUser -d postgres -c "CREATE DATABASE $SrcDb;"
docker exec $PgContainer pg_restore -U $PgUser -d $SrcDb --no-owner --no-privileges /tmp/oneclickecho.dump 2>&1 | Select-Object -Last 15

if ($CompanyId) {
    $srcCid = $CompanyId
    $exists = Invoke-PsqlScalar $SrcDb "SELECT 1 FROM companies WHERE id = '$srcCid'::uuid;"
    if (-not $exists) { throw "COMPANY_ID $srcCid not found in dump." }
} else {
    $srcCid = Invoke-PsqlScalar $SrcDb "SELECT id::text FROM companies WHERE lower(name) LIKE lower('%$CompanyName%') ORDER BY created_at LIMIT 1;"
    if (-not $srcCid) {
        docker exec $PgContainer psql -U $PgUser -d $SrcDb -c "SELECT id, name FROM companies ORDER BY name;"
        throw "Company '$CompanyName' not found in dump."
    }
}
$srcName = Invoke-PsqlScalar $SrcDb "SELECT name FROM companies WHERE id = '$srcCid'::uuid;"
Write-Host "==> Source company: $srcName ($srcCid)"

if ($CompanyId) {
    Write-Host "==> Delete scope: ONLY company id $srcCid (Biosvet and other companies are not touched)"
    $campaignCompanyWhere = "c.company_id = '$srcCid'::uuid"
    $directCompanyWhere = "company_id = '$srcCid'::uuid"
    $companiesWhere = "id = '$srcCid'::uuid"
} else {
    Write-Host "==> Delete scope: companies matching name '%$CompanyName%'"
    $campaignCompanyWhere = "c.company_id IN (SELECT id FROM companies WHERE lower(name) LIKE lower('%$CompanyName%'))"
    $directCompanyWhere = "company_id IN (SELECT id FROM companies WHERE lower(name) LIKE lower('%$CompanyName%'))"
    $companiesWhere = "lower(name) LIKE lower('%$CompanyName%')"
}

$deleteSql = @"
DELETE FROM viber_delivery_events WHERE campaign_lead_id IN (
  SELECT cl.id FROM campaign_leads cl JOIN campaigns c ON c.id = cl.campaign_id WHERE $campaignCompanyWhere);
DELETE FROM received_messages WHERE campaign_lead_id IN (
  SELECT cl.id FROM campaign_leads cl JOIN campaigns c ON c.id = cl.campaign_id WHERE $campaignCompanyWhere);
DELETE FROM campaign_leads WHERE campaign_id IN (SELECT id FROM campaigns WHERE $directCompanyWhere);
DELETE FROM campaign_lead_collections WHERE campaign_id IN (SELECT id FROM campaigns WHERE $directCompanyWhere);
DELETE FROM gpt_requests WHERE campaign_id IN (SELECT id FROM campaigns WHERE $directCompanyWhere);
DELETE FROM campaigns WHERE $directCompanyWhere;
DELETE FROM lead_assignments WHERE lead_collection_id IN (SELECT id FROM lead_collections WHERE $directCompanyWhere);
DELETE FROM lead_collections WHERE $directCompanyWhere;
DELETE FROM leads WHERE $directCompanyWhere;
DELETE FROM api_messages WHERE $directCompanyWhere;
DELETE FROM test_messages WHERE $directCompanyWhere;
DELETE FROM senders WHERE $directCompanyWhere;
DELETE FROM application_user_companies WHERE $directCompanyWhere;
DELETE FROM companies WHERE $companiesWhere;
"@

Write-Host "==> Delete old $srcName rows in target"
docker exec $PgContainer psql -U $PgUser -d $TargetDb -c $deleteSql

Copy-TableRows "companies" "id = '$srcCid'::uuid"
Copy-TableRows "senders" "company_id = '$srcCid'::uuid"
Copy-TableRows "leads" "company_id = '$srcCid'::uuid"
Copy-TableRows "lead_collections" "company_id = '$srcCid'::uuid"
Copy-TableRows "lead_assignments" "lead_collection_id IN (SELECT id FROM lead_collections WHERE company_id = '$srcCid'::uuid)"
Copy-TableRows "campaigns" "company_id = '$srcCid'::uuid"
Copy-TableRows "campaign_lead_collections" "campaign_id IN (SELECT id FROM campaigns WHERE company_id = '$srcCid'::uuid)"
Copy-TableRows "campaign_leads" "campaign_id IN (SELECT id FROM campaigns WHERE company_id = '$srcCid'::uuid)"
Copy-TableRows "received_messages" "campaign_lead_id IN (SELECT cl.id FROM campaign_leads cl JOIN campaigns c ON c.id = cl.campaign_id WHERE c.company_id = '$srcCid'::uuid)"
Copy-TableRows "viber_delivery_events" "campaign_lead_id IN (SELECT cl.id FROM campaign_leads cl JOIN campaigns c ON c.id = cl.campaign_id WHERE c.company_id = '$srcCid'::uuid)"
Copy-TableRows "api_messages" "company_id = '$srcCid'::uuid"
Copy-TableRows "test_messages" "company_id = '$srcCid'::uuid"
$inList = Invoke-PsqlScalar $TargetDb "SELECT string_agg(quote_literal(id::text), ', ') FROM ""AspNetUsers"";"
if (-not $inList) {
    Write-Host "  skip application_user_companies (no AspNetUsers on $TargetDb)"
    docker exec $PgContainer psql -U $PgUser -d $TargetDb -c "DELETE FROM application_user_companies WHERE company_id = '$srcCid'::uuid;" | Out-Null
} else {
    $srcCount = [int](Invoke-PsqlScalar $SrcDb "SELECT COUNT(*) FROM application_user_companies WHERE company_id = '$srcCid'::uuid;")
    $copyCount = [int](Invoke-PsqlScalar $SrcDb "SELECT COUNT(*) FROM application_user_companies WHERE company_id = '$srcCid'::uuid AND application_user_id IN ($inList);")
    if ($srcCount -gt $copyCount) {
        Write-Host "  note: $($srcCount - $copyCount) user link(s) skipped (user not on this server)."
    }
    Copy-TableRows "application_user_companies" "company_id = '$srcCid'::uuid AND application_user_id IN ($inList)"
}

docker exec $PgContainer psql -U $PgUser -d $TargetDb -c "SELECT setval(pg_get_serial_sequence('campaign_leads', 'viber_message_id'), COALESCE((SELECT MAX(viber_message_id) FROM campaign_leads), 1));"
docker exec $PgContainer psql -U $PgUser -d $TargetDb -c "SELECT name, sms_username FROM companies WHERE id = '$srcCid'::uuid; SELECT COUNT(*) AS campaigns FROM campaigns WHERE company_id = '$srcCid'::uuid; SELECT COUNT(*) AS leads FROM leads WHERE company_id = '$srcCid'::uuid;"

docker exec $PgContainer psql -U $PgUser -d postgres -c "DROP DATABASE IF EXISTS $SrcDb;"
Write-Host "Done. Company credentials (sms_username, sms_password, api_password) are now from the dump."
