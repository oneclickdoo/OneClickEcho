#Requires -Version 5.1
<#
.SYNOPSIS
  Release build: API + Dashboard together (required when JSON contract changes).

.DESCRIPTION
  Analytics and similar features add fields to API responses; the dashboard expects them.
  Always publish both artifacts in the same release window (API first or same pipeline stage).

.PARAMETER SkipDashboard
  Only dotnet publish API (e.g. hotfix API-only — avoid if frontend depends on new fields).

.PARAMETER SkipApi
  Only npm run build dashboard.
#>
param(
    [switch] $SkipDashboard,
    [switch] $SkipApi
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path (Join-Path $root "OneClickEcho.Api\OneClickEcho.Api.csproj"))) {
    Write-Error "Run this script from repo root (OneClickEcho.Api not found under $root)."
}

Set-Location $root
Write-Host "Repo root: $root" -ForegroundColor Cyan

if (-not $SkipApi) {
    Write-Host "`n=== dotnet publish API (Release) ===" -ForegroundColor Cyan
    $out = Join-Path $root "artifacts\api-publish"
    dotnet publish (Join-Path $root "OneClickEcho.Api\OneClickEcho.Api.csproj") -c Release -o $out
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Write-Host "API output: $out" -ForegroundColor Green
}

if (-not $SkipDashboard) {
    Write-Host "`n=== npm run build Dashboard ===" -ForegroundColor Cyan
    $dash = Join-Path $root "OneClickEcho.Dashboard"
    Push-Location $dash
    $nextCmd = Join-Path $dash "node_modules\.bin\next.cmd"
    if (-not (Test-Path $nextCmd)) {
        npm install
        if ($LASTEXITCODE -ne 0) { Pop-Location; exit $LASTEXITCODE }
    }
    npm run build
    if ($LASTEXITCODE -ne 0) { Pop-Location; exit $LASTEXITCODE }
    Pop-Location
    Write-Host "Dashboard .next: $(Join-Path $dash '.next')" -ForegroundColor Green
}

Write-Host "`nDone. Deploy API and Dashboard together when response shapes change." -ForegroundColor Yellow
